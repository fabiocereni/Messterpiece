using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

/// <summary>
/// Manages the pre-match warmup sequence with countdown and grayscale-to-color transition.
/// This script handles the first 5 seconds of the match before gameplay begins.
/// </summary>
public class MatchFlowManager : MonoBehaviour
{
    public static MatchFlowManager Instance { get; private set; }

    [Header("Post-Processing")]
    [Tooltip("Assign the Global Volume that contains the Color Adjustments override")]
    public Volume globalVolume;

    [Header("UI References")]
    [Tooltip("UI Text element to display the countdown (5, 4, 3, 2, 1, GO!)")]
    public TextMeshProUGUI countdownText;

    [Header("Countdown Settings")]
    [Tooltip("Duration of the countdown in seconds")]
    public float countdownDuration = 5f;

    // Public property to check if the warmup is in progress
    public bool IsWarmupActive { get; private set; } = true;

    // Event that fires when the warmup completes
    public event System.Action OnWarmupComplete;

    private VolumeProfile volumeProfile;
    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Validate references
        if (globalVolume == null)
        {
            Debug.LogError("[MatchFlowManager] ❌ Global Volume reference is missing! Assign it in the Inspector.");
            return;
        }

        Debug.Log("[MatchFlowManager] ✓ Global Volume found: " + globalVolume.name);

        if (countdownText == null)
        {
            Debug.LogError("[MatchFlowManager] ❌ Countdown Text reference is missing! Assign it in the Inspector.");
            return;
        }

        Debug.Log("[MatchFlowManager] ✓ Countdown Text found: " + countdownText.name);

        // Get the Volume Profile and Color Adjustments component
        volumeProfile = globalVolume.profile;

        if (volumeProfile == null)
        {
            Debug.LogError("[MatchFlowManager] ❌ Volume Profile is NULL! Create a new profile in the Global Volume component.");
            return;
        }

        Debug.Log("[MatchFlowManager] ✓ Volume Profile found: " + volumeProfile.name);

        if (!volumeProfile.TryGet(out colorAdjustments))
        {
            Debug.LogError("[MatchFlowManager] ❌ Color Adjustments override not found in Volume Profile! Add it via: Volume component → Add Override → Post-processing → Color Adjustments");
            return;
        }

        Debug.Log("[MatchFlowManager] ✓ Color Adjustments found");

        // Check if saturation is overridden (enabled)
        if (!colorAdjustments.saturation.overrideState)
        {
            Debug.LogWarning("[MatchFlowManager] ⚠️ Saturation is NOT overridden! Enabling it automatically...");
            colorAdjustments.saturation.overrideState = true;
        }

        Debug.Log($"[MatchFlowManager] ✓ Saturation override enabled. Current value: {colorAdjustments.saturation.value}");

        // Start the warmup sequence
        Debug.Log("[MatchFlowManager] 🎬 Starting warmup sequence...");
        StartCoroutine(WarmupSequence());
    }

    /// <summary>
    /// Main coroutine that handles the countdown and visual transition
    /// </summary>
    private IEnumerator WarmupSequence()
    {
        IsWarmupActive = true;

        // Lock cursor during warmup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set initial grayscale (saturation = -100)
        colorAdjustments.saturation.value = -100f;
        Debug.Log($"[MatchFlowManager] 🎨 Initial saturation set to: {colorAdjustments.saturation.value}");

        float elapsedTime = 0f;
        int lastCountdownNumber = -1;

        while (elapsedTime < countdownDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / countdownDuration;

            // Interpolate saturation from -100 (grayscale) to 0 (full color)
            colorAdjustments.saturation.value = Mathf.Lerp(-100f, 0f, progress);

            // Calculate remaining time for countdown display
            float remainingTime = countdownDuration - elapsedTime;
            int countdownNumber = Mathf.CeilToInt(remainingTime);

            // Update UI text
            if (countdownNumber > 0)
            {
                countdownText.text = countdownNumber.ToString();
                countdownText.fontSize = Mathf.Lerp(80f, 120f, 1f - (remainingTime % 1f)); // Pulse effect

                // Log only when number changes
                if (countdownNumber != lastCountdownNumber)
                {
                    Debug.Log($"[MatchFlowManager] ⏱️ Countdown: {countdownNumber} | Saturation: {colorAdjustments.saturation.value:F1}");
                    lastCountdownNumber = countdownNumber;
                }
            }
            else
            {
                countdownText.text = "GO!";
                countdownText.fontSize = 150f;
            }

            yield return null;
        }

        // Ensure final values are set
        colorAdjustments.saturation.value = 0f;
        Debug.Log($"[MatchFlowManager] 🎨 Final saturation set to: {colorAdjustments.saturation.value}");

        // Brief delay to show "GO!" message
        yield return new WaitForSeconds(0.5f);

        // Hide countdown text
        countdownText.gameObject.SetActive(false);

        // Mark warmup as complete
        IsWarmupActive = false;

        // Notify listeners that warmup is complete
        OnWarmupComplete?.Invoke();

        Debug.Log("[MatchFlowManager] ✅ Warmup sequence complete! Game started.");
    }

    /// <summary>
    /// Public method to check if players should be able to move
    /// </summary>
    public bool CanPlayerMove()
    {
        return !IsWarmupActive;
    }

    private void OnDestroy()
    {
        // Reset saturation when destroyed (in case you reload the scene)
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }
    }
}
