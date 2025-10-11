using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;

    private PlayerMovement playerMovement;
    private bool isDashing;
    private float lastDashTime = -10f;
    private float lastTapTime = -Mathf.Infinity;
    private Vector3 lastInputDirection;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!isDashing)
        {
            CheckDashInput();
        }
    }

    private void CheckDashInput()
    {
        float currentTime = Time.time;
        Vector3 inputDirection = GetInputDirection();
        if (inputDirection != Vector3.zero && Input.anyKeyDown)
        {
            // Se è un doppio tap e il cooldown è finito
            if (currentTime - lastTapTime < doubleTapTime && currentTime - lastDashTime > dashCooldown)
            {
                StartCoroutine(DoDash(lastInputDirection));
            }
            lastTapTime = currentTime;
            lastInputDirection = inputDirection;
        }
    }

    // Rileva input per qualsiasi tasto di movimento e ne ricava la direzione in cui si sta muovendo il player
    private Vector3 GetInputDirection()
    {
        // Calcola la direzione basata sull'input attuale e l'orientamento del giocatore
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 direction = transform.right * moveX + transform.forward * moveZ;
        return direction.normalized;
    }

    private IEnumerator DoDash(Vector3 direction)
    {
        isDashing = true;
        lastDashTime = Time.time;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            playerMovement.ApplyDash(direction.normalized * (dashDistance / dashDuration));
            yield return null;
        }

        isDashing = false;
    }
}