using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip dashSound;

    [Header("Dashing")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private bool dashRequested;
    private float nextDashTime = 0f;
    private float dashEndTime;

    public bool IsDashing { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform non assegnato su PlayerDash script! Assegnare la Main Camera dall'Inspector.");
        }
    }

    void Update()
    {
        // Controlli di stato (warmup, morte, match)
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove()) return;
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead()) return;
        if (MatchManager.Instance != null && !MatchManager.Instance.IsMatchActive) return;

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextDashTime)
        {
            dashRequested = true;
            nextDashTime = Time.time + dashCooldown;
        }
    }

    void FixedUpdate()
    {
        if (dashRequested)
        {
            StartDash();
            dashRequested = false;
        }

        if (IsDashing)
        {
            // Mantiene la velocità costante per la durata del dash
            if (Time.time >= dashEndTime)
            {
                IsDashing = false;
            }
            else
            {
                rb.linearVelocity = cameraTransform.forward.normalized * dashForce;
            }
        }
    }

    private void StartDash()
    {
        if (cameraTransform == null) return;

        IsDashing = true;
        dashEndTime = Time.time + dashDuration;

        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }
}
