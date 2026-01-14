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
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);

        // Usiamo la direzione precisa passata dall'IA
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, maxDistance, damageLayerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("🎯 Giocatore colpito!");
            }
        }

        // Per il proiettile visivo, lo facciamo guardare nella direzione del colpo
        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }
}