using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyRespawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Lista degli spawn point per gli NPC")]
    public Transform[] spawnPoints;
    [Tooltip("Tempo di attesa prima del respawn")]
    public float respawnDelay = 3.0f;

    private EnemyHealth enemyHealth;
    private NavMeshAgent agent;
    private EnemyAI_NavMesh aiScript;
    private Animator animator;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        agent = GetComponent<NavMeshAgent>();
        aiScript = GetComponent<EnemyAI_NavMesh>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Avvia la sequenza di respawn dell'NPC
    /// </summary>
    public void RespawnEnemy()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Aspetta il tempo stabilito (mentre il bot è "invisibile")
        yield return new WaitForSeconds(respawnDelay);

        // 1. Trova uno spawn point casuale
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Disabilita l'agente per teletrasportarlo correttamente
            if (agent != null) agent.enabled = false;
            
            transform.position = sp.position;
            transform.rotation = sp.rotation;
            
            if (agent != null) agent.enabled = true;
        }

        // 2. Ripristina i componenti e la visibilità
        ResetComponents();

        Debug.Log($"[EnemyRespawn] {gameObject.name} è tornato in gioco!");
    }

    private void ResetComponents()
    {
        // Ripristina la salute
        if (enemyHealth != null)
        {
            enemyHealth.RestoreHealth(enemyHealth.maxHealth);
        }

        // Riattiva l'IA
        if (aiScript != null) aiScript.enabled = true;

        // Riabilita il collider
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = true;

        // Mostra di nuovo il modello (SkinnedMesh e MeshRenderer)
        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>()) smr.enabled = true;
        foreach (var mr in GetComponentsInChildren<MeshRenderer>()) mr.enabled = true;

        // Reset Animator
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("isRunning", false);
        }
    }
}
