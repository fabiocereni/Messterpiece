using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [Header("Riferimenti")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSound;

    [Header("Impostazioni IA")]
    public float damage = 10f;
    [Tooltip("Assicurati di includere sia 'Player' che 'Enemy' qui!")]
    public LayerMask damageLayerMask;
    public float maxDistance = 100f;

    public void Shoot(Vector3 direction)
    {
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            // Otteniamo la radice del nemico che spara
            GameObject shooter = transform.root.gameObject; 

            // 1. Controllo Player
            PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Rispettiamo la nuova firma: danno, punto, e chi spara
                playerHealth.TakeDamage(damage, hit.point, shooter); 
            }
            // 2. Controllo Altri Nemici
            else 
            {
                EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Rispettiamo la firma esistente di EnemyHealth
                    enemyHealth.TakeDamage(damage, hit.point, shooter); 
                }
            }
        }

        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}