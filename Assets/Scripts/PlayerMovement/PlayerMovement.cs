using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement")]
    public float moveSpeed = 7.0f;
    public float groundDrag = 2.0f;
    public float jumpForce = 5.0f;
    public float jumpCooldown = 1.0f;
    public float airMultiplier = 1.0f;
    bool readyToJump;
    public float playerHeight = 2.0f;
    public LayerMask whatIsGround;
    public KeyCode jumpKey = KeyCode.Space;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    bool grounded;

    [Header("Sprint")]
    public float sprintSpeed = 10.0f;   // velocità durante lo sprint
    public KeyCode sprintKey = KeyCode.LeftShift;
    private bool isSprinting = false;
    private float walkSpeed; // per salvare la velocità normale

    [Header("Slope Settings")]
    public float maxSlopeAngle = 45f; // angolo massimo su cui il player può salire
    private RaycastHit slopeHit;
    private bool onSlope = false;

    [Header("Camera reference")]
    public PlayerCamera cameraController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        if (cameraController == null)
            cameraController = GetComponentInChildren<PlayerCamera>();

        // Salva la velocità normale
        walkSpeed = moveSpeed;
    }

    void Update()
    {
        // Ho letto che si può usare SphereCast ma non capisco cosa cambi quindi nulla
        // Nel dubbio --> grounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, playerHeight * 0.5f + 0.3f, whatIsGround);
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        MyInput();
        SpeedControl();
        rb.linearDamping = grounded ? groundDrag : 0;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Controlla sprint
        isSprinting = Input.GetKey(sprintKey);

        // Salva la velocità corrente
        moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
        // Ruota virtualmente la direzione in base alla yaw della camera
        Vector3 forward = Quaternion.Euler(0, cameraController.getYaw(), 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, cameraController.getYaw(), 0) * Vector3.right;

        moveDirection = forward * verticalInput + right * horizontalInput;

        onSlope = OnSlope();

        float slopeAngle = onSlope ? Vector3.Angle(Vector3.up, slopeHit.normal) : 0f;

        // --- GESTIONE SLOPE ---
        if (onSlope && slopeAngle <= maxSlopeAngle) // Slope fattibile
        {
            // Movimento aderente al piano
            Vector3 slopeMoveDir = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            rb.AddForce(slopeMoveDir * moveSpeed * 10f, ForceMode.Force);

            // Mantiene il player aderente al terreno, evita piccoli "salti"
            rb.AddForce(-slopeHit.normal * 80f, ForceMode.Force);
        }
        else if (onSlope && slopeAngle > maxSlopeAngle) // Caso in cui il player provi a salire su uno slope troppo ripido
        {
            // Scivolamento naturale (più dolce)
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            rb.AddForce(slideDir * 50f, ForceMode.Force);
        }
        else if (grounded && !onSlope) // Applica il movimento normale
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else // In aria
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump() => readyToJump = true;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            // slopeHit.normal è la normale della superficie sotto il player
            // Vector3.Angle(Vector3.up, slopeHit.normal) calcola l’angolo rispetto al piano orizzontale
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // Debug.Log("on slope? " + (angle > 0 && angle <= maxSlopeAngle));
            return angle > 0 && angle <= maxSlopeAngle; // true se il pendio è "salibile"
        }
        return false;
    }

    public bool getSprinting()
    {
        return isSprinting;
    }
}