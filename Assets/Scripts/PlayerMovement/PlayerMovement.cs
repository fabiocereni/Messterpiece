using UnityEngine;
using System.Collections;                   // per IEnumerator!

// Richiede che il GameObject abbia un componente CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float runSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public Transform cameraTransform;       // Riferimento al Transform della camera (deve essere assegnato nell'Inspector)
    public float mouseSensitivity = 1000f;
    public float maxVerticalAngle = 80f;    // Limite massimo dell'angolo verticale della camera (per evitare capovolgimenti)
    private CharacterController controller;
    private Vector3 velocity;               // Vettore per gestire la velocità (soprattutto per la gravità e il salto)
    private bool isGrounded;                // Indica se il giocatore è a contatto con il suolo
    private float currentSpeed;             // Velocità corrente del giocatore (camminata o corsa)
    private float rotationX = 0f;           // Angolo di rotazione verticale della camera
    private int jumpCount = 0;
    private int maxExtraJumps = 1;

    // PER IL DASH
    public float dashDistance = 8f;         // distanza o velocità dello scatto
    public float dashDuration = 0.15f;      // durata dello scatto
    public float doubleTapTime = 0.3f;      // tempo massimo per doppio tap
    public float dashCooldown = 1f;         // tempo di recupero tra dash
    private bool isDashing = false;
    private float lastDashTime = -10f;
    private Vector3 dashDirection;
    private float lastTapW, lastTapA, lastTapS, lastTapD;

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
        if (!isDashing)
        {
            CheckDashInput();
        }
    }

    void MovePlayer()
    {
        isGrounded = controller.isGrounded;

        // Resetta la velocità verticale se il giocatore è a terra per evitare accumuli
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Per usare le "freccette" della tastiera
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

    void CheckDashInput()
    {
        float currentTime = Time.time;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentTime - lastTapW < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(transform.forward));
            lastTapW = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentTime - lastTapS < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(-transform.forward));
            lastTapS = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentTime - lastTapA < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(-transform.right));
            lastTapA = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentTime - lastTapD < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(transform.right));
            lastTapD = currentTime;
        }
    }
    
    IEnumerator DoDash(Vector3 direction)
    {
        isDashing = true;
        lastDashTime = Time.time;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(direction.normalized * (dashDistance / dashDuration) * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }
}