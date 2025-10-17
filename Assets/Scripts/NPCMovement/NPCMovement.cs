using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    public float raggioDiMovimento = 20f; // serve per definire il raggio di movimento dell'NPC

    private NavMeshAgent agent;
    private bool isWaiting;
    private float waitTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        isWaiting = false;
        ScegliNuovaDestinazione();
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Attesa();
        }

        if (isWaiting)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0f)
            {
                isWaiting = false;
                ScegliNuovaDestinazione();
            }
        }

    }

    void Attesa()
    {
        waitTime = 0.01f;
        isWaiting = true;
    }

    void ScegliNuovaDestinazione()
    {
        Vector3 newPosition = Random.insideUnitSphere * raggioDiMovimento;
        newPosition += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, raggioDiMovimento, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
