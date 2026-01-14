using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_NavMesh : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Riferimento al transform del giocatore (opzionale, può trovarlo da solo)")]
    public Transform playerTransform;
    private NavMeshAgent agent;
    
    [Header("Combattimento")]
    public EnemyGun enemyGun; 
    public float fireRate = 1.0f; // Un colpo ogni secondo
    private float nextFireTime = 0f;
    
    [Tooltip("Altezza rispetto ai piedi del player dove il nemico mira")]
    public float aimVerticalOffset = 0.8f;

    [Header("Pattugliamento (Patrol)")]
    [Tooltip("Un array di punti (Transforms) tra cui il nemico si muoverà")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Rilevamento (Chase)")]
    [Tooltip("Il raggio in cui il nemico 'sente' la presenza del giocatore")]
    public float detectionRadius = 15f;
    [Tooltip("L'angolo del cono visivo del nemico (in gradi)")]
    public float fieldOfViewAngle = 90f;
    [Tooltip("Layer che contiene SOLO il giocatore")]
    public LayerMask playerLayer;
    [Tooltip("Layer che contengono gli ostacoli (es. Muri, Ambiente)")]
    public LayerMask obstacleLayer;

    private enum AIState { PATROL, CHASE }
    private AIState currentState;
    private bool playerIsInSight = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        currentState = AIState.PATROL;
        GotoNextPatrolPoint();
    }

    void Update()
    {
        CheckForPlayer();
    
        switch (currentState)
        {
            case AIState.PATROL:
                DoPatrol();
                break;
            case AIState.CHASE:
                DoChase();
                // Se il giocatore è visibile e il tempo è scaduto, spara
                if (playerIsInSight && Time.time >= nextFireTime)
                {
                    Attack();
                    nextFireTime = Time.time + fireRate;
                }
                break;
        }
    }
    
    void Attack()
    {
        if (enemyGun != null && playerTransform != null)
        {
            // Usiamo la variabile per regolare l'altezza in tempo reale
            Vector3 targetPoint = playerTransform.position + Vector3.up * aimVerticalOffset;

            // Calcoliamo la direzione dal FirePoint dell'arma verso il punto target
            Vector3 fireDirection = (targetPoint - enemyGun.firePoint.position).normalized;

            // Rotazione del corpo del nemico (solo asse Y)
            Vector3 lookPos = playerTransform.position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

            enemyGun.Shoot(fireDirection);
        }
    }


    void CheckForPlayer()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfViewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    playerIsInSight = true;
                    currentState = AIState.CHASE;
                    return;
                }
            }
        }

        if (playerIsInSight)
        {
            playerIsInSight = false;
            currentState = AIState.PATROL;
            GotoNextPatrolPoint();
        }
    }

    void DoPatrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextPatrolPoint();
        }
    }

    void DoChase()
    {
        agent.destination = playerTransform.position;
    }

    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("Nessun punto di pattuglia assegnato!", this);
            return;
        }
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}
