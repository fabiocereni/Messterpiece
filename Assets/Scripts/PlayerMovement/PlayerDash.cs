using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 50f;            // intensità dello scatto
    public float dashDuration = 0.3f;        // durata in secondi
    public float dashCooldown = 1.0f;        // tempo di ricarica
    public KeyCode dashKey = KeyCode.E;      // tasto per dash

    [Header("References")]
    public PlayerCamera cameraController;    // direzione basata sulla camera

    private Rigidbody rb;
    private bool canDash = true;
    private bool isDashing = false;

    private Vector3 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraController == null)
            cameraController = GetComponentInChildren<PlayerCamera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Calcola direzione del dash (basata sulla camera)
        // Se y = 0 (colpa di unity) --> dal padre (il Player), Unity spesso restituisce la direzione già “compensata” dal vincolo locale, risultando piatta
        dashDirection = cameraController.playerCamera.transform.forward;
        Debug.Log("Dash direction: " + dashDirection);

        // Disattiva momentaneamente la gravità (facoltativo)
        rb.useGravity = false;

        float elapsed = 0f;

        // Applica forza costante per tutta la durata del dash
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = Vector3.zero; // reset per evitare accumulo
            rb.AddForce(dashDirection * dashForce, ForceMode.Acceleration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Riattiva gravità
        rb.useGravity = true;

        isDashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}
