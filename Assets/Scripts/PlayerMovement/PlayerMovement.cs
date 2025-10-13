using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private int maxExtraJumps = 1;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private int jumpCount;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        // Resetta la velocità verticale se a terra
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
            jumpCount = 0;
        }

        // Input movimento
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Imposta velocità (corsa o camminata)
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Muove il giocatore
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gestione salto
        if (Input.GetButtonDown("Jump") && jumpCount < maxExtraJumps)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Metodo PUBBLICO per consentire al dash di influenzare il movimento
    public void ApplyDash(Vector3 dashVelocity)
    {
        controller.Move(dashVelocity * Time.deltaTime);
    }

    // prendo PlayerMovement.IsGrounded { get; } e lo metto in isGrounded
    public bool IsGrounded => isGrounded;
}