using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced countdown system for match start and game timer
/// Handles both warmup countdown and match timer display
/// </summary>
public class GameCountdownManager : MonoBehaviour
{
    public static GameCountdownManager Instance { get; private set; }
    
    [Header("UI References")]
    [Tooltip("Main countdown text (for warmup)")]
    public TextMeshProUGUI countdownText;
    
    [Tooltip("Match timer text (during gameplay)")]
    public TextMeshProUGUI matchTimerText;
    
    [Header("Countdown Settings")]
    [Tooltip("Warmup countdown duration in seconds")]
    public float warmupDuration = 5f;
    
    [Tooltip("Match duration in seconds (0 = infinite)")]
    public float matchDuration = 600f; // 10 minutes
    
    [Header("Visual Effects")]
    [Tooltip("Animation curve for countdown text scaling")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.2f);
    
    [Tooltip("Colors for different countdown states")]
    public Color normalCountdownColor = Color.white;
    public Color finalCountdownColor = Color.red;
    public Color goColor = Color.green;
    
    // Events
    public event System.Action OnCountdownComplete;
    public event System.Action OnMatchTimeExpired;
    
    // State
    private enum CountdownState { Idle, Warmup, Playing, Ended }
    private CountdownState currentState = CountdownState.Idle;
    
    private float currentTime;
    private bool isCountingDown = false;
    
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
        if (countdownText == null)
        {
            Debug.LogWarning("[GameCountdownManager] Countdown text not assigned!");
        }
        
        if (matchTimerText == null)
        {
            Debug.LogWarning("[GameCountdownManager] Match timer text not assigned!");
        }
        
        // Initially hide UI elements
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (matchTimerText != null) matchTimerText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!isCountingDown) return;
        
        // Update timer based on current state
        switch (currentState)
        {
            case CountdownState.Warmup:
                UpdateWarmupCountdown();
                break;
                
            case CountdownState.Playing:
                UpdateMatchTimer();
                break;
        }
    }
    
    /// <summary>
    /// Start the warmup countdown sequence
    /// </summary>
    public void StartWarmupCountdown()
    {
        if (isCountingDown)
        {
            Debug.LogWarning("[GameCountdownManager] Countdown already in progress!");
            return;
        }
        
        StartCoroutine(WarmupSequence());
    }
    
    /// <summary>
    /// Start the match timer (call after warmup completes)
    /// </summary>
    public void StartMatchTimer()
    {
        if (matchDuration <= 0)
        {
            Debug.Log("[GameCountdownManager] Infinite match duration - timer disabled");
            return;
        }
        
        currentState = CountdownState.Playing;
        currentTime = matchDuration;
        isCountingDown = true;
        
        // Show match timer, hide countdown
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (matchTimerText != null) matchTimerText.gameObject.SetActive(true);
        
        Debug.Log($"[GameCountdownManager] Match timer started: {matchDuration} seconds");
    }
    
    /// <summary>
    /// Stop all countdowns and timers
    /// </summary>
    public void StopCountdown()
    {
        isCountingDown = false;
        currentState = CountdownState.Ended;
        
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (matchTimerText != null) matchTimerText.gameObject.SetActive(false);
    }
    
    private IEnumerator WarmupSequence()
    {
        currentState = CountdownState.Warmup;
        isCountingDown = true;
        currentTime = warmupDuration;
        
        // Show countdown UI
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.color = normalCountdownColor;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < warmupDuration)
        {
            elapsedTime += Time.deltaTime;
            float remainingTime = warmupDuration - elapsedTime;
            int countdownNumber = Mathf.CeilToInt(remainingTime);
            
            // Update countdown display
            if (countdownText != null && countdownNumber > 0)
            {
                countdownText.text = countdownNumber.ToString();
                
                // Scale animation
                float scale = scaleCurve.Evaluate(1f - (remainingTime % 1f));
                countdownText.transform.localScale = Vector3.one * scale;
                
                // Color transition as we get closer to end
                if (countdownNumber <= 2)
                {
                    countdownText.color = Color.Lerp(normalCountdownColor, finalCountdownColor, (2f - countdownNumber) / 2f);
                }
            }
            
            yield return null;
        }
        
        // Show "GO!" message
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = goColor;
            countdownText.transform.localScale = Vector3.one * 1.5f;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide countdown text
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        
        // Notify listeners
        OnCountdownComplete?.Invoke();
        
        Debug.Log("[GameCountdownManager] Warmup countdown complete!");
    }
    
    private void UpdateWarmupCountdown()
    {
        // Warmup is handled by coroutine, but we keep this for consistency
    }
    
    private void UpdateMatchTimer()
    {
        if (matchDuration <= 0) return;
        
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            // Match time expired
            currentTime = 0;
            isCountingDown = false;
            currentState = CountdownState.Ended;
            
            // Hide timer
            if (matchTimerText != null) matchTimerText.gameObject.SetActive(false);
            
            // Notify listeners
            OnMatchTimeExpired?.Invoke();
            
            Debug.Log("[GameCountdownManager] Match time expired!");
            return;
        }
        
        // Update timer display
        if (matchTimerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            
            matchTimerText.text = $"{minutes:D2}:{seconds:D2}";
            
            // Change color when time is running low
            if (currentTime <= 10f)
            {
                // Pulsing red effect for last 10 seconds
                float pulse = Mathf.PingPong(Time.time * 3f, 0.5f) + 0.5f;
                matchTimerText.color = new Color(1f, 0f, 0f, pulse);
            }
            else if (currentTime <= 60f)
            {
                matchTimerText.color = Color.yellow;
            }
            else
            {
                matchTimerText.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Get remaining match time
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, currentTime);
    }
    
    /// <summary>
    /// Check if match is currently active
    /// </summary>
    public bool IsMatchActive()
    {
        return currentState == CountdownState.Playing && isCountingDown;
    }
    
    /// <summary>
    /// Check if warmup is currently active
    /// </summary>
    public bool IsWarmupActive()
    {
        return currentState == CountdownState.Warmup && isCountingDown;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}