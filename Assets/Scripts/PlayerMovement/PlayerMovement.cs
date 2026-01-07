using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip jumpSound;     // file audio (.wav o .mp3)
    
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

    [Header("Wall Settings")]
    public LayerMask whatIsWall;
    public float wallCheckDistance = 0.6f;

    [Header("Slope Settings")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool onSlope;

    [Header("References")]
    public Transform orientation;
    public PlayerDash playerDash;
    public ParticleSystem walkingDustEffect;
    public Animator playerAnimator;

    // Variabili private
    private Rigidbody rb;
    private bool grounded;
    private bool readyToJump;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private float currentMoveSpeed;
    private bool isPlayingDust = false;
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

        HandleMovementEffects();
    }

    void FixedUpdate()
    {
        MovePlayer();
        CheckWallSlide();
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

    void CheckWallSlide()
    {
        // Se stai facendo il dash, non fare nulla
        if (playerDash != null && playerDash.IsDashing)
        {
            return;
        }

        // Controlla se c'è un muro davanti/dietro/laterale
        bool touchingWall = Physics.Raycast(transform.position, orientation.forward, wallCheckDistance, whatIsWall) ||
                            Physics.Raycast(transform.position, -orientation.forward, wallCheckDistance, whatIsWall) ||
                            Physics.Raycast(transform.position, orientation.right, wallCheckDistance, whatIsWall) ||
                            Physics.Raycast(transform.position, -orientation.right, wallCheckDistance, whatIsWall);
        // Se tocchi un muro e non sei a terra, rimuovi friction simulando slide
        if (touchingWall && !grounded)
        {
            // Forza verso il basso per far scivolare il player
            rb.AddForce(Vector3.down * 15f, ForceMode.Force);
        }
    }

    void MovePlayer()
    {
        if (playerDash != null && playerDash.IsDashing)
        {
            return;
        }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        onSlope = CheckOnSlope();

        if (onSlope)
        {
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

            if (slopeAngle <= maxSlopeAngle) // Pendenza salibile
            {
                Vector3 slopeMoveDir = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
                rb.AddForce(slopeMoveDir * currentMoveSpeed * 10f, ForceMode.Force);

                // Applica forza verso il basso SOLO se il player si sta muovendo
                // E riduci la forza in discesa
                if (moveDirection.magnitude > 0.1f)
                {
                    // Calcola se stiamo salendo o scendendo
                    float slopeDirection = Vector3.Dot(moveDirection.normalized, -slopeHit.normal);

                    // Se stiamo salendo (slopeDirection > 0), usa forza piena
                    // Se stiamo scendendo (slopeDirection < 0), riduci forza
                    float slopeForceMultiplier = slopeDirection > 0 ? 1f : 0.3f;
                    rb.AddForce(-slopeHit.normal * 80f * slopeForceMultiplier, ForceMode.Force);
                }
            }
            else // Pendenza troppo ripida
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
        
        // Sounds
        audioSource.PlayOneShot(jumpSound);
    }

    // Funzione per il cooldown del salto
    void ResetJump()
    {
        readyToJump = true;
    }

    // Per controllare le pendenze
    private bool CheckOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle > 0.1f && angle <= maxSlopeAngle; // Evita false positive su terreno piatto
        }
        return false;
    }

    private void HandleMovementEffects() // ← Rinominato
    {
        // Controlla se il player sta camminando
        bool isMoving = horizontalInput != 0 || verticalInput != 0;

        // Condizioni per attivare effetti
        bool shouldPlayEffects = grounded && isMoving;

        if (playerDash != null && playerDash.IsDashing)
        {
            shouldPlayEffects = false;
        }

        // Effetto polvere
        if (shouldPlayEffects && !isPlayingDust)
        {
            if (walkingDustEffect != null)
            {
                walkingDustEffect.Play();
                isPlayingDust = true;
            }
        }
        else if (!shouldPlayEffects && isPlayingDust)
        {
            if (walkingDustEffect != null)
            {
                walkingDustEffect.Stop();
                isPlayingDust = false;
            }
        }

        // Animation
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isWalking", shouldPlayEffects);
        }
    }
}