using UnityEngine;

public class Gun : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip fireSound;     // file audio (.wav o .mp3)
    
    [Header("Fire Rate")]
    [Tooltip("Rounds Per Minute (600 = AK-47 style full-auto)")]
    public float fireRate = 600f;
    private float fireInterval;        // Calculated in Start()
    private float nextTimeToFire = 0f;

    [Header("Weapon Shake Settings")]
    [Tooltip("Weapon shake intensity (position offset)")]
    public float weaponShakeAmount = 0.02f;
    [Tooltip("Weapon rotation kick on shot (degrees)")]
    public float weaponKickRotation = 2f;
    [Tooltip("How fast weapon returns to origin")]
    public float weaponReturnSpeed = 15f;
    [Tooltip("Reference to weapon transform (gun model)")]
    public Transform weaponTransform;

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

        // Save weapon original position/rotation for shake
        if (weaponTransform != null)
        {
            weaponOriginalPosition = weaponTransform.localPosition;
            weaponOriginalRotation = weaponTransform.localRotation;
        }

        if (debugMode)
        {
            Debug.Log($"[Gun] Fire Rate: {fireRate} RPM, Interval: {fireInterval:F3}s ({1f/fireInterval:F1} rounds/sec)");
        }
    }

    void Update()
    {
        // ═══════════════════════════════════════════════════════
        // SEMI-AUTO + FULL-AUTO INPUT
        // GetButton (not GetButtonDown) → Allows hold for full-auto
        // Fire rate cooldown prevents too-fast spam
        // ═══════════════════════════════════════════════════════
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireInterval;
            Shoot();
        }

        // ═══════════════════════════════════════════════════════
        // WEAPON SHAKE RECOVERY - Return weapon to original position
        // ═══════════════════════════════════════════════════════
        RecoverWeaponPosition();
    }

    void Shoot()
    {
        // ═══════════════════════════════════════════════════════
        // MUZZLE FLASH (Cyan Cloud)
        // ═══════════════════════════════════════════════════════
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // ═══════════════════════════════════════════════════════
        // WEAPON SHAKE - Vibrate gun on shot
        // ═══════════════════════════════════════════════════════
        ApplyWeaponShake();

        // Crea un raggio dal centro esatto della telecamera
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit aimHit;
        Vector3 targetPoint;

        // ═══════════════════════════════════════════════════════
        // RAYCAST 1: AIMING (Ignora Enemy per evitare bug visuali)
        // ═══════════════════════════════════════════════════════
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

            // FAIL-SAFE: Se colpisce qualcosa troppo vicino (< minHitDistance),
            // probabilmente è il proprio collider o un trigger, quindi IGNORA
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
        
        // ═══════════════════════════════════════════════════════
        // AUDIO: play sound effect
        // ═══════════════════════════════════════════════════════
        
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        

        // ═══════════════════════════════════════════════════════
        // RAYCAST 2: DAMAGE (Include Enemy per applicare danno)
        // IMPORTANT: Filter out SphereColliders (AI detection radius)
        // Only hit CapsuleColliders (true hitbox)
        // ═══════════════════════════════════════════════════════
        RaycastHit[] allHits = Physics.RaycastAll(ray, maxRaycastDistance, damageLayerMask);

        // Filter: Find first valid hit (CapsuleCollider, not SphereCollider)
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

        // CRITICAL FIX: Se il targetPoint è DIETRO il firePoint o troppo laterale,
        // usa la direzione della camera invece (previene proiettili all'indietro)
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

        // SAFETY CHECK: Se la direzione è quasi zero (non dovrebbe mai succedere),
        // usa la direzione forward della camera
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = fpsCam.transform.forward;
            Debug.LogWarning("[Gun] Direzione invalida! Usando camera forward come fallback.");
        }

        // Calcola la rotazione necessaria per far sì che il proiettile guardi in quella direzione
        Quaternion projectileRotation = Quaternion.LookRotation(direction);

        // ═══════════════════════════════════════════════════════
        // STEP 2: Visual Tracer (ENABLED)
        // Spawn "fake bullet" that travels visually
        // Damage is already applied by raycast above!
        // ═══════════════════════════════════════════════════════
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

    /// <summary>
    /// Applies instant damage (Hitscan) to the hit target
    /// </summary>
    private void ApplyDamage(RaycastHit hit)
    {
        // Check if we hit an enemy (layer "Enemy")
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log($"[Gun] 🎯 ENEMY HIT! Target: {hit.collider.name} at distance {hit.distance:F2}m");

            // ═══════════════════════════════════════════════════════
            // APPLY DAMAGE - Get EnemyHealth component and deal damage
            // ═══════════════════════════════════════════════════════
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Apply damage (this will also spawn damage number)
                enemyHealth.TakeDamage(damagePerShot, hit.point);

                if (debugMode)
                {
                    Debug.Log($"[Gun] Applied {damagePerShot} damage to {hit.collider.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[Gun] Enemy hit but no EnemyHealth component found on {hit.collider.name}!");
            }

            // ═══════════════════════════════════════════════════════
            // ENEMY HIT VFX - Spawn paint explosion (cyan spheres)
            // ═══════════════════════════════════════════════════════
            if (enemyHitVfxPrefab != null)
            {
                // Spawn VFX at impact point
                GameObject hitVfx = Instantiate(enemyHitVfxPrefab, hit.point, Quaternion.identity);

                // Auto-destroy after particle lifetime
                Destroy(hitVfx, 2f);

                if (debugMode)
                {
                    Debug.Log($"[Gun] Enemy hit VFX spawned at {hit.point}");
                }
            }
        }
        else
        {
            // Hit something else (wall, obstacle)
            if (debugMode)
            {
                Debug.Log($"[Gun] Hit surface: {hit.collider.name} (layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
        }
    }

    /// <summary>
    /// Apply weapon shake (vibration) on shot
    /// Weapon "pumps" back and rotates slightly
    /// </summary>
    private void ApplyWeaponShake()
    {
        if (weaponTransform == null) return;

        // Random position offset (vibration effect)
        float randomX = Random.Range(-weaponShakeAmount, weaponShakeAmount);
        float randomY = Random.Range(-weaponShakeAmount, weaponShakeAmount);
        float randomZ = Random.Range(-weaponShakeAmount * 2f, 0f); // Backward "pump"

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

    /// <summary>
    /// Smoothly return weapon to original position
    /// Creates smooth "pump" animation
    /// </summary>
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
}