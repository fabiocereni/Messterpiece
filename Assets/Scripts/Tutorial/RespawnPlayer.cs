using UnityEngine;

/// <summary>
/// Trigger che riporta il player al checkpoint quando cade.
/// Usato sotto le piattaforme della section Jump.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RespawnPlayer : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;

    [Header("Optional Feedback")]
    [SerializeField] private AudioClip respawnSound;

    void Start()
    {
        // Assicurati che il collider sia trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        if (respawnPoint == null)
        {
            Debug.LogError("RespawnPoint non assegnato! Il player non potrà essere respawnato.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RespawnPlayerToCheckpoint(other.gameObject);
        }
    }

    void RespawnPlayerToCheckpoint(GameObject player)
    {
        if (respawnPoint != null)
        {
            // Resetta la velocità del player se ha un Rigidbody
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Teleporta il player
            player.transform.position = respawnPoint.position;

            // Play sound (opzionale)
            if (respawnSound != null)
            {
                AudioSource.PlayClipAtPoint(respawnSound, respawnPoint.position);
            }

            Debug.Log("Player respawnato al checkpoint!");
        }
    }
}
