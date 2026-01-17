using System.Collections;
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
    
    [Header("Respawn")]
    [Tooltip("Tempo di respawn del medikit")]
    public float respawnTime = 10f;
    
    private Renderer[] renderers;
    private Light[] lights;
    private Collider pickupCollider;

    private bool isCollected = false;
    
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        lights = GetComponentsInChildren<Light>(true);
        pickupCollider = GetComponent<Collider>();
    }

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

                PlayEffects();

                // Nascondi visivamente il medikit
                SetMedikitActive(false);

                // Avvia respawn
                StartCoroutine(RespawnCoroutine());
            }
        }
    }
    
    private void SetMedikitActive(bool active)
    {
        foreach (var r in renderers)
            r.enabled = active;

        foreach (var l in lights)
            l.enabled = active;

        if (pickupCollider != null)
            pickupCollider.enabled = active;
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
    
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        isCollected = false;
        SetMedikitActive(true);
    }
}
