using UnityEngine;

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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sensitivityMult = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
    }

    void LateUpdate()
    {
        // Disable camera movement during warmup
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensX * sensitivityMult * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * sensitivityMult * Time.deltaTime;

        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        Quaternion targetPlayerRotation = Quaternion.Euler(0, yRot, 0);
        Quaternion targetCamRotation = Quaternion.Euler(xRot, 0, 0);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetPlayerRotation,
            smoothSpeed
        );

        cam.localRotation = Quaternion.Lerp(
            cam.localRotation,
            targetCamRotation,
            smoothSpeed
        );
    }
}