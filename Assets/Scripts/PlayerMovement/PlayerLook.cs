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