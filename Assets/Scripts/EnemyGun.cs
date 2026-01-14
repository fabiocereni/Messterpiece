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
    public LayerMask damageLayerMask;
    public float maxDistance = 100f;

    public void Shoot(Vector3 direction)
    {
        //Riproduco suono
        
        
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            // 1. Cerchiamo lo script PlayerHealth sull'oggetto colpito
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();

            // 2. Se lo troviamo, applichiamo il danno
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
                Debug.Log($"🎯 Hai colpito il Player! Danno inflitto: {damage}");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                // Fail-safe: se l'oggetto ha il tag Player ma lo script è in un oggetto padre
                playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(damage);
            }
        }

        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}