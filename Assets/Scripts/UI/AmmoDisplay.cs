using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the circular ammo indicator UI
/// Displays ammo as a radial fill and animates reload
/// </summary>
public class AmmoDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the Gun script (drag Gun GameObject here)")]
    public Gun gunScript;

    [Tooltip("UI Image component (must be set to Filled - Radial 360)")]
    public Image ammoWheel;

    [Tooltip("TextMeshPro text for low ammo warning (optional)")]
    public TextMeshProUGUI reloadPromptText;

    [Header("Visual Settings")]
    [Tooltip("Color when ammo is full")]
    public Color fullColor = Color.cyan;

    [Tooltip("Color when ammo is low (< 30%)")]
    public Color lowColor = Color.red;

    [Tooltip("Color during reload animation")]
    public Color reloadColor = Color.yellow;

    [Header("Animation Settings")]
    [Tooltip("Smoothing speed for UI updates")]
    public float smoothSpeed = 10f;

    [Header("Low Ammo Warning")]
    [Tooltip("Ammo threshold to show reload prompt (0.3 = 30%)")]
    [Range(0f, 1f)]
    public float lowAmmoThreshold = 0.3f;

    [Tooltip("Flashing speed (times per second)")]
    public float flashSpeed = 2f;

    [Tooltip("Minimum alpha during flash (0 = invisible)")]
    [Range(0f, 1f)]
    public float minFlashAlpha = 0.2f;

    private float targetFillAmount = 1f;
    private float currentDisplayFill = 1f;
    private Coroutine reloadCoroutine;
    private Coroutine flashCoroutine;
    private bool isShowingWarning = false;

    void Start()
    {
        // Validation
        if (gunScript == null)
        {
            Debug.LogError("[AmmoDisplay] Gun script reference is missing! Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        if (ammoWheel == null)
        {
            Debug.LogError("[AmmoDisplay] Ammo Wheel Image reference is missing! Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        // Verify Image is set to Filled
        if (ammoWheel.type != Image.Type.Filled)
        {
            Debug.LogWarning("[AmmoDisplay] Ammo Wheel Image should be set to 'Filled' type! Setting it now...");
            ammoWheel.type = Image.Type.Filled;
            ammoWheel.fillMethod = Image.FillMethod.Radial360;
        }

        // Initialize
        currentDisplayFill = gunScript.GetAmmoFillAmount();
        ammoWheel.fillAmount = currentDisplayFill;
        UpdateColor();

        // Hide reload prompt at start
        if (reloadPromptText != null)
        {
            reloadPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (gunScript == null || ammoWheel == null) return;

        // Check if reloading
        if (gunScript.IsReloading())
        {
            // Hide reload prompt during reload
            HideReloadPrompt();

            // Start reload animation if not already running
            if (reloadCoroutine == null)
            {
                reloadCoroutine = StartCoroutine(AnimateReload());
            }
        }
        else
        {
            // Normal ammo display (shooting)
            targetFillAmount = gunScript.GetAmmoFillAmount();

            // Smooth transition
            currentDisplayFill = Mathf.Lerp(currentDisplayFill, targetFillAmount, Time.deltaTime * smoothSpeed);
            ammoWheel.fillAmount = currentDisplayFill;

            // Update color based on ammo level
            UpdateColor();

            // Show/hide reload prompt based on ammo level
            UpdateReloadPrompt();
        }
    }

    /// <summary>
    /// Updates the color of the ammo wheel based on current ammo
    /// </summary>
    void UpdateColor()
    {
        float fillPercent = currentDisplayFill;

        if (fillPercent <= 0.3f)
        {
            // Low ammo - red warning
            ammoWheel.color = lowColor;
        }
        else
        {
            // Normal - interpolate from low to full color
            ammoWheel.color = Color.Lerp(lowColor, fullColor, (fillPercent - 0.3f) / 0.7f);
        }
    }

    /// <summary>
    /// Animates the reload process - smoothly fills from 0 to 1
    /// Syncs with Gun.cs reloadTime
    /// </summary>
    IEnumerator AnimateReload()
    {
        // Set reload color
        ammoWheel.color = reloadColor;

        // Start from empty
        float elapsed = 0f;
        float duration = gunScript.reloadTime;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Smooth reload fill animation
            ammoWheel.fillAmount = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        // Ensure it's fully filled
        ammoWheel.fillAmount = 1f;
        currentDisplayFill = 1f;

        // Reset color
        UpdateColor();

        reloadCoroutine = null;
    }

    /// <summary>
    /// Optional: Call this to flash the UI when taking damage or special events
    /// </summary>
    public void FlashUI(Color flashColor, float duration = 0.2f)
    {
        StartCoroutine(FlashCoroutine(flashColor, duration));
    }

    IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        Color originalColor = ammoWheel.color;
        ammoWheel.color = flashColor;

        yield return new WaitForSeconds(duration);

        ammoWheel.color = originalColor;
    }

    /// <summary>
    /// Updates the reload prompt visibility based on ammo level
    /// Shows flashing text when ammo is low
    /// </summary>
    void UpdateReloadPrompt()
    {
        if (reloadPromptText == null) return;

        float currentAmmoPercent = gunScript.GetAmmoFillAmount();

        // Show prompt if ammo is low and not already showing
        if (currentAmmoPercent <= lowAmmoThreshold && !isShowingWarning)
        {
            ShowReloadPrompt();
        }
        // Hide prompt if ammo is above threshold and currently showing
        else if (currentAmmoPercent > lowAmmoThreshold && isShowingWarning)
        {
            HideReloadPrompt();
        }
    }

    /// <summary>
    /// Shows the reload prompt and starts flashing animation
    /// </summary>
    void ShowReloadPrompt()
    {
        if (reloadPromptText == null) return;

        isShowingWarning = true;
        reloadPromptText.gameObject.SetActive(true);

        // Start flashing if not already running
        if (flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashText());
        }
    }

    /// <summary>
    /// Hides the reload prompt and stops flashing
    /// </summary>
    void HideReloadPrompt()
    {
        if (reloadPromptText == null) return;

        isShowingWarning = false;
        reloadPromptText.gameObject.SetActive(false);

        // Stop flashing coroutine
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    /// <summary>
    /// Flashing animation for reload prompt text
    /// Smoothly fades alpha in and out
    /// </summary>
    IEnumerator FlashText()
    {
        if (reloadPromptText == null) yield break;

        while (isShowingWarning)
        {
            // Fade out (to minFlashAlpha)
            float elapsed = 0f;
            float duration = 1f / flashSpeed / 2f; // Half cycle

            Color startColor = reloadPromptText.color;
            startColor.a = 1f;
            Color targetColor = startColor;
            targetColor.a = minFlashAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                reloadPromptText.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            // Fade in (back to full alpha)
            elapsed = 0f;
            startColor.a = minFlashAlpha;
            targetColor.a = 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                reloadPromptText.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
        }
    }
}
