using UnityEngine;

public class Gun : MonoBehaviour
{
    // public float fireRate = 1f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;

    // riferimento al prefab del proiettile
    [SerializeField] private GameObject projectilePrefab;

    // Un Transform per definire da dove spara la pistola (un oggetto vuoto)
    public Transform firePoint;

    [Header("Raycast Settings")]
    [Tooltip("Layer che il raycast di mira DEVE IGNORARE (Player, TriggerZone, etc.)")]
    public LayerMask ignoreLayerMask;

    [Tooltip("Distanza minima valida per un hit. Se colpisce più vicino, usa fallback.")]
    public float minHitDistance = 2.5f;

    [Tooltip("Distanza massima del raycast")]
    public float maxRaycastDistance = 500f;

    [Header("Debug")]
    [Tooltip("Mostra log dettagliati dello shooting in Console")]
    public bool debugMode = false;

    // private float nextTimeToFire = 0f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // nextTimeToFire = Time.time + 1f / fireRate;
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
        RaycastHit hit;
        Vector3 targetPoint;

        // IMPORTANTE: Inverte il LayerMask per ignorare i layer specificati
        int layerMask = ~ignoreLayerMask.value;

        if (debugMode)
        {
            Debug.Log($"[Gun] Camera Position: {fpsCam.transform.position}, Forward: {fpsCam.transform.forward}");
            Debug.Log($"[Gun] FirePoint Position: {firePoint.position}");
        }

        // Controlla se il raggio colpisce qualcosa, IGNORANDO i layer specificati
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, layerMask))
        {
            if (debugMode)
            {
                Debug.Log($"[Gun] Raycast HIT: {hit.collider.name} a distanza {hit.distance}m, point: {hit.point}, layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            }

            // FAIL-SAFE: Se colpisce qualcosa troppo vicino (< minHitDistance),
            // probabilmente è il proprio collider o un trigger, quindi IGNORA
            if (hit.distance >= minHitDistance)
            {
                // Hit valido: usa il punto d'impatto
                targetPoint = hit.point;
            }
            else
            {
                // Hit troppo vicino: usa il fallback (spara dritto in avanti)
                targetPoint = ray.GetPoint(100);
                Debug.LogWarning($"[Gun] Hit troppo vicino ({hit.distance:F2}m su {hit.collider.name}). Usando fallback.");
            }
        }
        else
        {
            // Se non colpisce nulla (es. il cielo),
            // usa un punto lontano lungo la direzione del raggio
            targetPoint = ray.GetPoint(100);

            if (debugMode)
            {
                Debug.Log($"[Gun] Raycast NO HIT. Usando fallback targetPoint: {targetPoint}");
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
    }
}