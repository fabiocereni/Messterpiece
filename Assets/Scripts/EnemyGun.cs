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
    [Tooltip("Indica chi può essere colpito")]
    public LayerMask damageLayerMask;
    public float maxDistance = 100f;

    [Tooltip("Regolo l'altezza del tiro: valori negativi per mirare più in basso, positivi per più in alto.")]
    public float verticalOffset = -0.1f;

    public void Shoot(Vector3 direction)
    {
        // modifichiamo la direzione verso l'alto o verso il basso
        direction.y += verticalOffset;
        direction = direction.normalized; // manteniamo il vettore normalizzato dopo la modifica

        if (muzzleFlash != null) muzzleFlash.Play(); // effetto visivo del fuoco
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound); // effetto sonoro del fuoco

        // calcolo il danno basandomi anche sulle impostazioni globali del gioco
        float actualDamage = damage;
        if (GameSettings.Instance != null)
        {
            actualDamage *= GameSettings.Instance.EnemyDamageMultiplier;
        }

        RaycastHit hit;
        // eseguo il raycast per verificare se colpisco qualcosa
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            GameObject shooter = transform.root.gameObject; // serve a dire alla vittimia chi l'ha colpita

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
            // creo un proiettile fisico per effetti visivi
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}