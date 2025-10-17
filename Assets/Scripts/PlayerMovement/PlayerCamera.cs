using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Sensibilità mouse")]
    public float mouseSensitivity = 1000f;

    [Header("Limiti pitch")]
    public float minPitch = -90f;
    public float maxPitch = 90f;

    [Header("Riferimenti")]
    public Camera playerCamera; 

    [Header("Smoothing (Lerp)")]
    [Range(1f, 50f)] public float smoothSpeed = 15f;

    float pitch;
    float targetPitch;
    float yaw;
    float targetYaw;

    Rigidbody rb;
    bool cursorLocked = true;

    public float getYaw()
    {
        return yaw;
    }

    void Start()
    {
        LockCursor(true);

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        rb = GetComponent<Rigidbody>();

        yaw = targetYaw = transform.eulerAngles.y;
        pitch = targetPitch = playerCamera.transform.localEulerAngles.x;
    }

    void Update()
    {
        HandleCursorLock();

        if (!cursorLocked)
            return;

        // Input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Aggiorna target
        targetYaw += mouseX;
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (!cursorLocked)
            return;

        // Interpola verso i target (usando Lerp esponenziale)
        yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * smoothSpeed);
        pitch = Mathf.LerpAngle(pitch, targetPitch, Time.deltaTime * smoothSpeed);

        // Applica rotazioni
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Quaternion pitchRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (rb != null)
            rb.MoveRotation(yawRotation);
        else
            transform.rotation = yawRotation;

        playerCamera.transform.localRotation = pitchRotation;
    }

    void LockCursor(bool locked)
    {
        cursorLocked = locked;
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(false);
        else if (Input.GetMouseButtonDown(0) && !cursorLocked)
            LockCursor(true);
    }
}
