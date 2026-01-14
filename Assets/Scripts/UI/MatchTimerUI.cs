using UnityEngine;
using TMPro;

/// <summary>
/// Displays the match countdown timer on screen
/// Connects to MatchManager to show remaining time
/// </summary>
public class MatchTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text element that displays the remaining time")]
    public TextMeshProUGUI timerText;
    
    [Header("Display Settings")]
    [Tooltip("Format string for time display. Use {0} for minutes, {1} for seconds")]
    public string timeFormat = "{0}:{1}";
    
    [Tooltip("Show minutes as two digits (e.g., 01 instead of 1)")]
    public bool padMinutes = true;
    
    [Tooltip("Show seconds as two digits (e.g., 05 instead of 5)")]
    public bool padSeconds = true;
    
    [Header("Warning Settings")]
    [Tooltip("Time remaining (in seconds) to show warning color")]
    public float warningTime = 60f;
    
    [Tooltip("Color to use when time is running low")]
    public Color warningColor = Color.red;
    
    [Tooltip("Time remaining (in seconds) to show critical color")]
    public float criticalTime = 10f;
    
    [Tooltip("Color to use when time is critical")]
    public Color criticalColor = new Color(1f, 0f, 0f, 1f);
    
    private Color normalColor;
    private MatchManager matchManager;
    
    private void Awake()
    {
        // Get normal color from the text component
        if (timerText != null)
        {
            normalColor = timerText.color;
        }
    }
    
    private void Start()
    {
        // Find MatchManager instance
        matchManager = MatchManager.Instance;
        
        if (matchManager == null)
        {
            Debug.LogError("[MatchTimerUI] MatchManager instance not found!");
            enabled = false;
            return;
        }
        
        if (timerText == null)
        {
            Debug.LogError("[MatchTimerUI] Timer text reference not set!");
            enabled = false;
            return;
        }
        
        Debug.Log("[MatchTimerUI] Timer UI initialized");
    }
    
    private void Update()
    {
        if (matchManager == null || timerText == null) return;
        
        // Check if match is active
        if (!matchManager.IsMatchActive)
        {
            timerText.gameObject.SetActive(false);
            return;
        }
        
        // Make sure timer is visible during match
        if (!timerText.gameObject.activeSelf)
        {
            timerText.gameObject.SetActive(true);
        }
        
        // Get remaining time
        float remainingTime = matchManager.RemainingTime;
        
        // Format and display time
        UpdateTimerDisplay(remainingTime);
        
        // Update color based on remaining time
        UpdateTimerColor(remainingTime);
    }
    
    private void UpdateTimerDisplay(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        string minuteString = padMinutes ? minutes.ToString("D2") : minutes.ToString();
        string secondString = padSeconds ? seconds.ToString("D2") : seconds.ToString();
        
        timerText.text = string.Format(timeFormat, minuteString, secondString);
    }
    
    private void UpdateTimerColor(float remainingTime)
    {
        if (remainingTime <= criticalTime)
        {
            timerText.color = criticalColor;
            
            // Add pulsing effect for critical time
            float pulse = Mathf.PingPong(Time.time * 3f, 0.5f) + 0.5f;
            timerText.color = new Color(criticalColor.r, criticalColor.g, criticalColor.b, pulse);
        }
        else if (remainingTime <= warningTime)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }
}