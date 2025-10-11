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
    private float lastTapW, lastTapA, lastTapS, lastTapD;

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

        if (Input.GetKeyDown(KeyCode.W) && playerMovement.IsGrounded)
        {
            if (currentTime - lastTapW < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(transform.forward));
            lastTapW = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.S) && playerMovement.IsGrounded)
        {
            if (currentTime - lastTapS < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(-transform.forward));
            lastTapS = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.A) && playerMovement.IsGrounded)
        {
            if (currentTime - lastTapA < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(-transform.right));
            lastTapA = currentTime;
        }

        if (Input.GetKeyDown(KeyCode.D) && playerMovement.IsGrounded)
        {
            if (currentTime - lastTapD < doubleTapTime && currentTime - lastDashTime > dashCooldown)
                StartCoroutine(DoDash(transform.right));
            lastTapD = currentTime;
        }
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