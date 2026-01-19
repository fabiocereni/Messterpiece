using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// Gestisce la schermata di morte con countdown e opzioni
// Si collega automaticamente al PlayerRespawn
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
    
    // Fa partire il respawn dopo il countdown
    private void StartRespawnProcess()
    {
        StartCoroutine(RespawnCountdown());
    }
    
    // Ottiene i componenti necessari
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
    
    // Inizializza la schermata di morte
    private void InitializeDeathScreen()
    {
        // Configura pulsante menu se presente
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
            returnToMenuButton.gameObject.SetActive(false);
        }
    }
    
    // Trova il PlayerRespawn nella scena
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
    
    // Mostra la schermata di morte con countdown
    public void ShowDeathScreen()
    {
        ShowDeathScreen(respawnDelay);
    }
    
    // Mostra la schermata di morte con countdown personalizzato
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
    
    // Nasconde la schermata di morte
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
    
    // Coroutine per il respawn countdown
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
    
    // Aggiorna il display del countdown
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
    
    // Permette a PlayerRespawn di aggiornare il countdown
    public void SetCountdown(float countdown)
    {
        currentCountdown = countdown;
    }
    
    // Coroutine per fade-in
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
    
    // Mostra pulsante menu con delay
    private IEnumerator ShowMenuButtonWithDelay()
    {
        yield return new WaitForSeconds(showMenuButtonAfter);
        
        if (returnToMenuButton != null && isShowingDeathScreen)
        {
            returnToMenuButton.gameObject.SetActive(true);
        }
    }
    
    // Ritorna al menu principale
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
    
    // Forza respawn immediato
    public void ForceRespawn()
    {
        if (playerRespawn != null && isShowingDeathScreen)
        {
            HideDeathScreen();
            // Il respawn verrà gestito dal PlayerRespawn
        }
    }
    
    // Imposta il testo di morte personalizzato
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