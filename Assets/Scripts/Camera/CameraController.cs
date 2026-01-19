using UnityEngine;
using CameraSystem;

// Controller principale per gestire il sistema camera FPS/TPS
// Gestisce lo switching tra modalità e coordina i componenti
[RequireComponent(typeof(FirstPersonCamera))]
[RequireComponent(typeof(ThirdPersonCamera))]
public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera FPS (figlia di CameraHolder)")]
    public Camera fpsMainCamera;

    [Tooltip("Camera TPS (separata, da creare nella scena)")]
    public Camera tpsMainCamera;

    [Tooltip("Transform del CameraHolder (parent della camera FPS)")]
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
    [Tooltip("Transform del WeaponHolder - resta sempre in posizione FPS")]
    public Transform weaponHolder;

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

    #region Unity Lifecycle

    private void Awake()
    {
        // ottieni componenti
        fpsCamera = GetComponent<FirstPersonCamera>();
        tpsCamera = GetComponent<ThirdPersonCamera>();
        playerLook = GetComponent<PlayerLook>();

        // Auto-find references se non assegnate
        AutoFindReferences();

        // salva posizioni originali
        if (cameraHolder != null)
        {
            originalCameraPosition = cameraHolder.localPosition;
        }

        // Inizializza componenti camera
        InitializeCameraComponents();
    }

    private void Start()
    {
        // imposta modalità iniziale
        SetCameraMode(startingMode, instant: true);
    }

    private void Update()
    {
        // toggle camera con input
        HandleCameraToggle();

        // update camera corrente
        UpdateCurrentCamera();
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
        // Auto-find FPS MainCamera
        if (fpsMainCamera == null)
        {
            Transform cameraHolderTransform = transform.Find("CameraHolder");
            if (cameraHolderTransform != null)
            {
                fpsMainCamera = cameraHolderTransform.GetComponentInChildren<Camera>();
                cameraHolder = cameraHolderTransform;

                if (showDebugLogs && fpsMainCamera != null)
                    Debug.Log("[CameraController] FPS MainCamera trovata automaticamente");
            }
        }

        // Auto-find TPS MainCamera
        if (tpsMainCamera == null)
        {
            GameObject tpsCamObj = GameObject.Find("TPSCamera");
            if (tpsCamObj != null)
            {
                tpsMainCamera = tpsCamObj.GetComponent<Camera>();

                if (showDebugLogs && tpsMainCamera != null)
                    Debug.Log("[CameraController] TPS MainCamera trovata automaticamente");
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
            fpsCamera.Initialize(this, fpsMainCamera, cameraHolder);
        }

        // Inizializza TPS Camera
        if (tpsCamera != null)
        {
            tpsCamera.Initialize(this, tpsMainCamera, playerTransform);
        }

        if (showDebugLogs)
            Debug.Log("[CameraController] Componenti camera inizializzati");
    }

    #endregion

    #region Camera Mode Management

    // imposto la modalità camera
    public void SetCameraMode(CameraMode mode, bool instant = false)
    {
        if (currentMode == mode && !instant)
            return;

        currentMode = mode;

        // aggiorno visibilità player
        UpdatePlayerVisibility();

        // notifica PlayerLook
        if (playerLook != null)
        {
            playerLook.SetCameraMode(mode);
        }

        // attiva/disattiva componenti camera
        if (fpsCamera != null)
            fpsCamera.enabled = (mode == CameraMode.FirstPerson);

        if (tpsCamera != null)
            tpsCamera.enabled = (mode == CameraMode.ThirdPerson);

        // attiva/disattiva le camere stesse
        if (fpsMainCamera != null)
            fpsMainCamera.enabled = (mode == CameraMode.FirstPerson);

        if (tpsMainCamera != null)
            tpsMainCamera.enabled = (mode == CameraMode.ThirdPerson);

        // quando si passa a TPS, resetta la rotazione del CameraHolder
        // questo evita che l'arma resti puntata nella direzione FPS precedente
        if (mode == CameraMode.ThirdPerson && cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.identity;
        }

        // aggiorno Gun.cs per usare la camera attiva
        UpdateGunCameraReference();

        if (showDebugLogs)
            Debug.Log($"[CameraController] Camera mode changed to: {mode}");
    }

    // toggle tra FPS e TPS
    public void ToggleCameraMode()
    {
        CameraMode newMode = (currentMode == CameraMode.FirstPerson) 
            ? CameraMode.ThirdPerson 
            : CameraMode.FirstPerson;
        
        SetCameraMode(newMode);
    }

    // ottiene la modalità corrente
    public CameraMode GetCurrentMode()
    {
        return currentMode;
    }

    // controllo se è in prima persona
    public bool IsFirstPerson()
    {
        return currentMode == CameraMode.FirstPerson;
    }

    // controllo se è in terza persona
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
 
    }

    private void UpdateCameraPosition()
    {
  
    }


    private void UpdatePlayerVisibility()
    {
        bool showPlayer = (currentMode == CameraMode.ThirdPerson);

        // aggiorna i renderer nell'array hideInFPS (metodo legacy)
        if (hideInFPS != null && hideInFPS.Length > 0)
        {
            foreach (Renderer renderer in hideInFPS)
            {
                if (renderer != null)
                {
                    renderer.enabled = showPlayer;
                }
            }
        }

        // aggiorna PlayerBodyVisibility (gestisce anche ShadowsOnly mode)
        PlayerBodyVisibility bodyVisibility = GetComponent<PlayerBodyVisibility>();
        if (bodyVisibility != null)
        {
            bodyVisibility.SetVisible(showPlayer);
        }

        if (showDebugLogs)
            Debug.Log($"[CameraController] Player visibility: {showPlayer}");
    }


    #endregion

    #region Public API

    // ottiene la camera attiva corrente
    public Camera GetMainCamera()
    {
        if (currentMode == CameraMode.FirstPerson)
            return fpsMainCamera;
        else
            return tpsMainCamera;
    }

    // Aggiorna il reference della camera in Gun.cs
    private void UpdateGunCameraReference()
    {
        Gun gun = GetComponent<Gun>();
        if (gun != null)
        {
            Camera activeCamera = GetMainCamera();
            if (activeCamera != null)
            {
                gun.fpsCam = activeCamera;

                if (showDebugLogs)
                    Debug.Log($"[CameraController] Gun camera updated to: {activeCamera.name}");
            }
        }
    }

    // Ottiene il CameraHolder
    public Transform GetCameraHolder()
    {
        return cameraHolder;
    }

    // resetta la camera alla posizione di default
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
        if (!showDebugLogs)
            return;

        // Disegna FPS position
        if (fpsMainCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(fpsMainCamera.transform.position, 0.2f);
        }

        // Disegna TPS position
        if (tpsMainCamera != null && currentMode == CameraMode.ThirdPerson)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(tpsMainCamera.transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, tpsMainCamera.transform.position);
        }
    }

    #endregion
}
