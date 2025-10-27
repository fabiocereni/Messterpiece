using UnityEngine;
using UnityEngine.AI;
public class NPC : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;

    public NavMeshAgent Agent { get => agent; }

    [SerializeField]
    private string currentState;
    public PathNPC path;
    private GameObject player;
    public float sightDistance = 40f;
    public float fieldOfView = 85f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Inizialise();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        CanSeePlayer();
    }
    
    public bool CanSeePlayer()
    {
        if (player != null)
        {
            // is the player close enough to be seen?
            if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position;
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                Debug.LogError("ERRORE: Non trovo il GameObject con il tag 'Player'. Controlla l'Inspector del Player!");
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    Ray ray = new Ray(transform.position, targetDirection);
                    Debug.DrawRay(ray.origin, ray.direction * sightDistance);
                }
            }
        }
        return true;
    }
}
