using System.Collections;
using UnityEngine;

// Medikit che cura il player al contatto e poi scompare
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
        renderers = GetComponentsInChildren<Renderer>(true); // cerca parti grafiche
        lights = GetComponentsInChildren<Light>(true); // cerca luci
        pickupCollider = GetComponent<Collider>(); // collider di raccolta
    }

    // si attiva al contatto quando cammini sopra
    private void OnTriggerEnter(Collider other)
    {
        // evita di raccogliere più volte
        if (isCollected) return;

        // controlla se è un player
        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>(); // ottengo la salute del player
            
            if (playerHealth != null)
            {
                // curo il player
                playerHealth.Heal(healAmount);
                
                isCollected = true; // segna come raccolto

                PlayEffects(); // suono e particelle

                // nascondi visivamente il medikit
                SetMedikitActive(false);

                // avvio il respawn
                StartCoroutine(RespawnCoroutine());
            }
        }
    }
    
    // "spengo" il medikit solo a livello grafico
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
        // suono
        if (pickupSound != null)
        {
            // uso PlayClipAtPoint per non dover gestire un audio source
            // altrimenti con audio source il suono verrebbe troncato appena diventa invisibile
            AudioSource.PlayClipAtPoint(pickupSound, transform.position); 
        }

        // particelle
        if (pickupVFX != null)
        {
            GameObject vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        isCollected = false; // resetto lo stato, quindi può essere raccolto di nuovo
        SetMedikitActive(true); // riattivo visivamente il medikit
    }
}
