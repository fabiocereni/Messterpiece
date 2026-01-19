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

    // Avvio la sequenza di respawn dell'NPC
    public void RespawnEnemy()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Aspetta il tempo di respawn
        yield return new WaitForSeconds(respawnDelay);

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // disabilito la navmesh per teletrasportare l'NPC senza problemi
            if (agent != null) agent.enabled = false;
            
            transform.position = sp.position;
            transform.rotation = sp.rotation;
            
            if (agent != null) agent.enabled = true;
        }

        // ripristino i componenti e la visibilità
        ResetComponents();
    }

    private void ResetComponents()
    {
        // ripristino la salute
        if (enemyHealth != null)
        {
            enemyHealth.RestoreHealth(enemyHealth.maxHealth);
        }

        // riattivo l'IA
        if (aiScript != null) aiScript.enabled = true;

        // riabilito il collider
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = true;

        // riattivo tutti i pezzi del corpo, figlio per figlio
        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>()) smr.enabled = true;
        foreach (var mr in GetComponentsInChildren<MeshRenderer>()) mr.enabled = true;

        // resetto Animator
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("isRunning", false);
        }
    }
}
