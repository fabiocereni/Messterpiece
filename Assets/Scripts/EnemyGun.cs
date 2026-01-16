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

    [Tooltip("Regola l'altezza del tiro: valori negativi per mirare più in basso, positivi per più in alto.")]
    public float verticalOffset = -0.1f;

    public void Shoot(Vector3 direction)
    {
        // 1. APPLICA OFFSET: Modifichiamo la direzione verso l'alto o verso il basso
        direction.y += verticalOffset;
        direction = direction.normalized; // Manteniamo il vettore normalizzato dopo la modifica

        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);

        // Calcola danno con moltiplicatore difficoltà
        float actualDamage = damage;
        if (GameSettings.Instance != null)
        {
            actualDamage *= GameSettings.Instance.EnemyDamageMultiplier;
        }

        RaycastHit hit;
        // Usiamo la direzione modificata per il Raycast
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            GameObject shooter = transform.root.gameObject; 

            PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(actualDamage, hit.point, shooter); 
            }
            else 
            {
                EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(actualDamage, hit.point, shooter); 
                }
            }
        }

        if (projectilePrefab != null)
        {
            // Usiamo la direzione modificata anche per la rotazione del proiettile
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}