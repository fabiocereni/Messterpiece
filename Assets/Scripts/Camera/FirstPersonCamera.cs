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
    public bool enableHeadBob = false; // false: la testa rimane ferma
    
    [Tooltip("Intensità del bobbing")]
    public float bobAmount = 0.05f; // intensità del bobbing
    
    [Tooltip("Velocità del bobbing")]
    public float bobSpeed = 14f; // velocità del bobbing

    private CameraController cameraController;
    private Camera mainCamera;
    private Transform cameraHolder;

    // Head bob
    private float bobTimer = 0f;
    private Vector3 targetCameraPosition;

    // posizione originale della camera
    private Vector3 originalLocalPosition;

    public void Initialize(CameraController controller, Camera camera, Transform holder)
    {
        cameraController = controller;
        mainCamera = camera; // telecamera principale
        cameraHolder = holder; // oggetto che contiene la camera

        // salva posizione originale del CameraHolder
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

    // lateUpdate viene chiamato dopo tutti gli Update, quindi prima esegue i movimenti del player, poi aggiorna la camera
    private void LateUpdate()
    {
        if (!enabled || cameraHolder == null)
            return;

        // posiziono la camera al posto giusto
        UpdateCameraPosition();

        // applico head bobbing se abilitato
        if (enableHeadBob)
        {
            ApplyHeadBob();
        }
    }

    private void UpdateCameraPosition()
    {
        // mantiene X e Z originali, imposta Y da cameraOffset
        targetCameraPosition = new Vector3(
            originalLocalPosition.x,
            cameraOffset.y,
            originalLocalPosition.z
        );

        // linear interpolation permette di rendere il movimento fluido
        cameraHolder.localPosition = Vector3.Lerp(
            cameraHolder.localPosition,
            targetCameraPosition,
            positionSmoothSpeed
        );
    }

    private void ApplyHeadBob()
    {
        // controlla se il player si sta muovendo
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement == null)
            return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);

        if (isMoving)
        {
            // frequenza dei passi, quindi piu' alto, piu' veloce farà su e giu la testa
            bobTimer += Time.deltaTime * bobSpeed;

            // calcolo bobbing (seno per movimento verticale)
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

            // applichiamo offset
            Vector3 bobPosition = targetCameraPosition + new Vector3(0f, bobOffset, 0f);
            cameraHolder.localPosition = bobPosition;
        }
        else
        {
            // resetto il bobbing
            bobTimer = 0f;
        }
    }

    /// resetto la posizione della camera FPS
    public void ResetPosition()
    {
        if (cameraHolder != null)
        {
            // mantiene X e Z originali, imposta Y da cameraOffset
            targetCameraPosition = new Vector3(
                originalLocalPosition.x,
                cameraOffset.y,
                originalLocalPosition.z
            );
            cameraHolder.localPosition = targetCameraPosition;
        }
    }
}
