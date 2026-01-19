using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip fireSound;     // file audio (.wav o .mp3)

    [Header("Fire Rate")]
    [Tooltip("Rounds Per Minute (600 = AK-47 style full-auto)")]
    public float fireRate = 600f;
    private float fireInterval;        // Calculated in Start()
    private float nextTimeToFire = 0f;

    [Header("Ammo System")]
    [Tooltip("Maximum ammo capacity")]
    public int maxAmmo = 30;
    [Tooltip("Current ammo in magazine")]
    public int currentAmmo;
    [Tooltip("Reload duration in seconds")]
    public float reloadTime = 2f;
    [Tooltip("Is the gun currently reloading?")]
    private bool isReloading = false;

    [Header("Weapon Shake Settings")]
    [Tooltip("Weapon shake intensity (position offset)")]
    public float weaponShakeAmount = 0.02f;
    [Tooltip("Weapon rotation kick on shot (degrees)")]
    public float weaponKickRotation = 2f;
    [Tooltip("How fast weapon returns to origin")]
    public float weaponReturnSpeed = 15f;
    [Tooltip("Reference to weapon transform (gun model)")]
    public Transform weaponTransform;

    [Header("Animation")]
    [Tooltip("Animator component for reload animation (on Paintball_Maker)")]
    public Animator weaponAnimator;

    private Vector3 weaponOriginalPosition;
    private Quaternion weaponOriginalRotation;
    private Vector3 weaponCurrentOffset = Vector3.zero;
    private Quaternion weaponCurrentRotationOffset = Quaternion.identity;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;

    [Header("Damage Settings")]
    [Tooltip("Danno inflitto per ogni colpo")]
    public float damagePerShot = 25f;

    [Header("Impact VFX")]
    [Tooltip("Prefab paint explosion quando colpisci enemy (cyan spheres burst)")]
    public GameObject enemyHitVfxPrefab;
    [Tooltip("Prefab kill effect (skull + numero)")]
    public GameObject enemyKillVfxPrefab;

    // riferimento al prefab del proiettile
    [SerializeField] private GameObject projectilePrefab;

    // Un Transform per definire da dove spara la pistola (un oggetto vuoto)
    public Transform firePoint;

    [Header("Raycast Settings")]
    [Tooltip("Layer per il raycast di AIMING (ignora Enemy per evitare bug di mira su collider curvi)")]
    public LayerMask aimingIgnoreMask;

    [Tooltip("Layer per il raycast di DAMAGE (cosa può essere danneggiato)")]
    public LayerMask damageLayerMask;

    [Header("Kill Attribution")]
    [Tooltip("Reference to the Player GameObject (for kill tracking). Leave empty to auto-detect.")]
    public GameObject ownerPlayer;

    [Tooltip("Distanza minima valida per un hit. Se colpisce più vicino, usa fallback.")]
    public float minHitDistance = 2.5f;

    [Tooltip("Distanza massima del raycast")]
    public float maxRaycastDistance = 500f;

    [Header("Debug")]
    [Tooltip("Mostra log dettagliati dello shooting in Console")]
    public bool debugMode = false;

    void Start()
    {
        // Calculate fire interval from RPM
        fireInterval = 60f / fireRate;  // 600 RPM → 0.1 seconds

        // Initialize ammo
        currentAmmo = maxAmmo;

        // Save weapon original position/rotation for shake
        if (weaponTransform != null)
        {
            weaponOriginalPosition = weaponTransform.localPosition;
            weaponOriginalRotation = weaponTransform.localRotation;
        }

        // Disable animator initially (only enable during reload)
        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = false;
        }

        // Auto-detect player if not assigned (find Player tag or parent)
        if (ownerPlayer == null)
        {
            // Try to find Player tag
            ownerPlayer = GameObject.FindGameObjectWithTag("Player");

            // If not found, assume gun is child of player
            if (ownerPlayer == null)
            {
                ownerPlayer = transform.root.gameObject; // Get root parent
            }

            if (debugMode)
            {
                Debug.Log($"[Gun] Auto-detected owner: {ownerPlayer.name}");
            }
        }

        if (debugMode)
        {
            Debug.Log($"[Gun] Fire Rate: {fireRate} RPM, Interval: {fireInterval:F3}s ({1f/fireInterval:F1} rounds/sec)");
        }
    }

    void Update()
    {
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
        {
            RecoverWeaponPosition();
            return;
        }

        PlayerHealth playerHealth = ownerPlayer?.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead())
        {
            RecoverWeaponPosition();
            return;
        }

        if (MatchManager.Instance != null && !MatchManager.Instance.IsMatchActive)
        {
            RecoverWeaponPosition();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && !isReloading)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + fireInterval;
                Shoot();
            }
            else
            {
                // Auto-reload when empty
                if (!isReloading)
                {
                    StartCoroutine(Reload());
                }
            }
        }

        RecoverWeaponPosition();
    }

    void Shoot()
    {
        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        ApplyWeaponShake();

        // Crea un raggio dal centro esatto della telecamera
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit aimHit;
        Vector3 targetPoint;

        int aimLayerMask = ~aimingIgnoreMask.value;

        if (debugMode)
        {
            Debug.Log($"[Gun] Camera Position: {fpsCam.transform.position}, Forward: {fpsCam.transform.forward}");
            Debug.Log($"[Gun] FirePoint Position: {firePoint.position}");
        }

        // Raycast per calcolare targetPoint (direzione visuale)
        if (Physics.Raycast(ray, out aimHit, maxRaycastDistance, aimLayerMask))
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] AIM Raycast HIT: {aimHit.collider.name} a distanza {aimHit.distance}m, layer: {LayerMask.LayerToName(aimHit.collider.gameObject.layer)}");
            }

            if (aimHit.distance >= minHitDistance)
            {
                // Hit valido: usa il punto d'impatto
                targetPoint = aimHit.point;
            }
            else
            {
                // Hit troppo vicino: usa il fallback (spara dritto in avanti)
                targetPoint = ray.GetPoint(100);
                Debug.LogWarning($"[Gun] Aim hit troppo vicino ({aimHit.distance:F2}m su {aimHit.collider.name}). Usando fallback.");
            }
        }
        else
        {
            // Se non colpisce nulla (es. il cielo),
            // usa un punto lontano lungo la direzione del raggio
            targetPoint = ray.GetPoint(100);

            if (debugMode)
            {
                Debug.Log($"[Gun] AIM Raycast NO HIT. Usando fallback targetPoint: {targetPoint}");
            }
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        RaycastHit[] allHits = Physics.RaycastAll(ray, maxRaycastDistance, damageLayerMask);

        RaycastHit validDamageHit = new RaycastHit();
        bool foundValidHit = false;

        foreach (RaycastHit potentialHit in allHits)
        {
            // Skip if too close
            if (potentialHit.distance < minHitDistance)
                continue;

            // Skip if it's a SphereCollider (AI detection, not hitbox)
            if (potentialHit.collider is SphereCollider)
            {
                if (debugMode)
                {
                    Debug.Log($"[Gun] Skipping SphereCollider on {potentialHit.collider.name} (AI detection radius)");
                }
                continue;
            }

            // Valid hit found (CapsuleCollider or other solid collider)
            validDamageHit = potentialHit;
            foundValidHit = true;
            break;
        }

        if (foundValidHit)
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] DAMAGE Raycast HIT: {validDamageHit.collider.name} (Type: {validDamageHit.collider.GetType().Name}) a distanza {validDamageHit.distance}m");
            }

            // Applica danno al collider VERO (CapsuleCollider)
            ApplyDamage(validDamageHit);
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] DAMAGE Raycast NO HIT (shot into the void or only hit detection spheres)");
            }
        }

        // Calcola la direzione dal "firePoint" (la canna) al "targetPoint"
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        float angleFromCameraForward = Vector3.Angle(fpsCam.transform.forward, direction);

        if (angleFromCameraForward > 90f)
        {
            // Il target è dietro o troppo laterale (>90°) - usa camera forward
            direction = fpsCam.transform.forward;
            Debug.LogWarning($"[Gun] TargetPoint dietro/laterale (angle {angleFromCameraForward:F1}°)! Usando camera forward.");
        }

        if (debugMode)
        {
            Debug.Log($"[Gun] TargetPoint: {targetPoint}, Direction: {direction}, Angle from forward: {angleFromCameraForward:F2}°");
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * 50f, Color.blue, 2f);
            Debug.DrawLine(firePoint.position, targetPoint, Color.red, 2f);
            Debug.DrawRay(firePoint.position, direction * 50f, Color.green, 2f);
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = fpsCam.transform.forward;
            Debug.LogWarning("[Gun] Direzione invalida! Usando camera forward come fallback.");
        }

        Quaternion projectileRotation = Quaternion.LookRotation(direction);

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                projectileRotation
            ) as GameObject;

            if (debugMode)
            {
                Debug.Log($"[Gun] Visual tracer spawned at {firePoint.position}, direction: {direction}");
            }
        }
        else
        {
            Debug.LogWarning("[Gun] Projectile Prefab o Fire Point non assegnati.");
        }
    }

    private void ApplyDamage(RaycastHit hit)
    {

        IDamagable damagable = hit.collider.GetComponent<IDamagable>();
        if (damagable != null)
        {
            Debug.Log($"[Gun] 🎯 TARGET HIT! Object: {hit.collider.name} at distance {hit.distance:F2}m");

            // Apply damage via IDamagable interface
            damagable.Damage(damagePerShot);

            if (debugMode)
            {
                Debug.Log($"[Gun] Applied {damagePerShot} damage to {hit.collider.name} via IDamagable");
            }

            if (enemyHitVfxPrefab != null)
            {
                GameObject hitVfx = Instantiate(enemyHitVfxPrefab, hit.point, Quaternion.identity);
                Destroy(hitVfx, 2f);
            }

            return;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log($"[Gun] ENEMY HIT! Target: {hit.collider.name} at distance {hit.distance:F2}m");

            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Apply damage with kill attribution
                enemyHealth.TakeDamage(damagePerShot, hit.point, ownerPlayer);

                if (debugMode)
                {
                    Debug.Log($"[Gun] Applied {damagePerShot} damage to {hit.collider.name} (attacker: {ownerPlayer.name})");
                }
            }
            else
            {
                Debug.LogWarning($"[Gun] Enemy hit but no EnemyHealth component found on {hit.collider.name}!");
            }

            // Enemy hit VFX
            if (enemyHitVfxPrefab != null)
            {
                GameObject hitVfx = Instantiate(enemyHitVfxPrefab, hit.point, Quaternion.identity);
                Destroy(hitVfx, 2f);

                if (debugMode)
                {
                    Debug.Log($"[Gun] Enemy hit VFX spawned at {hit.point}");
                }
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] Hit surface: {hit.collider.name} (layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
        }
    }

    private void ApplyWeaponShake()
    {
        if (weaponTransform == null) return;

        // Random position offset (vibration effect)
        float randomX = Random.Range(-weaponShakeAmount, weaponShakeAmount);
        float randomY = Random.Range(-weaponShakeAmount, weaponShakeAmount);
        float randomZ = Random.Range(-weaponShakeAmount * 2f, 0f);

        weaponCurrentOffset = new Vector3(randomX, randomY, randomZ);

        // Rotation kick (slight upward tilt)
        float kickX = Random.Range(-weaponKickRotation, weaponKickRotation * 0.5f); // Mostly up
        float kickY = Random.Range(-weaponKickRotation * 0.3f, weaponKickRotation * 0.3f); // Slight horizontal
        float kickZ = Random.Range(-weaponKickRotation * 0.2f, weaponKickRotation * 0.2f); // Slight roll

        weaponCurrentRotationOffset = Quaternion.Euler(kickX, kickY, kickZ);

        if (debugMode)
        {
            Debug.Log($"[Gun] Weapon shake: offset {weaponCurrentOffset}, rotation {kickX:F1}° X");
        }
    }

    private void RecoverWeaponPosition()
    {
        if (weaponTransform == null) return;

        // Smoothly lerp back to original position
        weaponCurrentOffset = Vector3.Lerp(weaponCurrentOffset, Vector3.zero, Time.deltaTime * weaponReturnSpeed);
        weaponCurrentRotationOffset = Quaternion.Slerp(weaponCurrentRotationOffset, Quaternion.identity, Time.deltaTime * weaponReturnSpeed);

        // Apply to weapon transform
        weaponTransform.localPosition = weaponOriginalPosition + weaponCurrentOffset;
        weaponTransform.localRotation = weaponOriginalRotation * weaponCurrentRotationOffset;
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // Enable animator and force reload animation from start
        if (weaponAnimator != null)
        {
            weaponAnimator.enabled = true;

            // Force play animation from beginning (layer 0, normalized time 0)
            weaponAnimator.Play("Gun_Reload", 0, 0f);
        }

        if (debugMode)
        {
            Debug.Log($"[Gun] Reloading... ({reloadTime}s)");
        }

        yield return new WaitForSeconds(reloadTime);

        // Return to Idle state before disabling
        if (weaponAnimator != null)
        {
            weaponAnimator.Play("Idle", 0, 0f);

            // Wait one frame for state to update
            yield return null;

            weaponAnimator.enabled = false;
        }

        currentAmmo = maxAmmo;
        isReloading = false;

        if (debugMode)
        {
            Debug.Log($"[Gun] Reload complete! Ammo: {currentAmmo}/{maxAmmo}");
        }
    }

    public float GetAmmoFillAmount()
    {
        return (float)currentAmmo / maxAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}