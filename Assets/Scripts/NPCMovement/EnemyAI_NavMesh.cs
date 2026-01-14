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

    [Header("Rilevamento")]
    [Tooltip("Angolo totale del cono visivo (es. 90 gradi)")]
    public float fieldOfViewAngle = 90f;
    [Tooltip("Distanza massima di visione")]
    public float detectionRadius = 15f;
    [Tooltip("Layer degli ostacoli per non vedere attraverso i muri")]
    public LayerMask obstacleLayer;
    [Tooltip("Spara solo se il nemico è girato verso il player entro questo angolo")]
    public float shootingAngleThreshold = 10f;

    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.stoppingDistance = 0.5f;

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        GotoNextPatrolPoint();
    }

    void Update()
    {
        bool canSeePlayer = false;
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < detectionRadius)
        {
            // 1. Calcola la direzione e l'angolo tra il nemico e il giocatore
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            // 2. Controllo Cono Visivo: il player deve essere davanti (entro metà angolo per lato)
            if (angle < fieldOfViewAngle / 2f)
            {
                // 3. Controllo Ostacoli: Raycast per vedere se ci sono muri nel mezzo
                // Partiamo da Y+1 per non colpire il pavimento
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, dist, obstacleLayer))
                {
                    canSeePlayer = true;
                }
            }
        }

        if (canSeePlayer) // Stato CHASE
        {
            agent.speed = runSpeed;
            agent.SetDestination(playerTransform.position);
            if (animator != null) animator.SetBool("isRunning", true);
            
            // 4. Spara solo se l'angolo di puntamento è stretto (non spara mentre si sta ancora girando)
            Vector3 aimDir = (playerTransform.position - transform.position).normalized;
            float aimAngle = Vector3.Angle(transform.forward, aimDir);

            if (aimAngle < shootingAngleThreshold && Time.time >= nextFireTime)
            {
                enemyGun.Shoot((playerTransform.position + Vector3.up * 0.8f - enemyGun.firePoint.position).normalized);
                nextFireTime = Time.time + fireRate;
            }
        }
        else // Stato PATROL
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