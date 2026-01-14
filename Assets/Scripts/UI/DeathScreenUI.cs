using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gestisce la schermata di morte con countdown e opzioni
/// Si collega automaticamente al PlayerRespawn
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; }
    
    [Header("UI Elements")]
    [Tooltip("Testo principale \"SEI MORTO\"")]
    public TextMeshProUGUI deathTitleText;
    
    [Tooltip("Testo countdown respawn")]
    public TextMeshProUGUI respawnCountdownText;
    
    [Tooltip("Pannello della schermata di morte")]
    public GameObject deathPanel;
    
    [Header("Respawn Settings")]
    [Tooltip("Mostra countdown automatico")]
    public bool showCountdown = true;
    
    [Tooltip("Delay respawn in secondi")]
    public float respawnDelay = 3f;
    
    [Header("Visual Settings")]
    [Tooltip("Colore testo morte")]
    public Color deathTextColor = Color.red;
    
    [Tooltip("Colore testo countdown")]
    public Color countdownColor = Color.white;
    
    [Tooltip("Effetto fade-in")]
    public bool useFadeIn = true;
    [Tooltip("Durata fade-in")]
    public float fadeInDuration = 0.5f;
    
    [Header("Optional - Game Over Button")]
    [Tooltip("Pulsante per tornare al menu (opzionale)")]
    public Button returnToMenuButton;
    
    [Tooltip("Mostra pulsante menu solo dopo X secondi")]
    public float showMenuButtonAfter = 10f;
    
    // Stato interno
    private bool isShowingDeathScreen = false;
    private float currentCountdown;
    private CanvasGroup canvasGroup;
    private PlayerRespawn playerRespawn;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Ottieni componenti
        GetRequiredComponents();
        
        // Inizializza stato
        InitializeDeathScreen();
    }
    
    private void Start()
    {
        // Trova PlayerRespawn
        FindPlayerRespawn();
        
        // Nascondi schermata all'avvio
        HideDeathScreen();
    }
    
    /// <summary>
    /// Fa partire il respawn dopo il countdown
    /// </summary>
    private void StartRespawnProcess()
    {
        StartCoroutine(RespawnCountdown());
    }
    
    /// <summary>
    /// Ottiene i componenti necessari
    /// </summary>
    private void GetRequiredComponents()
    {
        // Canvas group per effetti fade
        if (deathPanel != null)
        {
            canvasGroup = deathPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = deathPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Imposta colori iniziali
        if (deathTitleText != null)
        {
            deathTitleText.color = deathTextColor;
        }
        
        if (respawnCountdownText != null)
        {
            respawnCountdownText.color = countdownColor;
        }
    }
    
    /// <summary>
    /// Inizializza la schermata di morte
    /// </summary>
    private void InitializeDeathScreen()
    {
        // Configura pulsante menu se presente
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
            returnToMenuButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Trova il PlayerRespawn nella scena
    /// </summary>
    private void FindPlayerRespawn()
    {
        playerRespawn = FindObjectOfType<PlayerRespawn>();
        
        if (playerRespawn == null)
        {
            Debug.LogWarning("[DeathScreenUI] PlayerRespawn non trovato nella scena!");
        }
        else
        {
            Debug.Log("[DeathScreenUI] PlayerRespawn trovato - DeathScreenUI gestirà il respawnDelay");
        }
    }
    
    /// <summary>
    /// Mostra la schermata di morte con countdown
    /// </summary>
    public void ShowDeathScreen()
    {
        ShowDeathScreen(respawnDelay);
    }
    
    /// <summary>
    /// Mostra la schermata di morte con countdown personalizzato
    /// </summary>
    public void ShowDeathScreen(float customRespawnDelay)
    {
        if (isShowingDeathScreen) return;
        
        isShowingDeathScreen = true;
        currentCountdown = customRespawnDelay;
        
        // Mostra pannello
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            
            // Fade-in effect
            if (useFadeIn && canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }
        
        // Mostra testo morte
        if (deathTitleText != null)
        {
            deathTitleText.gameObject.SetActive(true);
        }
        
        // Mostra countdown
        if (showCountdown && respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(true);
        }
        
        // Mostra pulsante menu con delay
        if (returnToMenuButton != null && showMenuButtonAfter > 0)
        {
            StartCoroutine(ShowMenuButtonWithDelay());
        }
        
        // Sblocca cursore
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Inizia countdown
        StartRespawnProcess();
    }
    
    /// <summary>
    /// Nasconde la schermata di morte
    /// </summary>
    public void HideDeathScreen()
    {
        if (!isShowingDeathScreen) return;
        
        isShowingDeathScreen = false;
        
        // Nascondi tutti gli elementi
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
        
        if (deathTitleText != null)
        {
            deathTitleText.gameObject.SetActive(false);
        }
        
        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(false);
        }
        
        if (returnToMenuButton != null)
        {
            returnToMenuButton.gameObject.SetActive(false);
        }
        
        // Ripristina cursore per gioco
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Coroutine per il respawn countdown
    /// </summary>
    private IEnumerator RespawnCountdown()
    {
        while (currentCountdown > 0 && isShowingDeathScreen)
        {
            UpdateCountdownDisplay(currentCountdown);
            yield return new WaitForSeconds(1f);
            currentCountdown--;
        }
        
        // Fine countdown
        if (isShowingDeathScreen)
        {
            UpdateCountdownDisplay(0);
            yield return new WaitForSeconds(0.5f); // Breve pausa per leggere "RESPAWNING..."
            
            // Nascondi schermata
            HideDeathScreen();
            
            // Notifica PlayerRespawn che il countdown è finito e può respawnare
            if (playerRespawn != null)
            {
                // Chiama il metodo pubblico per completare il respawn
                playerRespawn.CompleteRespawn();
            }
        }
    }
    
    /// <summary>
    /// Aggiorna il display del countdown
    /// </summary>
    private void UpdateCountdownDisplay(float countdown)
    {
        if (respawnCountdownText != null)
        {
            int seconds = Mathf.CeilToInt(countdown);
            
            if (countdown <= 0)
            {
                respawnCountdownText.text = "RESPAWNING...";
            }
            else
            {
                respawnCountdownText.text = $"RESPAWN IN {seconds}...";
            }
            
            // Assicura che la scala rimanga costante
            if (respawnCountdownText.transform.localScale != Vector3.one)
            {
                respawnCountdownText.transform.localScale = Vector3.one;
            }
        }
    }
    
    /// <summary>
    /// Permette a PlayerRespawn di aggiornare il countdown
    /// </summary>
    public void SetCountdown(float countdown)
    {
        currentCountdown = countdown;
    }
    
    /// <summary>
    /// Coroutine per fade-in
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Mostra pulsante menu con delay
    /// </summary>
    private IEnumerator ShowMenuButtonWithDelay()
    {
        yield return new WaitForSeconds(showMenuButtonAfter);
        
        if (returnToMenuButton != null && isShowingDeathScreen)
        {
            returnToMenuButton.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Ritorna al menu principale
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[DeathScreenUI] Ritorno al menu principale");
        
        // Trova GameManager e ritorna al menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // Fallback: carica direttamente la scena MainMenu
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    /// <summary>
    /// Forza respawn immediato
    /// </summary>
    public void ForceRespawn()
    {
        if (playerRespawn != null && isShowingDeathScreen)
        {
            HideDeathScreen();
            // Il respawn verrà gestito dal PlayerRespawn
        }
    }
    
    /// <summary>
    /// Imposta il testo di morte personalizzato
    /// </summary>
    public void SetDeathMessage(string message)
    {
        if (deathTitleText != null)
        {
            deathTitleText.text = message;
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        // Cleanup eventi
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
        }
    }
    
    // Metodi per debug
    [ContextMenu("Test Show Death Screen")]
    public void DebugShowDeathScreen()
    {
        ShowDeathScreen();
    }
    
    [ContextMenu("Test Hide Death Screen")]
    public void DebugHideDeathScreen()
    {
        HideDeathScreen();
    }
    
    [ContextMenu("Test Return To Menu")]
    public void DebugReturnToMenu()
    {
        ReturnToMainMenu();
    }
}