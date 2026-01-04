using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Fire Rate")]
    [Tooltip("Rounds Per Minute (600 = AK-47 style full-auto)")]
    public float fireRate = 600f;
    private float fireInterval;        // Calculated in Start()
    private float nextTimeToFire = 0f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;

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
    }

    void Shoot()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash?.Play();
        }

        // Crea un raggio dal centro esatto della telecamera
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit aimHit;
        RaycastHit damageHit;
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
        // RAYCAST 2: DAMAGE (Include Enemy per applicare danno)
        // ═══════════════════════════════════════════════════════
        if (Physics.Raycast(ray, out damageHit, maxRaycastDistance, damageLayerMask))
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] DAMAGE Raycast HIT: {damageHit.collider.name} a distanza {damageHit.distance}m, layer: {LayerMask.LayerToName(damageHit.collider.gameObject.layer)}");
            }

            if (damageHit.distance >= minHitDistance)
            {
                // Applica danno istantaneo
                ApplyDamage(damageHit);
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] DAMAGE Raycast NO HIT (shot into the void)");
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
        // STEP 2: Visual Tracer (Temporarily commented out)
        // We'll re-enable this in Step 2 as a "fake bullet" visual
        // that travels while the raycast handles instant damage
        // ═══════════════════════════════════════════════════════
        /*
        // Istanzia il proiettile
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                projectileRotation
            ) as GameObject;
        }
        else
        {
            Debug.LogWarning("[Gun] Projectile Prefab o Fire Point non assegnati.");
        }
        */
    }

    /// <summary>
    /// Applies instant damage (Hitscan) to the hit target
    /// STEP 1: Debug placeholder - full damage system in Step 4
    /// </summary>
    private void ApplyDamage(RaycastHit hit)
    {
        // ═══════════════════════════════════════════════════════
        // STEP 1 PLACEHOLDER: Debug feedback only
        // In Step 4 we'll add real damage + VFX + audio
        // ═══════════════════════════════════════════════════════

        // Check if we hit an enemy (layer "Enemy")
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log($"[Gun] 🎯 ENEMY HIT! Target: {hit.collider.name} at distance {hit.distance:F2}m");

            // TODO (Step 4): Apply real damage to enemy health component
            // TODO (Step 4): Spawn impact VFX (cyan paint cloud)
            // TODO (Step 4): Play hitmarker audio (splash sound)
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
}