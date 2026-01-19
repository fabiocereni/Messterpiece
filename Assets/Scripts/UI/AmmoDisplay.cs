using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        if (ammoWheel.type != Image.Type.Filled)
        {
            Debug.LogWarning("[AmmoDisplay] Ammo Wheel Image should be set to 'Filled' type! Setting it now...");
            ammoWheel.type = Image.Type.Filled;
            ammoWheel.fillMethod = Image.FillMethod.Radial360;
        }

        currentDisplayFill = gunScript.GetAmmoFillAmount();
        ammoWheel.fillAmount = currentDisplayFill;
        UpdateColor();

        if (reloadPromptText != null)
        {
            reloadPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (gunScript == null || ammoWheel == null) return;

        if (gunScript.IsReloading())
        {
            HideReloadPrompt();

            if (reloadCoroutine == null)
            {
                reloadCoroutine = StartCoroutine(AnimateReload());
            }
        }
        else
        {
            targetFillAmount = gunScript.GetAmmoFillAmount();

            currentDisplayFill = Mathf.Lerp(currentDisplayFill, targetFillAmount, Time.deltaTime * smoothSpeed);
            ammoWheel.fillAmount = currentDisplayFill;

            UpdateColor();

            UpdateReloadPrompt();
        }
    }

    void UpdateColor()
    {
        float fillPercent = currentDisplayFill;

        if (fillPercent <= 0.3f)
        {
            ammoWheel.color = lowColor;
        }
        else
        {
            ammoWheel.color = Color.Lerp(lowColor, fullColor, (fillPercent - 0.3f) / 0.7f);
        }
    }

    IEnumerator AnimateReload()
    {
        ammoWheel.color = reloadColor;

        float elapsed = 0f;
        float duration = gunScript.reloadTime;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            ammoWheel.fillAmount = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        ammoWheel.fillAmount = 1f;
        currentDisplayFill = 1f;

        UpdateColor();

        reloadCoroutine = null;
    }

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

    void UpdateReloadPrompt()
    {
        if (reloadPromptText == null) return;

        float currentAmmoPercent = gunScript.GetAmmoFillAmount();

        if (currentAmmoPercent <= lowAmmoThreshold && !isShowingWarning)
        {
            ShowReloadPrompt();
        }
        else if (currentAmmoPercent > lowAmmoThreshold && isShowingWarning)
        {
            HideReloadPrompt();
        }
    }

    void ShowReloadPrompt()
    {
        if (reloadPromptText == null) return;

        isShowingWarning = true;
        reloadPromptText.gameObject.SetActive(true);

        if (flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashText());
        }
    }

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

    IEnumerator FlashText()
    {
        if (reloadPromptText == null) yield break;

        while (isShowingWarning)
        {
            float elapsed = 0f;
            float duration = 1f / flashSpeed / 2f;

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
