using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_NavMesh : MonoBehaviour
{
    private Transform currentTarget; // target che l'NPC sta inseguendo
    private NavMeshAgent agent; // il movimento dell'NPC
    private Animator animator; // per le animazioni

    [Header("Riferimenti")]
    public EnemyGun enemyGun; // arma dell'NPC

    [Header("Impostazioni Movimento")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 10f;

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
    
    [Header("Combat Tuning")]
    [Tooltip("Distanza alla quale l'NPC si ferma per sparare invece di correre addosso")]
    public float attackStoppingDistance = 6f; 
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;
    
    // Logica di Allerta
    private Transform alertTarget; // chi ha colpito l'NPC
    private float alertTimer = 0f;
    private float alertDuration = 5f; // per quanti secondi l'NPC rimane allertato, quindi insegue il player

    [Header("Pattugliamento")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = -1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.stoppingDistance = 0.5f;

        // inizializza con un punto di pattuglia casuale
        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            GotoNextPatrolPoint();
        }
    }

    void Update()
    {
        FindClosestTarget(); // scansiono l'area e controllo se c'è un player da inseguire
    
        if (currentTarget != null && IsEntityDead(currentTarget.gameObject))
        {
            currentTarget = null;
        }

        if (currentTarget != null) 
        {
            HandleCombat();
        }
        else 
        {
            HandlePatrol();
        }
    }

    private void HandleCombat()
    {
        // 1. Forza sempre la rotazione verso il bersaglio (anche se l'agente è fermo)
        FaceTarget(currentTarget.position);

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        // 2. CONTROLLO DISTANZA (Anti-girotondo)
        if (dist <= attackStoppingDistance)
        {
            // Troppo vicino per correre: ci fermiamo e puntiamo l'arma
            agent.isStopped = true; 
            if (animator != null) animator.SetBool("isRunning", false);
        }
        else
        {
            // Lontano: corriamo verso il bersaglio
            agent.isStopped = false;
            agent.speed = runSpeed;
            agent.SetDestination(currentTarget.position);
            if (animator != null) animator.SetBool("isRunning", true);
        }

        // 3. LOGICA DI SPARO
        Vector3 aimDir = (currentTarget.position - transform.position).normalized;
        float aimAngle = Vector3.Angle(transform.forward, aimDir);

        if (aimAngle < shootingAngleThreshold && Time.time >= nextFireTime)
        {
            // Controllo ostacoli prima di sparare (usa il firePoint dell'arma)
            if (enemyGun != null && !Physics.Raycast(enemyGun.firePoint.position, aimDir, dist, obstacleLayer))
            {
                // Miriamo al petto (offset 0.8)
                Vector3 shootDirection = (currentTarget.position + Vector3.up * 0.8f - enemyGun.firePoint.position).normalized;
                enemyGun.Shoot(shootDirection);
                nextFireTime = Time.time + fireRate;

                // Trigger animazione SOLO se siamo fermi
                if (animator != null && agent.isStopped)
                {
                    animator.SetTrigger("shoot");
                }
            }
        }
    }

    private void HandlePatrol()
    {
        // l'NPC puo' muoversi dopo un combattimento
        agent.isStopped = false; 
        agent.speed = walkSpeed;
        if (animator != null) animator.SetBool("isRunning", false);
        
        if (!agent.pathPending && agent.remainingDistance < 0.8f)
        {
            GotoNextPatrolPoint();
        }
    }
    
    // metodo che permette all'NPC di essere avvisato che è stato colpito
    public void ReportHit(GameObject attacker)
    {
        if (attacker == null) return;
        alertTarget = attacker.transform; // salvo la posizione di chi ha sparato l'NPC
        alertTimer = alertDuration; // per un certo timer l'NPC è in allerta
    }
    
    bool IsEntityDead(GameObject obj)
    {
        EnemyHealth eHealth = obj.GetComponentInParent<EnemyHealth>();
        if (eHealth != null) return eHealth.IsDead();

        PlayerHealth pHealth = obj.GetComponentInParent<PlayerHealth>();
        if (pHealth != null) return pHealth.IsDead();

        return false;
    }

    void FindClosestTarget()
    {
        if (alertTimer > 0) alertTimer -= Time.deltaTime;

        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRadius, targetLayers);
    
        Transform bestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in potentialTargets)
        {
            // Ignora se stesso
            if (col.transform.root == transform.root) continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            bool isAlertSource = (alertTimer > 0 && col.transform == alertTarget);

            if (isAlertSource || angle < fieldOfViewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, col.transform.position);
            
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

    // va al prossimo patrol point
    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        if (patrolPoints.Length == 1)
        {
            agent.SetDestination(patrolPoints[0].position);
            return;
        }

        // scelgo un indice casuale differente dal precedente
        int newIndex = currentPatrolIndex;
        while (newIndex == currentPatrolIndex)
        {
            newIndex = Random.Range(0, patrolPoints.Length);
        }

        currentPatrolIndex = newIndex;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    // fa girare l'NPC verso una posizione target
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; 
    
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction); // calcola la rotazione verso il target
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed); // rotazione graduale
        }
    }

    // serve solo per debug in scena
    private void OnDrawGizmosSelected()
    {
        // Raggio di rilevamento
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        // Distanza di arresto combattimento
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackStoppingDistance);
    }

}