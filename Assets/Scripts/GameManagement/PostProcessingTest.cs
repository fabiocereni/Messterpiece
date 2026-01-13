using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Simple test script to verify URP Post-Processing is working
/// Press T in Play Mode to toggle grayscale on/off
/// </summary>
public class PostProcessingTest : MonoBehaviour
{
    public Volume testVolume;
    private ColorAdjustments colorAdjustments;
    private bool isGrayscale = false;

    void Start()
    {
        if (testVolume == null)
        {
            Debug.LogError("[PostProcessingTest] ❌ Volume reference missing!");
            return;
        }

        if (!testVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("[PostProcessingTest] ❌ Color Adjustments not found in Volume!");
            return;
        }

        // Enable saturation override
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = 0f;

        Debug.Log("[PostProcessingTest] ✅ Test ready! Press T to toggle grayscale.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isGrayscale = !isGrayscale;

            if (colorAdjustments != null)
            {
                float targetSaturation = isGrayscale ? -100f : 0f;
                colorAdjustments.saturation.value = targetSaturation;

                Debug.Log($"[PostProcessingTest] 🎨 Saturation set to: {targetSaturation} (Grayscale: {isGrayscale})");
            }
        }
    }
}
