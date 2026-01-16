using UnityEngine;

/// <summary>
/// Gestisce la logica della camera in terza persona (TPS)
/// Include collision detection, wall avoidance e smooth follow
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("TPS Settings")]
    [Tooltip("Distanza ideale della camera dal player")]
    public float targetDistance = 3.5f;
    
    [Tooltip("Altezza della camera rispetto al player")]
    public float cameraHeight = 1.8f;
    
    [Tooltip("Offset laterale (over-the-shoulder)")]
    public float shoulderOffset = 0.5f;

    [Header("Camera Smoothing")]
    [Tooltip("Velocità di smoothing della posizione")]
    public float positionSmoothSpeed = 10f;
    
    [Tooltip("Velocità di smoothing della rotazione")]
    public float rotationSmoothSpeed = 8f;

    [Header("Collision Settings")]
    [Tooltip("LayerMask per collision detection")]
    public LayerMask collisionLayers = -1;
    
    [Tooltip("Raggio della sfera per SphereCast")]
    public float collisionRadius = 0.3f;
    
    [Tooltip("Distanza minima dalla parete")]
    public float minDistanceFromWall = 0.5f;
    
    [Tooltip("Velocità di zoom-in quando colpisce muro")]
    public float collisionZoomSpeed = 15f;

    [Header("Camera Pivot")]
    [Tooltip("Punto di pivot della camera (occhi del player)")]
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Debug")]
    [Tooltip("Mostra gizmo di collision detection")]
    public bool showCollisionGizmos = false;

    // References
    private CameraController cameraController;
    private Camera mainCamera;
    private Transform cameraHolder;
    private Transform playerTransform;

    // Camera state
    private Vector3 currentCameraPosition;
    private float currentDistance;
    private Vector3 pivotPosition;

    // Collision
    private bool isColliding = false;
    private float targetCollisionDistance;

    // Original position
    private Vector3 originalLocalPosition;

    /// <summary>
    /// Inizializza il componente TPS Camera
    /// </summary>
    public void Initialize(CameraController controller, Camera camera, Transform holder, Transform player)
    {
        cameraController = controller;
        mainCamera = camera;
        cameraHolder = holder;
        playerTransform = player;

        // Salva posizione locale originale
        if (cameraHolder != null)
        {
            originalLocalPosition = cameraHolder.localPosition;
        }

        // Inizializza distanza
        currentDistance = targetDistance;
        targetCollisionDistance = targetDistance;
    }

    private void OnEnable()
    {
        // Quando viene attivato TPS, inizializza la posizione
        if (cameraHolder != null && playerTransform != null)
        {
            ResetPosition();
        }
    }

    private void OnDisable()
    {
        // Quando viene disattivato TPS, ripristina posizione locale originale
        if (cameraHolder != null)
        {
            cameraHolder.localPosition = originalLocalPosition;
            cameraHolder.localRotation = Quaternion.identity;
        }
    }

    private void LateUpdate()
    {
        if (!enabled || cameraHolder == null || playerTransform == null)
            return;

        // Update camera position e collision
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // 1. Calcola pivot position (punto di rotazione della camera)
        pivotPosition = playerTransform.position + pivotOffset;

        // 2. Calcola direzione camera (dietro il player + offset laterale)
        Vector3 targetDirection = CalculateCameraDirection();

        // 3. Collision detection
        float adjustedDistance = CheckCameraCollision(pivotPosition, targetDirection);

        // 4. Calcola posizione finale
        Vector3 targetPosition = pivotPosition - (targetDirection * adjustedDistance);

        // 5. Smooth movement
        currentCameraPosition = Vector3.Lerp(
            currentCameraPosition,
            targetPosition,
            Time.deltaTime * positionSmoothSpeed
        );

        // 6. Applica posizione al CameraHolder
        cameraHolder.position = currentCameraPosition;

        // 7. Guarda sempre verso il player (pivot point)
        UpdateCameraRotation();
    }

    private Vector3 CalculateCameraDirection()
    {
        // Direzione basata sulla rotazione del player
        // Ritorna la direzione FORWARD del player (poi verrà sottratta per posizionare dietro)

        // Forward del player (dove guarda)
        Vector3 playerForward = playerTransform.forward;
        Vector3 playerRight = playerTransform.right;

        // Direzione della camera (forward del player)
        // Verrà sottratta dal pivot per posizionare la camera dietro
        Vector3 direction = playerForward;

        // Offset laterale (over-the-shoulder)
        direction += playerRight * shoulderOffset;

        // Normalizza
        direction.Normalize();

        return direction;
    }

    private float CheckCameraCollision(Vector3 origin, Vector3 direction)
    {
        // SphereCast dal pivot verso la posizione target della camera
        RaycastHit hit;
        
        float checkDistance = targetDistance + collisionRadius;
        
        // Esegui SphereCast
        if (Physics.SphereCast(
            origin,
            collisionRadius,
            direction,
            out hit,
            checkDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            // Collisione rilevata
            isColliding = true;

            // Calcola distanza sicura dal muro
            float safeDistance = hit.distance - collisionRadius - minDistanceFromWall;
            safeDistance = Mathf.Max(safeDistance, minDistanceFromWall);

            // Smooth zoom
            targetCollisionDistance = Mathf.Lerp(
                targetCollisionDistance,
                safeDistance,
                Time.deltaTime * collisionZoomSpeed
            );

            if (showCollisionGizmos)
            {
                Debug.DrawLine(origin, hit.point, Color.red);
            }

            return targetCollisionDistance;
        }
        else
        {
            // Nessuna collisione
            isColliding = false;

            // Ritorna gradualmente alla distanza target
            targetCollisionDistance = Mathf.Lerp(
                targetCollisionDistance,
                targetDistance,
                Time.deltaTime * collisionZoomSpeed
            );

            if (showCollisionGizmos)
            {
                Debug.DrawRay(origin, direction * targetDistance, Color.green);
            }

            return targetCollisionDistance;
        }
    }

    private void UpdateCameraRotation()
    {
        // La camera guarda sempre verso il pivot point (occhi del player)
        Vector3 lookDirection = pivotPosition - cameraHolder.position;
        
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            cameraHolder.rotation = Quaternion.Slerp(
                cameraHolder.rotation,
                targetRotation,
                Time.deltaTime * rotationSmoothSpeed
            );
        }
    }

    /// <summary>
    /// Imposta la distanza della camera
    /// </summary>
    public void SetDistance(float distance)
    {
        targetDistance = Mathf.Max(distance, 1f);
    }

    /// <summary>
    /// Ottiene la distanza corrente
    /// </summary>
    public float GetCurrentDistance()
    {
        return currentDistance;
    }

    /// <summary>
    /// Check se la camera sta collidendo
    /// </summary>
    public bool IsColliding()
    {
        return isColliding;
    }

    /// <summary>
    /// Resetta la posizione TPS
    /// </summary>
    public void ResetPosition()
    {
        currentDistance = targetDistance;
        targetCollisionDistance = targetDistance;
        
        if (playerTransform != null)
        {
            pivotPosition = playerTransform.position + pivotOffset;
            Vector3 direction = CalculateCameraDirection();
            currentCameraPosition = pivotPosition - (direction * currentDistance);
            
            if (cameraHolder != null)
            {
                cameraHolder.position = currentCameraPosition;
            }
        }
    }

    #region Debug Gizmos

    private void OnDrawGizmos()
    {
        if (!showCollisionGizmos || !enabled)
            return;

        if (playerTransform != null)
        {
            // Disegna pivot point
            Gizmos.color = Color.yellow;
            Vector3 pivot = playerTransform.position + pivotOffset;
            Gizmos.DrawWireSphere(pivot, 0.1f);

            // Disegna camera position
            Gizmos.color = isColliding ? Color.red : Color.green;
            Gizmos.DrawWireSphere(currentCameraPosition, collisionRadius);

            // Disegna linea tra pivot e camera
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pivot, currentCameraPosition);
        }
    }

    #endregion
}
