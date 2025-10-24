using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float groundDrag = 2.0f;
    public float airMultiplier = 1.0f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpCooldown = 1.0f;

    [Header("Ground Check")]
    public float playerHeight = 2.0f;
    public LayerMask whatIsGround;

    [Header("Slope Settings")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool onSlope;

    [Header("References")]
    public Transform orientation;
    public PlayerDash playerDash;

    // Variabili private
    private Rigidbody rb;
    private bool grounded;
    private bool readyToJump;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private float currentMoveSpeed;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        if (playerDash == null)
        {
            Debug.LogError("Riferimento a PlayerDash mancante su PlayerMovement!");
        }
    }

    void Update()
    {
        // Controllo se sei a terra
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();

        rb.linearDamping = grounded ? groundDrag : 0;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Controlla sprint
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentMoveSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Logica di salto con cooldown
        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
        // Se il giocatore sta scattando, non applicare il movimento normale
        // Questo permette all'impulso del dash di non essere cancellato
        if (playerDash != null && playerDash.IsDashing)
        {
            return; // Salta il resto della funzione
        }

        // Calcola la direzione di movimento
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Controlla se siamo su una pendenza
        onSlope = CheckOnSlope();

        if (onSlope)
        {
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

            if (slopeAngle <= maxSlopeAngle) // Pendenza salibile
            {
                Vector3 slopeMoveDir = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
                rb.AddForce(slopeMoveDir * currentMoveSpeed * 10f, ForceMode.Force);

                // Forza per tenere il player "incollato" alla pendenza
                rb.AddForce(-slopeHit.normal * 80f, ForceMode.Force);
            }
            else // Pendenza troppo ripida (scivola)
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                rb.AddForce(slideDir * 50f, ForceMode.Force);
            }
        }
        else if (grounded) // Movimento normale a terra
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f, ForceMode.Force);
        }
        else // Movimento in aria
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }
    
    // Funzione per limitare la velocità (dallo script di esempio)
    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    // Funzione di salto
    void Jump()
    {
        // Resetta la velocità y per un salto consistente
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Funzione per il cooldown del salto
    void ResetJump()
    {
        readyToJump = true;
    }

    // Per controllare le pendenze
    private bool CheckOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle > 0; // È su una pendenza se l'angolo è maggiore di 0
        }
        return false;
    }
}