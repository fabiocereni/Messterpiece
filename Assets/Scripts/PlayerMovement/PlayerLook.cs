using UnityEngine;
using CameraSystem;

public class PlayerLook : MonoBehaviour
{
    public float sensX = 200f;
    public float sensY = 200f;
    public Transform cam;

    // Un valore più basso = più fluido.
    public float smoothSpeed = 0.1f;

    private float xRot;
    private float yRot;

    private float sensitivityMult = 1.0f;

    // Camera Controller integration
    private CameraController cameraController;
    private CameraMode currentCameraMode;

    /// <summary>
    /// Resetta la rotazione del giocatore per guardare in una direzione specifica
    /// </summary>
    public void ResetRotation(Vector3 forwardDirection)
    {
        // Calcola la rotazione Y dall'forward direction
        yRot = Mathf.Atan2(forwardDirection.x, forwardDirection.z) * Mathf.Rad2Deg;
        xRot = 0f; // Resetta la rotazione verticale

        // Applica immediatamente la rotazione
        transform.rotation = Quaternion.Euler(0, yRot, 0);
        if (cam != null)
        {
            cam.localRotation = Quaternion.Euler(xRot, 0, 0);
        }
    }

    /// <summary>
    /// Imposta la modalità camera corrente
    /// Chiamato da CameraController quando cambia modalità
    /// </summary>
    public void SetCameraMode(CameraMode mode)
    {
        currentCameraMode = mode;

        // Quando switcha a TPS, mantieni la rotazione corrente
        // Quando switcha a FPS, resetta la camera rotation locale
        if (mode == CameraMode.FirstPerson && cam != null)
        {
            cam.localRotation = Quaternion.Euler(xRot, 0, 0);
        }
    }

    /// <summary>
    /// Ottiene la rotazione verticale corrente (pitch)
    /// Usato da ThirdPersonCamera per sapere dove guardare
    /// </summary>
    public float GetVerticalRotation()
    {
        return xRot;
    }

    /// <summary>
    /// Ottiene la rotazione orizzontale corrente (yaw)
    /// </summary>
    public float GetHorizontalRotation()
    {
        return yRot;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sensitivityMult = PlayerPrefs.GetFloat("Sensitivity", 1.0f);

        // Ottieni reference al CameraController
        cameraController = GetComponent<CameraController>();

        if (cameraController != null)
        {
            currentCameraMode = cameraController.GetCurrentMode();
        }
    }

    void LateUpdate()
    {
        // Disable camera movement during warmup
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
        {
            return;
        }

        // Update camera mode
        if (cameraController != null)
        {
            currentCameraMode = cameraController.GetCurrentMode();
        }

        // In TPS mode, ThirdPersonCamera gestisce tutto (mouse input + rotazione)
        // PlayerLook non deve fare nulla per evitare conflitti
        if (currentCameraMode == CameraMode.ThirdPerson)
        {
            return;
        }

        // Solo in FPS mode: processa input e applica rotazione
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensX * sensitivityMult * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * sensitivityMult * Time.deltaTime;

        // Update rotations
        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        // Applica rotazione FPS
        ApplyFirstPersonRotation();
    }

    private void ApplyRotation()
    {
        if (currentCameraMode == CameraMode.FirstPerson)
        {
            // FPS: Player ruota su Y, Camera ruota su X (come prima)
            ApplyFirstPersonRotation();
        }
        else
        {
            // TPS: Player ruota su Y, Camera gestita da ThirdPersonCamera
            ApplyThirdPersonRotation();
        }
    }

    private void ApplyFirstPersonRotation()
    {
        // Rotazione player (Y axis)
        Quaternion targetPlayerRotation = Quaternion.Euler(0, yRot, 0);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetPlayerRotation,
            smoothSpeed
        );

        // Rotazione camera (X axis)
        if (cam != null)
        {
            Quaternion targetCamRotation = Quaternion.Euler(xRot, 0, 0);
            cam.localRotation = Quaternion.Lerp(
                cam.localRotation,
                targetCamRotation,
                smoothSpeed
            );
        }
    }

    private void ApplyThirdPersonRotation()
    {
        // In TPS: il player ruota in base al movimento della camera
        // Ma la rotazione verticale (xRot) viene gestita dalla ThirdPersonCamera

        // Rotazione player (Y axis)
        Quaternion targetPlayerRotation = Quaternion.Euler(0, yRot, 0);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetPlayerRotation,
            smoothSpeed
        );

        // In TPS la camera gestisce la propria rotazione
        // Ma dobbiamo comunque tracciare xRot per quando si torna in FPS
    }
}