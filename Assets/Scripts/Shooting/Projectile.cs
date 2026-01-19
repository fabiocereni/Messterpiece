using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Visual Tracer Settings")]
    [Tooltip("Speed of the visual tracer (very fast for instant feel: 200+ m/s)")]
    public float speed = 250f; // velocità del proiettile

    [Tooltip("Lifetime before auto-destroy (short for instant effect)")]
    public float lifetime = 0.5f; // durata del proiettile

    [Header("VFX")]
    [Tooltip("Should destroy on impact with walls? (Optional visual feedback)")]
    public bool destroyOnWallHit = true; // proiettile si distrugge all'impatto con i muri

    [Tooltip("Hide the projectile mesh? (Only show trail)")]
    public bool hideProjectileMesh = true; // nascondi il mesh del proiettile

    void Start()
    {
        // nascondi la mesh del proiettile per vedere solo la scia
        if (hideProjectileMesh)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
        // distruggi il proiettile dopo un certo tempo
        StartCoroutine(DestroyAfterTime(lifetime));
    }

    void Update()
    {
        // muovi il proiettile in avanti
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    // questa funzione viene chiamata quando il proiettile entra in collisione con un altro collider
    void OnTriggerEnter(Collider other)
    {
        // ignoe trigger zones
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerZone"))
        {
            return;
        }

        // ignore other projectiles
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            return;
        }

        // ignora te stesso
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return;
        }

        // 
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        // distruisci il proiettile se colpisce un muro
        if (destroyOnWallHit)
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}