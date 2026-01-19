using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Vita massima del nemico")]
    public float maxHealth = 100f;
    
    [Tooltip("Vita corrente")]
    private float currentHealth;
    // salva chi ha inflitto l'ultimo danno per calcolare a chi assegnare la kill
    private GameObject lastAttacker = null;

    [Header("Damage Feedback")]
    [Tooltip("Prefab del numero danno (WorldSpace UI)")]
    public GameObject damageNumberPrefab; // testo del danno
    
    [Tooltip("Offset Y sopra il nemico dove appare il numero")]
    public float damageNumberOffsetY = 2f; // altezza sopra il nemico

    [Header("Death")]
    [Tooltip("Prefab VFX kill effect (skull + numero)")]
    public GameObject killVfxPrefab; // effetto visivo della morte

    [Tooltip("Durata del kill VFX (deve matchare la duration del particle system)")]
    public float killVfxDuration = 1.5f; // durata effetto

    [Tooltip("Delay prima di distruggere il GameObject dopo morte (deve matchare killVfxDuration)")]
    public float destroyDelay = 1.5f; // tempo per nascondere il corpo

    private bool isDead = false;

    void Start()
    {
        // applica moltiplicatore difficoltà alla vita
        float healthMultiplier = GameSettings.Instance != null 
            ? GameSettings.Instance.EnemyHealthMultiplier 
            : 1f;
        currentHealth = maxHealth * healthMultiplier;
    }

    // Applica danno al nemico
    // damage: quantità di danno
    // hitPoint: punto di impatto (per spawnare il numero del danno)
    // attacker: chi ha inflitto il danno (per assegnare la kill)
    public void TakeDamage(float damage, Vector3 hitPoint, GameObject attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (attacker != null)
        {
            lastAttacker = attacker;
    
            // Usiamo GetComponentInParent per essere sicuri di colpire l'NPC 
            // anche se questo script è su un collider figlio
            EnemyAI_NavMesh ai = GetComponentInParent<EnemyAI_NavMesh>();
            if (ai != null)
            {
                ai.ReportHit(attacker); // Segnala all'NPC che è stata colpito
            }
        }

        // spawna il numero del danno
        SpawnDamageNumber(damage, hitPoint);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // spawna il numero del danno sopra il nemico
    private void SpawnDamageNumber(float damage, Vector3 hitPoint)
    {
        if (damageNumberPrefab == null) return;

        // mi posiziono sopra il nemico (o al punto di impatto)
        Vector3 spawnPosition = transform.position + Vector3.up * damageNumberOffsetY;
        
        // istanzio il prefab
        GameObject damageNumberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        
        // imposto il testo del danno
        DamageNumber damageNumberScript = damageNumberObj.GetComponent<DamageNumber>();
        if (damageNumberScript != null)
        {
            damageNumberScript.SetDamage(damage);
        }
    }

    // gestisco la morte dell'NPC
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        GameObject rootEntity = transform.root.gameObject; 

        if (MatchManager.Instance != null)
        {
            // segna una kill per lastAttacker che ha ucciso rootEntity
            MatchManager.Instance.RegisterKill(lastAttacker, rootEntity);
        }

        // ferma l'NPC quando muore
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // disabilito la navmesh
        var ai = GetComponent<EnemyAI_NavMesh>();
        if (ai != null) ai.enabled = false;

        // nascondo MeshRenderer standard (armi ...)
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers) mr.enabled = false;

        // nascodo SkinnedMeshRenderer (quelli del corpo animato)
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedRenderers) smr.enabled = false;

        // disabilito il collider per evitare interazioni
        var capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null) capsuleCollider.enabled = false;
    
        // se il nemico ha un'arma, assicurati che non possa più sparare
        if (GetComponentInChildren<EnemyGun>() != null) 
            GetComponentInChildren<EnemyGun>().enabled = false;

        // effetti di morte
        if (killVfxPrefab != null)
        {
            GameObject killVfx = Instantiate(killVfxPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(killVfx, killVfxDuration);
        }

        EnemyRespawn respawnSystem = GetComponent<EnemyRespawn>();
        if (respawnSystem != null)
        {
            respawnSystem.RespawnEnemy();
        }
        else
        {
            // Se non c'è il sistema di respawn, distruggi normalmente
            Destroy(gameObject, destroyDelay);
        }
    }

    // ripristino la vita
    public void RestoreHealth(float amount)
    {
        isDead = false;
        currentHealth = amount;
    }

    // controllo se è morto
    public bool IsDead()
    {
        return isDead;
    }

    // ritorna la vita corrente
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
