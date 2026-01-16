using UnityEngine;
using CameraSystem;

/// <summary>
/// Controller principale per gestire il sistema camera FPS/TPS
/// Gestisce lo switching tra modalità e coordina i componenti
/// </summary>
[RequireComponent(typeof(FirstPersonCamera))]
[RequireComponent(typeof(ThirdPersonCamera))]
public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("La camera principale del giocatore")]
    public Camera mainCamera;
    
    [Tooltip("Transform del CameraHolder (parent della camera)")]
    public Transform cameraHolder;
    
    [Tooltip("Transform del player (root object)")]
    public Transform playerTransform;

    [Header("Camera Settings")]
    [Tooltip("Modalità camera iniziale")]
    public CameraMode startingMode = CameraMode.FirstPerson;
    
    [Tooltip("Tasto per switchare tra FPS e TPS")]
    public KeyCode toggleKey = KeyCode.V;
    
    [Tooltip("Velocità di transizione tra modalità")]
    public float transitionSpeed = 10f;

    [Header("Player Visuals")]
    [Tooltip("GameObject PlayerVisuals da nascondere in FPS")]
    public GameObject playerVisuals;
    
    [Tooltip("Renderer da nascondere in FPS (es. Body, Eyes)")]
    public Renderer[] hideInFPS;

    [Header("Weapon Settings")]
    [Tooltip("Transform del WeaponHolder")]
    public Transform weaponHolder;
    
    [Tooltip("Posizione locale arma in FPS")]
    public Vector3 weaponPositionFPS = new Vector3(0.4f, -0.3f, 0.5f);
    
    [Tooltip("Posizione locale arma in TPS")]
    public Vector3 weaponPositionTPS = new Vector3(0.3f, 0f, 0.3f);
    
    [Tooltip("Velocità di transizione posizione arma")]
    public float weaponTransitionSpeed = 8f;

    [Header("Debug")]
    [Tooltip("Mostra log di debug")]
    public bool showDebugLogs = false;

    // Componenti
    private FirstPersonCamera fpsCamera;
    private ThirdPersonCamera tpsCamera;
    private PlayerLook playerLook;

    // Stato
    private CameraMode currentMode;
    private bool isTransitioning = false;

    // Cache
    private Vector3 originalCameraPosition;
    private Vector3 originalWeaponPosition;

    #region Unity Lifecycle

    private void Awake()
    {
        // Ottieni componenti
        fpsCamera = GetComponent<FirstPersonCamera>();
        tpsCamera = GetComponent<ThirdPersonCamera>();
        playerLook = GetComponent<PlayerLook>();

        // Auto-find references se non assegnate
        AutoFindReferences();

        // Salva posizioni originali
        if (cameraHolder != null)
        {
            originalCameraPosition = cameraHolder.localPosition;
        }
        
        if (weaponHolder != null)
        {
            originalWeaponPosition = weaponHolder.localPosition;
        }

        // Inizializza componenti camera
        InitializeCameraComponents();
    }

    private void Start()
    {
        // Imposta modalità iniziale
        SetCameraMode(startingMode, instant: true);
    }

    private void Update()
    {
        // Toggle camera con input
        HandleCameraToggle();

        // Update camera corrente
        UpdateCurrentCamera();

        // Smooth weapon transition
        UpdateWeaponPosition();
    }

    private void LateUpdate()
    {
        // LateUpdate per camera (dopo movimento player)
        UpdateCameraPosition();
    }

    #endregion

    #region Initialization

    private void AutoFindReferences()
    {
        // Auto-find MainCamera
        if (mainCamera == null)
        {
            Transform cameraHolderTransform = transform.Find("CameraHolder");
            if (cameraHolderTransform != null)
            {
                mainCamera = cameraHolderTransform.GetComponentInChildren<Camera>();
                cameraHolder = cameraHolderTransform;
                
                if (showDebugLogs && mainCamera != null)
                    Debug.Log("[CameraController] MainCamera trovata automaticamente");
            }
        }

        // Auto-find PlayerTransform
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Auto-find PlayerVisuals
        if (playerVisuals == null)
        {
            Transform visualsTransform = transform.Find("PlayerVisuals");
            if (visualsTransform != null)
            {
                playerVisuals = visualsTransform.gameObject;
                
                if (showDebugLogs)
                    Debug.Log("[CameraController] PlayerVisuals trovato automaticamente");
            }
        }

        // Auto-find WeaponHolder
        if (weaponHolder == null && cameraHolder != null)
        {
            Transform weaponTransform = cameraHolder.Find("WeaponHolder");
            if (weaponTransform != null)
            {
                weaponHolder = weaponTransform;
                
                if (showDebugLogs)
                    Debug.Log("[CameraController] WeaponHolder trovato automaticamente");
            }
        }

        // Auto-find hideInFPS renderers
        if (hideInFPS == null || hideInFPS.Length == 0)
        {
            if (playerVisuals != null)
            {
                hideInFPS = playerVisuals.GetComponentsInChildren<Renderer>();
                
                if (showDebugLogs)
                    Debug.Log($"[CameraController] {hideInFPS.Length} renderer trovati automaticamente");
            }
        }
    }

    private void InitializeCameraComponents()
    {
        // Inizializza FPS Camera
        if (fpsCamera != null)
        {
            fpsCamera.Initialize(this, mainCamera, cameraHolder);
        }

        // Inizializza TPS Camera
        if (tpsCamera != null)
        {
            tpsCamera.Initialize(this, mainCamera, cameraHolder, playerTransform);
        }

        if (showDebugLogs)
            Debug.Log("[CameraController] Componenti camera inizializzati");
    }

    #endregion

    #region Camera Mode Management

    /// <summary>
    /// Imposta la modalità camera
    /// </summary>
    public void SetCameraMode(CameraMode mode, bool instant = false)
    {
        if (currentMode == mode && !instant)
            return;

        currentMode = mode;

        // Aggiorna visibilità player
        UpdatePlayerVisibility();

        // Notifica PlayerLook
        if (playerLook != null)
        {
            playerLook.SetCameraMode(mode);
        }

        // Attiva/disattiva componenti camera
        if (fpsCamera != null)
            fpsCamera.enabled = (mode == CameraMode.FirstPerson);
        
        if (tpsCamera != null)
            tpsCamera.enabled = (mode == CameraMode.ThirdPerson);

        if (showDebugLogs)
            Debug.Log($"[CameraController] Camera mode changed to: {mode}");
    }

    /// <summary>
    /// Toggle tra FPS e TPS
    /// </summary>
    public void ToggleCameraMode()
    {
        CameraMode newMode = (currentMode == CameraMode.FirstPerson) 
            ? CameraMode.ThirdPerson 
            : CameraMode.FirstPerson;
        
        SetCameraMode(newMode);
    }

    /// <summary>
    /// Ottiene la modalità corrente
    /// </summary>
    public CameraMode GetCurrentMode()
    {
        return currentMode;
    }

    /// <summary>
    /// Check se è in prima persona
    /// </summary>
    public bool IsFirstPerson()
    {
        return currentMode == CameraMode.FirstPerson;
    }

    /// <summary>
    /// Check se è in terza persona
    /// </summary>
    public bool IsThirdPerson()
    {
        return currentMode == CameraMode.ThirdPerson;
    }

    #endregion

    #region Update Logic

    private void HandleCameraToggle()
    {
        // Disabilita toggle durante warmup o morte
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
            return;

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead())
            return;

        // Toggle con input
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCameraMode();
        }
    }

    private void UpdateCurrentCamera()
    {
        // Gli update specifici sono gestiti dai componenti
        // Questo metodo può essere usato per logica condivisa
    }

    private void UpdateCameraPosition()
    {
        // LateUpdate per camera position
        // Gestito dai componenti FirstPersonCamera e ThirdPersonCamera
    }

    private void UpdateWeaponPosition()
    {
        if (weaponHolder == null)
            return;

        // Target position in base alla modalità
        Vector3 targetPosition = (currentMode == CameraMode.FirstPerson) 
            ? weaponPositionFPS 
            : weaponPositionTPS;

        // Smooth lerp
        weaponHolder.localPosition = Vector3.Lerp(
            weaponHolder.localPosition,
            targetPosition,
            Time.deltaTime * weaponTransitionSpeed
        );
    }

    private void UpdatePlayerVisibility()
    {
        if (hideInFPS == null || hideInFPS.Length == 0)
            return;

        bool showPlayer = (currentMode == CameraMode.ThirdPerson);

        foreach (Renderer renderer in hideInFPS)
        {
            if (renderer != null)
            {
                renderer.enabled = showPlayer;
            }
        }

        if (showDebugLogs)
            Debug.Log($"[CameraController] Player visibility: {showPlayer}");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Ottiene la camera principale
    /// </summary>
    public Camera GetMainCamera()
    {
        return mainCamera;
    }

    /// <summary>
    /// Ottiene il CameraHolder
    /// </summary>
    public Transform GetCameraHolder()
    {
        return cameraHolder;
    }

    /// <summary>
    /// Resetta la camera alla posizione di default
    /// </summary>
    public void ResetCamera()
    {
        if (cameraHolder != null)
        {
            cameraHolder.localPosition = originalCameraPosition;
            cameraHolder.localRotation = Quaternion.identity;
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        if (!showDebugLogs || cameraHolder == null)
            return;

        // Disegna FPS position
        Gizmos.color = Color.green;
        Vector3 fpsPos = transform.position + transform.TransformDirection(originalCameraPosition);
        Gizmos.DrawWireSphere(fpsPos, 0.2f);

        // Disegna TPS position
        if (tpsCamera != null && currentMode == CameraMode.ThirdPerson)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mainCamera.transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, mainCamera.transform.position);
        }
    }

    #endregion
}
