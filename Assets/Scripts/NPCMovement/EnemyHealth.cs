using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Vita massima del nemico")]
    public float maxHealth = 100f;
    
    [Tooltip("Vita corrente")]
    private float currentHealth;

    // Track who dealt the last damage (for kill attribution)
    private GameObject lastAttacker = null;

    [Header("Damage Feedback")]
    [Tooltip("Prefab del numero danno (WorldSpace UI)")]
    public GameObject damageNumberPrefab;
    
    [Tooltip("Offset Y sopra il nemico dove appare il numero")]
    public float damageNumberOffsetY = 2f;

    [Header("Death")]
    [Tooltip("Prefab VFX kill effect (skull + numero)")]
    public GameObject killVfxPrefab;

    [Tooltip("Durata del kill VFX (deve matchare la duration del particle system)")]
    public float killVfxDuration = 1.5f;

    [Tooltip("Delay prima di distruggere il GameObject dopo morte (deve matchare killVfxDuration)")]
    public float destroyDelay = 1.5f;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Applica danno al nemico
    /// </summary>
    /// <param name="damage">Amount of damage</param>
    /// <param name="hitPoint">World position where hit occurred</param>
    /// <param name="attacker">GameObject that dealt the damage (for kill attribution)</param>
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Track who attacked (for kill credit)
        if (attacker != null)
        {
            lastAttacker = attacker;
        }

        Debug.Log($"[EnemyHealth] {gameObject.name} took {damage} damage from {(attacker != null ? attacker.name : "unknown")}. Health: {currentHealth}/{maxHealth}");

        // Spawn damage number popup
        SpawnDamageNumber(damage, hitPoint);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Spawna il numero del danno sopra il nemico
    /// </summary>
    private void SpawnDamageNumber(float damage, Vector3 hitPoint)
    {
        if (damageNumberPrefab == null) return;

        // Position sopra il nemico (o al punto di impatto)
        Vector3 spawnPosition = transform.position + Vector3.up * damageNumberOffsetY;
        
        // Istanzia il prefab
        GameObject damageNumberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        
        // Imposta il testo del danno
        DamageNumber damageNumberScript = damageNumberObj.GetComponent<DamageNumber>();
        if (damageNumberScript != null)
        {
            damageNumberScript.SetDamage(damage);
        }
    }

    /// <summary>
    /// Nemico muore
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[EnemyHealth] {gameObject.name} KILLED by {(lastAttacker != null ? lastAttacker.name : "unknown")}!");

        // ═══════════════════════════════════════════════════════
        // REGISTER KILL WITH MATCH MANAGER
        // ═══════════════════════════════════════════════════════
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.RegisterKill(lastAttacker, gameObject);
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] MatchManager not found! Kill not registered.");
        }

        // NASCONDI IMMEDIATAMENTE il mesh del nemico (così la sfera VFX lo copre)
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Nascondi anche eventuali child meshes (es. arma del nemico)
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in childRenderers)
        {
            renderer.enabled = false;
        }

        // Spawn kill VFX AL CENTRO del nemico (Y+1.0 per capsule height 2)
        if (killVfxPrefab != null)
        {
            GameObject killVfx = Instantiate(killVfxPrefab, transform.position, Quaternion.identity);
            Destroy(killVfx, killVfxDuration);
        }

        // Disabilita AI e collider
        var ai = GetComponent<EnemyAI_NavMesh>();
        if (ai != null) ai.enabled = false;

        var capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null) capsuleCollider.enabled = false;

        // Distruggi dopo delay (sincronizzato con durata VFX)
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// Getter per checking se morto
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Getter health corrente
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
