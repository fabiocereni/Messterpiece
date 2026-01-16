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
    
    private Transform alertTarget;
    private float alertTimer = 0f;
    private float alertDuration = 5f; // Quanto tempo resta "allerta" dopo il colpo
    public float rotationSpeed = 5f; // Velocità di rotazione su se stesso
    

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
        FindClosestTarget();
    
        if (currentTarget != null && IsEntityDead(currentTarget.gameObject))
        {
            currentTarget = null;
        }

        if (currentTarget != null) 
        {
            // Forza la rotazione verso il bersaglio ---
            FaceTarget(currentTarget.position);
            // ----------------------------------------------------

            float dist = Vector3.Distance(transform.position, currentTarget.position);
            agent.speed = runSpeed;
            agent.SetDestination(currentTarget.position);
        
            if (animator != null) animator.SetBool("isRunning", true);
        
            Vector3 aimDir = (currentTarget.position - transform.position).normalized;
            float aimAngle = Vector3.Angle(transform.forward, aimDir);

            if (aimAngle < shootingAngleThreshold && Time.time >= nextFireTime)
            {
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
    
    public void ReportHit(GameObject attacker)
    {
        if (attacker == null) return;
        alertTarget = attacker.transform;
        alertTimer = alertDuration; // Si attiva il timer di allerta
    }
    
    // Funzione di supporto per capire se un bersaglio è morto
    bool IsEntityDead(GameObject obj)
    {
        // Controlla se è un nemico
        EnemyHealth eHealth = obj.GetComponentInParent<EnemyHealth>();
        if (eHealth != null) return eHealth.IsDead();

        // Controlla se è il player (assumendo che PlayerHealth abbia IsDead())
        PlayerHealth pHealth = obj.GetComponentInParent<PlayerHealth>();
        if (pHealth != null) return pHealth.IsDead();

        return false;
    }

    /// <summary>
    /// Scansiona l'area e identifica il bersaglio più vicino che rientra nel FOV e non è coperto da muri
    /// </summary>
    void FindClosestTarget()
    {
        // Riduciamo il timer nel tempo
        if (alertTimer > 0) alertTimer -= Time.deltaTime;

        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRadius, targetLayers);
    
        Transform bestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in potentialTargets)
        {
            if (col.transform == transform) continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            // Entra nel loop se il bersaglio è nel FOV 
            // OPPURE se è il bersaglio che ci ha appena colpito (alertTarget)
            bool isAlertSource = (alertTimer > 0 && col.transform == alertTarget);

            if (isAlertSource || angle < fieldOfViewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, col.transform.position);
            
                // Controllo Ostacoli (Raycast) - Resta necessario per non sparare attraverso i muri
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
    
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Impedisce all'NPC di inclinarsi verso l'alto/basso
    
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // Ruota gradualmente ma velocemente verso il target
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
}