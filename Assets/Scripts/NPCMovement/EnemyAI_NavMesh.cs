using UnityEngine;
using UnityEngine.AI; // Importante! Devi importare questo namespace

/// <summary>
/// Gestisce l'IA di un nemico usando il sistema NavMesh di Unity.
/// L'IA ha due stati:
/// 1. PATROL: Si muove tra una serie di punti di pattuglia.
/// 2. CHASE: Insegue il giocatore dopo averlo visto.
///
/// CONFIGURAZIONE (LEGGI "GUIDA ALLA CONFIGURAZIONE" SOTTO):
/// 1. Installa il pacchetto AI Navigation (Window > Package Manager).
/// 2. "Bake" (Cuoci) la tua mappa per creare una NavMesh.
/// 3. Aggiungi questo script al tuo nemico.
/// 4. Aggiungi un componente "NavMesh Agent" al nemico.
/// 5. Assegna il tag "Player" al tuo giocatore.
/// 6. Crea dei punti di pattuglia e assegnali all'array "patrolPoints".
/// 7. Imposta i Layer (Player e Obstacle).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))] // Assicura che ci sia sempre un NavMeshAgent
public class EnemyAI_NavMesh : MonoBehaviour
{
    // --- Riferimenti ---
    [Header("Riferimenti")]
    [Tooltip("Riferimento al transform del giocatore (opzionale, può trovarlo da solo)")]
    public Transform playerTransform;
    private NavMeshAgent agent; // Il componente che gestisce il movimento

    // --- Impostazioni di Pattugliamento ---
    [Header("Pattugliamento (Patrol)")]
    [Tooltip("Un array di punti (Transforms) tra cui il nemico si muoverà")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0; // L'indice del punto di pattuglia attuale

    // --- Impostazioni di Rilevamento ---
    [Header("Rilevamento (Chase)")]
    [Tooltip("Il raggio in cui il nemico 'sente' la presenza del giocatore")]
    public float detectionRadius = 15f;
    [Tooltip("L'angolo del cono visivo del nemico (in gradi)")]
    public float fieldOfViewAngle = 90f;
    [Tooltip("Layer che contiene SOLO il giocatore")]
    public LayerMask playerLayer;
    [Tooltip("Layer che contengono gli ostacoli (es. Muri, Ambiente)")]
    public LayerMask obstacleLayer;

    // --- Stato ---
    private enum AIState { PATROL, CHASE }
    private AIState currentState;
    private bool playerIsInSight = false;

    void Start()
    {
        // Prendi i componenti necessari
        agent = GetComponent<NavMeshAgent>();

        // Cerca il giocatore se non è stato assegnato
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Inizia lo stato di pattugliamento
        currentState = AIState.PATROL;
        GotoNextPatrolPoint();
    }

    void Update()
    {
        // Controlla sempre se il giocatore è in vista
        CheckForPlayer();

        // Esegui la logica in base allo stato attuale
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

    /// <summary>
    /// Controlla la logica di rilevamento del giocatore.
    /// </summary>
    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        // Controlla se il giocatore è all'interno del raggio di rilevamento
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            // Il giocatore è abbastanza vicino, ora controlla il cono visivo e la linea di vista

            // 1. Controllo Cono Visivo (Field of View)
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfViewAngle / 2)
            {
                // 2. Controllo Linea di Vista (Raycast)
                // Controlla se c'è un ostacolo tra il nemico e il giocatore
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    // GIOCATORE VISTO!
                    playerIsInSight = true;
                    currentState = AIState.CHASE;
                    return; // Esci, abbiamo trovato il giocatore
                }
            }
        }

        // Se arriviamo qui, il giocatore non è in vista
        // Se stavamo inseguendo, torniamo a pattugliare
        if (playerIsInSight)
        {
            playerIsInSight = false;
            currentState = AIState.PATROL;
            GotoNextPatrolPoint(); // Ricomincia a pattugliare dal punto più vicino
        }
    }

    /// <summary>
    /// Logica per lo stato PATROL.
    /// </summary>
    void DoPatrol()
    {
        // Controlla se l'agente ha raggiunto la destinazione
        // 'remainingDistance' non è affidabile se non è 'pathPending'
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Raggiunto il punto, vai al successivo
            GotoNextPatrolPoint();
        }
    }

    /// <summary>
    /// Logica per lo stato CHASE.
    /// </summary>
    void DoChase()
    {
        // Imposta la destinazione sulla posizione attuale del giocatore
        // Il NavMeshAgent calcolerà automaticamente il percorso migliore
        agent.destination = playerTransform.position;
    }

    /// <summary>
    /// Imposta la destinazione dell'agente sul prossimo punto di pattuglia.
    /// </summary>
    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("Nessun punto di pattuglia assegnato!", this);
            return;
        }

        // Imposta la destinazione sul punto attuale
        agent.destination = patrolPoints[currentPatrolIndex].position;

        // Aggiorna l'indice per il prossimo giro, tornando a 0 se è alla fine
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}