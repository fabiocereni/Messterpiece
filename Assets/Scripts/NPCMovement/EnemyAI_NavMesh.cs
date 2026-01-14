using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_NavMesh : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Impostazioni")]
    public EnemyGun enemyGun;
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // L'agente comanda tutto: posizione e rotazione
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.stoppingDistance = 0.5f;

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        GotoNextPatrolPoint();
    }

    void Update()
    {
        // Se insegue il player
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        
        if (dist < 15f) // Raggio di Chase
        {
            agent.speed = runSpeed;
            agent.SetDestination(playerTransform.position);
            if (animator != null) animator.SetBool("isRunning", true);
            
            if (Time.time >= nextFireTime)
            {
                enemyGun.Shoot((playerTransform.position + Vector3.up * 0.8f - enemyGun.firePoint.position).normalized);
                nextFireTime = Time.time + fireRate;
            }
        }
        else // Patrol
        {
            agent.speed = walkSpeed;
            if (animator != null) animator.SetBool("isRunning", false);
            
            if (!agent.pathPending && agent.remainingDistance < 0.8f)
                GotoNextPatrolPoint();
        }
    }

    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}