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
        // Esegue il Raycast usando la maschera di layer configurata
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            // 1. Prova a colpire il Player
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
            }
            // 2. Prova a colpire un Enemy (usa lo script EnemyHealth che abbiamo configurato)
            else 
            {
                EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Passiamo il danno, il punto di impatto e 'gameObject' come attaccante
                    enemyHealth.TakeDamage(damage, hit.point, gameObject);
                }
            }
        }

        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}