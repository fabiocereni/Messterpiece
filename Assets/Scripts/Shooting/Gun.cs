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

        // Controlla se il raggio colpisce qualcosa
        if (Physics.Raycast(ray, out hit))
        {
            // Se colpisce, il punto d'impatto è la nostra destinazione
            targetPoint = hit.point;
        }
        else
        {
            // Se non colpisce nulla (es. il cielo),
            // usa un punto lontano lungo la direzione del raggio
            targetPoint = ray.GetPoint(100); // 100 metri in avanti
        }

        // Calcola la direzione dal "firePoint" (la canna) al "targetPoint"
        Vector3 direction = targetPoint - firePoint.position;

        // Calcola la rotazione necessaria per far sì che il proiettile guardi in quella direzione
        Quaternion projectileRotation = Quaternion.LookRotation(direction);

        // Istanzia il proiettile
        // Posizione: nasce dal firePoint
        // Rotazione: usa la nuova rotazione calcolata
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
            Debug.LogWarning("Projectile Prefab o Fire Point non assegnati.");
        }
    }
}