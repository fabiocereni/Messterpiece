using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_NavMesh : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Riferimento al transform del giocatore (opzionale, può trovarlo da solo)")]
    public Transform playerTransform;
    private NavMeshAgent agent;

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
                break;
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
