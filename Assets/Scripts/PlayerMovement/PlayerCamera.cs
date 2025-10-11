using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private float rotationX;

    private void Start()
    {
        // Blocca il cursore
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotazione orizzontale del giocatore
        transform.Rotate(Vector3.up * mouseX);

        // Rotazione verticale della camera
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}