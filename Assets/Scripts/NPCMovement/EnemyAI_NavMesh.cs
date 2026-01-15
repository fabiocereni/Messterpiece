using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_NavMesh : MonoBehaviour
{
    // Rimosso playerTransform come unico riferimento fisso, ora cerchiamo un target
    private Transform currentTarget; 
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Impostazioni")]
    public EnemyGun enemyGun;
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    [Header("Rilevamento")]
    [Tooltip("Layer che l'enemy deve attaccare (es. Player e Enemy)")]
    public LayerMask targetLayers; 
    [Tooltip("Angolo totale del cono visivo")]
    public float fieldOfViewAngle = 90f;
    [Tooltip("Distanza massima di visione")]
    public float detectionRadius = 15f;
    [Tooltip("Layer degli ostacoli per non vedere attraverso i muri")]
    public LayerMask obstacleLayer;
    [Tooltip("Spara solo se il nemico è girato verso il target entro questo angolo")]
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

        GotoNextPatrolPoint();
    }

    void Update()
    {
        // 1. Cerchiamo il bersaglio più vicino tra i layer selezionati
        FindClosestTarget();

        // 2. Se abbiamo un bersaglio valido
        if (currentTarget != null) // Stato CHASE
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            
            agent.speed = runSpeed;
            agent.SetDestination(currentTarget.position);
            
            if (animator != null) animator.SetBool("isRunning", true);
            
            // Logica di puntamento e sparo sul bersaglio corrente
            Vector3 aimDir = (currentTarget.position - transform.position).normalized;
            float aimAngle = Vector3.Angle(transform.forward, aimDir);

            if (aimAngle < shootingAngleThreshold && Time.time >= nextFireTime)
            {
                // Puntiamo al petto del bersaglio (+0.8f)
                enemyGun.Shoot((currentTarget.position + Vector3.up * 0.8f - enemyGun.firePoint.position).normalized);
                nextFireTime = Time.time + fireRate;
            }
        }
        else // Stato PATROL (nessun bersaglio nel cono visivo)
        {
            agent.speed = walkSpeed;
            if (animator != null) animator.SetBool("isRunning", false);
            
            if (!agent.pathPending && agent.remainingDistance < 0.8f)
                GotoNextPatrolPoint();
        }
    }

    /// <summary>
    /// Scansiona l'area e identifica il bersaglio più vicino che rientra nel FOV e non è coperto da muri
    /// </summary>
    void FindClosestTarget()
    {
        // Trova tutti i potenziali bersagli nei layer selezionati
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRadius, targetLayers);
        
        Transform bestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in potentialTargets)
        {
            // Salta se stesso (molto importante se il nemico è sul layer Enemy!)
            if (col.transform == transform) continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            // Controllo Cono Visivo
            if (angle < fieldOfViewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, col.transform.position);
                
                // Controllo Ostacoli (Raycast)
                if (!Physics.Raycast(transform.position + Vector3.up, directionToTarget, distanceToTarget, obstacleLayer))
                {
                    if (distanceToTarget < minDistance)
                    {
                        minDistance = distanceToTarget;
                        bestTarget = col.transform;
                    }
                }
            }
        }

        currentTarget = bestTarget;
    }

    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // Utile per vedere il raggio di rilevamento nell'editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}