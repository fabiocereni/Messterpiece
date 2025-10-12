using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransformerFirstPerson;
    [SerializeField] private Transform cameraTransformerThirdPerson;
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private float rotationX;

    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public bool isFirstPerson;
    
    //var per third person orbitale (vedere sotto) 
    public Transform thirdPersonPivot; // il punto attorno al quale far ruotare la camera
    public float distanceFromPivot = 5f; // distanza della camera dal pivot
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateCameraState();
        // Blocca il cursore
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Premendo V cambia visuale
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            UpdateCameraState();
        }

        RotateCamera();
    }

    private void UpdateCameraState()
    {
        // Attiva/disattiva le telecamere
        if (firstPersonCamera != null)
            firstPersonCamera.enabled = isFirstPerson;

        if (thirdPersonCamera != null)
            thirdPersonCamera.enabled = !isFirstPerson;
    }
    
    private void RotateCamera() { 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotazione orizzontale del giocatore
        transform.Rotate(Vector3.up * mouseX);

        // Rotazione verticale della camera
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);
        if(isFirstPerson)
            cameraTransformerFirstPerson.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        else
        {
            if (thirdPersonPivot == null || cameraTransformerThirdPerson == null)
                return;

            // Calcola la rotazione della camera in base al mouse
            rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(rotationX, transform.eulerAngles.y, 0f);

            // Posizione orbitale intorno al pivot
            Vector3 offset = rotation * new Vector3(0f, 0f, -distanceFromPivot);
            cameraTransformerThirdPerson.position = thirdPersonPivot.position + offset;

            // Guarda sempre il pivot
            cameraTransformerThirdPerson.LookAt(thirdPersonPivot.position);
        }
    }
}
