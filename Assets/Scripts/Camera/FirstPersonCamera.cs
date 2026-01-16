using UnityEngine;

/// <summary>
/// Gestisce la logica della camera in prima persona (FPS)
/// Camera posizionata nella testa del player
/// </summary>
public class FirstPersonCamera : MonoBehaviour
{
    [Header("FPS Settings")]
    [Tooltip("Posizione locale della camera rispetto al player")]
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    
    [Tooltip("Velocità di smoothing del movimento camera")]
    public float positionSmoothSpeed = 0.1f;

    [Header("Head Bob (Optional)")]
    [Tooltip("Abilita head bobbing durante il movimento")]
    public bool enableHeadBob = false;
    
    [Tooltip("Intensità del bobbing")]
    public float bobAmount = 0.05f;
    
    [Tooltip("Velocità del bobbing")]
    public float bobSpeed = 14f;

    // References
    private CameraController cameraController;
    private Camera mainCamera;
    private Transform cameraHolder;

    // Head bob
    private float bobTimer = 0f;
    private Vector3 targetCameraPosition;

    // Original position
    private Vector3 originalLocalPosition;

    /// <summary>
    /// Inizializza il componente FPS Camera
    /// </summary>
    public void Initialize(CameraController controller, Camera camera, Transform holder)
    {
        cameraController = controller;
        mainCamera = camera;
        cameraHolder = holder;

        // Salva posizione originale del CameraHolder
        if (cameraHolder != null)
        {
            originalLocalPosition = cameraHolder.localPosition;
        }
    }

    private void OnEnable()
    {
        // Quando viene attivato FPS, imposta la posizione corretta
        if (cameraHolder != null)
        {
            ResetPosition();
        }
    }

    private void LateUpdate()
    {
        if (!enabled || cameraHolder == null)
            return;

        // Update camera position
        UpdateCameraPosition();

        // Head bob (optional)
        if (enableHeadBob)
        {
            ApplyHeadBob();
        }
    }

    private void UpdateCameraPosition()
    {
        // In FPS la camera mantiene X e Z originali, modifica solo Y
        // Usa l'originalLocalPosition salvato + offset Y dal cameraOffset
        targetCameraPosition = new Vector3(
            originalLocalPosition.x,
            cameraOffset.y,
            originalLocalPosition.z
        );

        // Smooth lerp (opzionale, per transizione fluida)
        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            targetCameraPosition,
            positionSmoothSpeed
        );
    }

    private void ApplyHeadBob()
    {
        // Controlla se il player si sta muovendo
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement == null)
            return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);

        if (isMoving)
        {
            // Incrementa timer
            bobTimer += Time.deltaTime * bobSpeed;

            // Calcola bobbing (seno per movimento verticale)
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

            // Applica offset
            Vector3 bobPosition = targetCameraPosition + new Vector3(0f, bobOffset, 0f);
            cameraHolder.localPosition = bobPosition;
        }
        else
        {
            // Reset bobbing
            bobTimer = 0f;
        }
    }

    /// <summary>
    /// Resetta la posizione della camera FPS
    /// </summary>
    public void ResetPosition()
    {
        if (cameraHolder != null)
        {
            // Mantiene X e Z originali, imposta Y da cameraOffset
            targetCameraPosition = new Vector3(
                originalLocalPosition.x,
                cameraOffset.y,
                originalLocalPosition.z
            );
            cameraHolder.localPosition = targetCameraPosition;
        }
    }
}
