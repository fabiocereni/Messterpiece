using UnityEngine;

// Richiede che il GameObject abbia un componente CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float runSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    // Riferimento al Transform della camera (deve essere assegnato nell'Inspector)
    public Transform cameraTransform;
    public float mouseSensitivity = 1000f;
    // Limite massimo dell'angolo verticale della camera (per evitare capovolgimenti)
    public float maxVerticalAngle = 80f;

    private CharacterController controller;
    // Vettore per gestire la velocità (soprattutto per la gravità e il salto)
    private Vector3 velocity;
    // Indica se il giocatore è a contatto con il suolo
    private bool isGrounded;
    // Velocità corrente del giocatore (camminata o corsa)
    private float currentSpeed;
    // Angolo di rotazione verticale della camera
    private float rotationX = 0f;
    private int jumpCount = 0;
    private int maxExtraJumps = 1;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Blocca il cursore al centro dello schermo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    void MovePlayer()
    {
        isGrounded = controller.isGrounded;

        // Resetta la velocità verticale se il giocatore è a terra per evitare accumuli
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calcola il vettore di movimento basato sulla direzione del giocatore
        // transform.right e transform.forward assicurano che il movimento sia relativo all'orientamento del giocatore
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Imposta la velocità corrente: corsa se premuto Shift, altrimenti camminata
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Muove il giocatore applicando il vettore di movimento, la velocità e il deltaTime per uniformità
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Controllo per tasto "Salto" e per "Double Jump"
        if (Input.GetButtonDown("Jump") && jumpCount < maxExtraJumps)
        {
            // Calcola la velocità verticale necessaria per il salto usando la formula fisica
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;
        }

        if (isGrounded) jumpCount = 0;


        // Applica la gravità alla velocità verticale
        velocity.y += gravity * Time.deltaTime;

        // Muove il giocatore lungo l'asse verticale (per gravità o salto)
        controller.Move(velocity * Time.deltaTime);
    }

    void RotateCamera()
    {
        // Legge il movimento orizzontale del mouse (spostamento sinistra/destra)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        // Legge il movimento verticale del mouse (spostamento su/giù)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ruota il GameObject del giocatore attorno all'asse Y per il movimento orizzontale
        transform.Rotate(Vector3.up * mouseX);

        // Aggiorna l'angolo verticale della camera (inverso)
        rotationX -= mouseY;

        // Limita l'angolo verticale per evitare che la camera si capovolga
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);

        // Applica la rotazione verticale alla camera (solo sull'asse X)
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}