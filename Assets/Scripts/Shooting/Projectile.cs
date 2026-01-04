using System.Collections;
using UnityEngine;

/// <summary>
/// VISUAL TRACER ONLY
/// This projectile is PURELY VISUAL!
/// Damage is applied instantly by raycast in Gun.cs
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Visual Tracer Settings")]
    [Tooltip("Speed of the visual tracer (very fast for instant feel: 200+ m/s)")]
    public float speed = 250f;

    [Tooltip("Lifetime before auto-destroy (short for instant effect)")]
    public float lifetime = 0.5f;

    [Header("VFX")]
    [Tooltip("Should destroy on impact with walls? (Optional visual feedback)")]
    public bool destroyOnWallHit = true;

    [Tooltip("Hide the projectile mesh? (Only show trail)")]
    public bool hideProjectileMesh = true;

    void Start()
    {
        // Hide the sphere mesh (only trail visible)
        if (hideProjectileMesh)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }

        // Auto-destroy after lifetime
        StartCoroutine(DestroyAfterTime(lifetime));
    }

    void Update()
    {
        // Move forward (visual only)
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // ═══════════════════════════════════════════════════════
        // IMPORTANT: This projectile does NOT apply damage!
        // Damage is handled by instant raycast in Gun.cs
        // This is ONLY for visual feedback
        // ═══════════════════════════════════════════════════════

        // Ignore trigger zones
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerZone"))
        {
            return;
        }

        // Ignore other projectiles
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            return;
        }

        // Ignore player (self)
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return;
        }

        // Pass through enemies (damage already applied by raycast!)
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Optional: Spawn impact VFX here
            return; // Don't destroy, pass through
        }

        // Hit a wall/obstacle/ground → Destroy for visual feedback
        if (destroyOnWallHit)
        {
            // Optional: Spawn wall splatter VFX here
            Destroy(this.gameObject);
        }
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}