using System.Collections; // Necessario per IEnumerator
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip dashSound;     // file audio (.wav o .mp3)
    
    [Header("Dashing")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f; // Durata in secondi del dash
    public float dashCooldown = 1f;
    public Transform cameraTransform; // <-- ASSEGNA LA MAIN CAMERA QUI

    private Rigidbody rb;
    private bool dashRequested;
    private float nextDashTime = 0f;

    // Proprietà pubblica per far sapere agli altri script se stiamo scattando
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
        // Disable dashing during warmup
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
        {
            return;
        }

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
            HandleDash();
            dashRequested = false;
        }
    }

    private void HandleDash()
    {
        if (cameraTransform != null)
        {
            // Imposta lo stato di dash
            IsDashing = true;

            // Applica l'impulso
            rb.AddForce(cameraTransform.forward * dashForce, ForceMode.Impulse);
            
            // Riproduci l'audio
            audioSource.PlayOneShot(dashSound);

            // Avvia la coroutine per resettare lo stato dopo 'dashDuration'
            StartCoroutine(StopDash());
        }
    }

    private IEnumerator StopDash()
    {
        // Attendi la fine della durata del dash
        yield return new WaitForSeconds(dashDuration);
        
        // Resetta lo stato
        IsDashing = false;
    }
}