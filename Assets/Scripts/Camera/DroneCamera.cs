using UnityEngine;

/// <summary>
/// Script per la DroneCamera che segue il player dall'alto quando è morto
/// Può essere una camera statica o che segue il player da lontano
/// </summary>
public class DroneCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Se true, segue il player dall'alto. Se false, rimane fissa")]
    public bool followPlayer = true;

    [Tooltip("Riferimento al transform del player (auto-find se null)")]
    public Transform playerTransform;

    [Tooltip("Offset Y dalla posizione del player")]
    public float heightOffset = 15f;

    [Tooltip("Distanza dietro al player")]
    public float distanceOffset = 10f;

    [Tooltip("Velocità di smoothing del movimento")]
    public float smoothSpeed = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Guarda sempre verso il player")]
    public bool lookAtPlayer = true;

    [Tooltip("Angolo di pitch della camera (guardare verso il basso)")]
    public float pitchAngle = 45f;

    private Camera droneCamera;

    private void Awake()
    {
        droneCamera = GetComponent<Camera>();

        // La DroneCamera parte disattivata
        if (droneCamera != null)
        {
            droneCamera.enabled = false;
        }
    }

    private void Start()
    {
        // Auto-find player se non assegnato
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("[DroneCamera] Player trovato automaticamente");
            }
            else
            {
                Debug.LogWarning("[DroneCamera] Player non trovato! Assegna il player nell'Inspector.");
            }
        }
    }

    private void LateUpdate()
    {
        // Solo se la camera è attiva e dobbiamo seguire il player
        if (droneCamera == null || !droneCamera.enabled || !followPlayer || playerTransform == null)
            return;

        // Calcola posizione target sopra e dietro il player
        Vector3 targetPosition = playerTransform.position
            - playerTransform.forward * distanceOffset
            + Vector3.up * heightOffset;

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Guarda verso il player
        if (lookAtPlayer)
        {
            Vector3 lookDirection = playerTransform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Applica pitch angle
            targetRotation *= Quaternion.Euler(pitchAngle, 0, 0);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Imposta il transform del player da seguire
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    /// <summary>
    /// Resetta la posizione della camera sopra il player
    /// </summary>
    public void ResetPosition()
    {
        if (playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position
            - playerTransform.forward * distanceOffset
            + Vector3.up * heightOffset;

        transform.position = targetPosition;

        if (lookAtPlayer)
        {
            Vector3 lookDirection = playerTransform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    // Visualizza gizmo per debug
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, playerTransform.position);
    }
}
