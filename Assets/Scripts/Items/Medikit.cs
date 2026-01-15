using UnityEngine;

/// <summary>
/// Medikit che cura il player al contatto e poi scompare
/// Richiede un Collider con isTrigger = true
/// </summary>
public class Medikit : MonoBehaviour
{
    [Header("Impostazioni")]
    [Tooltip("Quantità di vita da ripristinare")]
    public float healAmount = 50f;
    
    [Tooltip("Tag del player (default: 'Player')")]
    public string playerTag = "Player";

    [Header("Effetti (Opzionali)")]
    [Tooltip("Suono quando raccolto")]
    public AudioClip pickupSound;
    
    [Tooltip("Effetto particelle quando raccolto")]
    public GameObject pickupVFX;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Evita raccolte multiple
        if (isCollected) return;

        // Controlla se è il player
        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // Cura il player
                playerHealth.Heal(healAmount);
                
                isCollected = true;
                
                Debug.Log($"[Medikit] Player curato di {healAmount} HP");

                // Effetti opzionali
                PlayEffects();

                // Distruggi il medikit
                Destroy(gameObject);
            }
        }
    }

    private void PlayEffects()
    {
        // Suono
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Particelle
        if (pickupVFX != null)
        {
            GameObject vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
}
