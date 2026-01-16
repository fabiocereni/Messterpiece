using UnityEngine;

/// <summary>
/// Gestisce la logica della camera in terza persona (TPS)
/// Include collision detection, wall avoidance, smooth follow e rotazione verticale indipendente
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("TPS Settings")]
    [Tooltip("Distanza ideale della camera dal player")]
    public float targetDistance = 3.5f;
    
    [Tooltip("Offset laterale (over-the-shoulder)")]
    public float shoulderOffset = 0.8f;

    [Header("Mouse Sensitivity")]
    [Tooltip("Sensibilità mouse orizzontale")]
    public float sensX = 200f;
    [Tooltip("Sensibilità mouse verticale")]
    public float sensY = 200f;

    [Header("Vertical Rotation Limits")]
    [Tooltip("Angolo minimo verticale (guardare in basso)")]
    public float minVerticalAngle = -40f;
    [Tooltip("Angolo massimo verticale (guardare in alto)")]
    public float maxVerticalAngle = 60f;

    [Header("Camera Smoothing")]
    [Tooltip("Velocità di smoothing della posizione")]
    public float positionSmoothSpeed = 10f;
    
    [Tooltip("Velocità di smoothing della rotazione")]
    public float rotationSmoothSpeed = 15f;

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
    private Camera tpsCamera;
    private Transform playerTransform;

    // Camera state
    private Vector3 currentCameraPosition;
    private float currentDistance;
    private Vector3 pivotPosition;

    // Rotazione indipendente per TPS
    private float yaw;   // Rotazione orizzontale
    private float pitch; // Rotazione verticale

    // Collision
    private bool isColliding = false;
    private float targetCollisionDistance;

    // Sensitivity multiplier from settings
    private float sensitivityMult = 1.0f;

    /// <summary>
    /// Inizializza il componente TPS Camera
    /// </summary>
    public void Initialize(CameraController controller, Camera camera, Transform player)
    {
        cameraController = controller;
        tpsCamera = camera;
        playerTransform = player;

        // Inizializza distanza
        currentDistance = targetDistance;
        targetCollisionDistance = targetDistance;

        // Carica sensitivity dalle impostazioni
        sensitivityMult = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
    }

    private void OnEnable()
    {
        // Quando viene attivato TPS, sincronizza con la rotazione attuale del player
        if (tpsCamera != null && playerTransform != null)
        {
            // Inizializza yaw dalla rotazione attuale del player
            yaw = playerTransform.eulerAngles.y;
            // Resetta pitch a 0 (guarda dritto)
            pitch = 0f;
            
            ResetPosition();
        }
    }

    private void LateUpdate()
    {
        if (!enabled || tpsCamera == null || playerTransform == null)
            return;

        // Processa input del mouse per rotazione
        HandleMouseInput();

        // Update camera position e collision
        UpdateCameraPosition();

        // Sincronizza la rotazione del player con la direzione della camera
        SyncPlayerRotation();

        // Sincronizza il CameraHolder (arma) con il pitch della camera
        SyncCameraHolderRotation();
    }

    private void HandleMouseInput()
    {
        // Disabilita durante warmup o morte
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
            return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensX * sensitivityMult * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * sensitivityMult * Time.deltaTime;

        // Aggiorna rotazioni
        yaw += mouseX;
        pitch -= mouseY;
        
        // Clamp pitch (limita quanto puoi guardare su/giù)
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        // 1. Calcola pivot position (punto di rotazione della camera - occhi del player)
        pivotPosition = playerTransform.position + pivotOffset;

        // 2. Calcola la direzione della camera basata su yaw e pitch
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 cameraDirection = cameraRotation * Vector3.forward;

        // 3. Calcola offset laterale in world space
        Vector3 rightOffset = cameraRotation * Vector3.right * shoulderOffset;

        // 4. Posizione target: dietro il player + offset laterale
        // La camera è DIETRO (opposta alla direzione forward) rispetto al pivot
        Vector3 targetPosition = pivotPosition - (cameraDirection * targetDistance) + rightOffset;

        // 5. Collision detection (usa la direzione dal pivot alla camera target)
        Vector3 collisionDirection = (targetPosition - pivotPosition).normalized;
        float adjustedDistance = CheckCameraCollision(pivotPosition, collisionDirection);
        
        // Ricalcola posizione con distanza aggiustata
        targetPosition = pivotPosition + (collisionDirection * adjustedDistance);

        // 6. Smooth movement
        currentCameraPosition = Vector3.Lerp(
            currentCameraPosition,
            targetPosition,
            Time.deltaTime * positionSmoothSpeed
        );

        // 7. Applica posizione alla TPS Camera
        tpsCamera.transform.position = currentCameraPosition;

        // 8. La camera guarda nella direzione calcolata
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        tpsCamera.transform.rotation = Quaternion.Slerp(
            tpsCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    private void SyncPlayerRotation()
    {
        // Il player ruota solo su Y per seguire la direzione della camera
        // Questo mantiene il player allineato con dove sta guardando la camera
        Quaternion targetPlayerRotation = Quaternion.Euler(0f, yaw, 0f);
        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            targetPlayerRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    private void SyncCameraHolderRotation()
    {
        // Sincronizza il CameraHolder (che contiene l'arma) con il pitch della camera TPS
        // Così l'arma punta dove la camera sta guardando
        if (cameraController == null)
            return;

        Transform cameraHolder = cameraController.GetCameraHolder();
        if (cameraHolder == null)
            return;

        // Il CameraHolder ruota solo su X (pitch) in local space
        // La rotazione Y è già gestita dal player stesso
        Quaternion targetRotation = Quaternion.Euler(pitch, 0f, 0f);
        cameraHolder.localRotation = Quaternion.Slerp(
            cameraHolder.localRotation,
            targetRotation,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    private float CheckCameraCollision(Vector3 origin, Vector3 direction)
    {
        // SphereCast dal pivot verso la posizione target della camera
        RaycastHit hit;
        
        float checkDistance = targetDistance + collisionRadius;
        
        // Esegui SphereCast (direzione invertita perché andiamo verso la camera, non verso il player)
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

        if (playerTransform != null && tpsCamera != null)
        {
            // Inizializza rotazioni dalla rotazione attuale del player
            yaw = playerTransform.eulerAngles.y;
            pitch = 0f;

            pivotPosition = playerTransform.position + pivotOffset;
            
            // Calcola posizione iniziale
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 cameraDirection = cameraRotation * Vector3.forward;
            Vector3 rightOffset = cameraRotation * Vector3.right * shoulderOffset;
            
            currentCameraPosition = pivotPosition - (cameraDirection * currentDistance) + rightOffset;
            tpsCamera.transform.position = currentCameraPosition;
            tpsCamera.transform.rotation = cameraRotation;
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

