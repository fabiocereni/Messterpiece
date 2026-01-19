using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// Controller principale per la schermata di fine partita
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    
    [Header("Riferimenti UI")]
    [Tooltip("Pannello principale game over")]
    public GameObject gameOverPanel;
    
    [Tooltip("Titolo della schermata game over")]
    public TextMeshProUGUI titleText;
    
    [Tooltip("Container per la leaderboard finale")]
    public Transform leaderboardContainer;
    
    [Tooltip("Prefab per le righe della leaderboard")]
    public GameObject leaderboardRowPrefab;
    
    [Header("Pulsanti")]
    [Tooltip("Pulsante 'Gioca Ancora'")]
    public Button playAgainButton;
    
    [Tooltip("Pulsante 'Menu'")]
    public Button menuButton;
    
    [Header("Testi Pulsanti")]
    [Tooltip("Testo pulsante 'Gioca Ancora'")]
    public string playAgainText = "Gioca Ancora";
    
    [Tooltip("Testo pulsante 'Menu'")]
    public string menuText = "Menu";
    
    [Header("Impostazioni")]
    [Tooltip("Mostra statistiche del giocatore")]
    public bool showPlayerStats = true;
    
    [Tooltip("Metodo di ordinamento leaderboard")]
    public SortMethod leaderboardSort = SortMethod.ByScore;
    
    [Header("UI da Nascondere Durante GameOver")]
    [Tooltip("GameObject della barra della vita (HealthBarUI)")]
    public GameObject healthBarUI;

    [Tooltip("GameObject dell'indicatore munizioni (AmmoDisplay)")]
    public GameObject ammoDisplayUI;

    [Tooltip("GameObject del nome del player")]
    public GameObject playerNameUI;

    [Tooltip("Script della leaderboard in-game (per disabilitare TAB)")]
    public LeaderboardUI leaderboardUIScript;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Enum per ordinamento
    public enum SortMethod { ByScore, ByKills, ByKDRatio }

    // Stato interno
    private List<GameObject> spawnedRows = new List<GameObject>();
    private bool isGameOverActive = false;

    // Stati originali delle UI (per ripristinarle correttamente)
    private bool healthBarOriginalState;
    private bool ammoDisplayOriginalState;
    private bool playerNameOriginalState;
    private bool leaderboardScriptOriginalState;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Persistere tra le scene per mantenere lo stato
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();

        // Se i riferimenti UI non sono stati assegnati manualmente, prova a trovarli automaticamente
        if (healthBarUI == null || ammoDisplayUI == null || leaderboardUIScript == null)
        {
            RefreshUIReferences();
        }

        if (showDebugLogs)
            Debug.Log("[GameOverUI] Inizializzato e pronto per gli eventi");
    }
    
    private void OnEnable()
    {
        // Iscriviti all'evento di cambio scena per riscriversi a MatchManager
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Chiamato quando una nuova scena viene caricata - riscriviti a MatchManager e ritrova UI
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"[GameOverUI] Scena caricata: {scene.name}, tentativo di riscrizione a MatchManager");

        // Aspetta un frame per permettere a MatchManager di inizializzarsi
        StartCoroutine(ResubscribeAfterDelay());

        // Ritrova i riferimenti UI nella nuova scena
        RefreshUIReferences();
    }
    
    private System.Collections.IEnumerator ResubscribeAfterDelay()
    {
        // Aspetta la fine del frame per permettere a tutti gli Awake/Start di eseguire
        yield return null;
        
        SubscribeToEvents();
    }
    
    // Inizializza gli elementi UI
    private void InitializeUI()
    {
        // Nascondi pannello game over all'avvio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameOverUI] GameOverPanel non assegnato!");
        }
        
        // Configura pulsanti
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            
            // Imposta testo pulsante
            TextMeshProUGUI buttonText = playAgainButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = playAgainText;
        }
        else
        {
            Debug.LogError("[GameOverUI] PlayAgainButton non assegnato!");
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
            
            // Imposta testo pulsante
            TextMeshProUGUI buttonText = menuButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = menuText;
        }
        else
        {
            Debug.LogError("[GameOverUI] MenuButton non assegnato!");
        }
        
        // Imposta titolo
        if (titleText != null)
        {
            titleText.text = "PARTITA TERMINATA";
        }
        
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Elementi UI inizializzati");
    }
    
    // Si iscrive agli eventi rilevanti
    private void SubscribeToEvents()
    {
        // Iscriviti all'evento di fine partita
        if (MatchManager.Instance != null)
        {
            // Rimuovi prima per evitare duplicati (importante per riscrizione dopo cambio scena)
            MatchManager.Instance.OnMatchEnd -= OnMatchEnded;
            MatchManager.Instance.OnMatchEnd += OnMatchEnded;
            
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Sottoscritto a MatchManager.OnMatchEnd");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("[GameOverUI] MatchManager.Instance non trovato!");
        }
    }
    
    // Handler per l'evento di fine partita
    private void OnMatchEnded()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Ricevuto evento OnMatchEnd");
        
        ShowGameOver();
    }
    
    // Mostra la schermata di game over con le statistiche finali
    public void ShowGameOver()
    {
        if (isGameOverActive)
        {
            if (showDebugLogs)
                Debug.LogWarning("[GameOverUI] Game over già attivo");
            return;
        }

        isGameOverActive = true;

        // Pausa il gioco
        Time.timeScale = 0f;

        // Mostra il cursore
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Nascondi le UI di gameplay
        HideGameplayUI();

        // Mostra il pannello
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Popola la leaderboard finale
        PopulateFinalLeaderboard();

        if (showDebugLogs)
            Debug.Log("[GameOverUI] Schermata game over mostrata");
    }
    
    // Nasconde la schermata di game over
    public void HideGameOver()
    {
        if (!isGameOverActive)
            return;

        isGameOverActive = false;

        // Riavvia il gioco
        Time.timeScale = 1f;

        // Ripristina le UI di gameplay
        ShowGameplayUI();

        // Nascondi il pannello
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Pulisci le righe della leaderboard
        ClearLeaderboardRows();

        if (showDebugLogs)
            Debug.Log("[GameOverUI] Schermata game over nascosta");
    }
    
    // Popola la leaderboard finale con le statistiche
    private void PopulateFinalLeaderboard()
    {
        if (MatchManager.Instance == null)
        {
            Debug.LogError("[GameOverUI] MatchManager.Instance non disponibile!");
            return;
        }
        
        // Pulisci righe esistenti
        ClearLeaderboardRows();
        
        // Ottieni la leaderboard ordinata
        var leaderboard = GetSortedLeaderboard();
        
        // Crea righe per ogni giocatore
        int rank = 1;
        foreach (var stats in leaderboard)
        {
            CreateLeaderboardRow(rank, stats);
            rank++;
        }
        
        if (showDebugLogs)
            Debug.Log($"[GameOverUI] Leaderboard popolata con {leaderboard.Count} giocatori");
    }
    
    // Ottiene la leaderboard ordinata secondo il metodo selezionato
    private List<PlayerStats> GetSortedLeaderboard()
    {
        var leaderboard = MatchManager.Instance.GetLeaderboard();
        
        switch (leaderboardSort)
        {
            case SortMethod.ByScore:
                return leaderboard.OrderByDescending(s => s.GetScore()).ToList();
            case SortMethod.ByKills:
                return leaderboard.OrderByDescending(s => s.kills).ToList();
            case SortMethod.ByKDRatio:
                return leaderboard.OrderByDescending(s => s.GetKDRatio()).ToList();
            default:
                return leaderboard;
        }
    }
    
    // Crea una riga della leaderboard
    private void CreateLeaderboardRow(int rank, PlayerStats stats)
    {
        if (leaderboardContainer == null || leaderboardRowPrefab == null)
        {
            Debug.LogError("[GameOverUI] Container o prefab leaderboard non assegnati!");
            return;
        }
        
        // Crea la riga
        GameObject rowObj = Instantiate(leaderboardRowPrefab, leaderboardContainer);
        
        // Ottieni il componente LeaderboardRow se disponibile
        LeaderboardRow row = rowObj.GetComponent<LeaderboardRow>();
        if (row != null)
        {
            row.SetData(rank, stats);
        }
        else
        {
            // Fallback: crea manualmente il testo
            CreateFallbackLeaderboardRow(rowObj, rank, stats);
        }
        
        spawnedRows.Add(rowObj);
    }
    
    // Crea una riga leaderboard fallback se LeaderboardRow non è disponibile
    private void CreateFallbackLeaderboardRow(GameObject rowObj, int rank, PlayerStats stats)
    {
        TextMeshProUGUI textComponent = rowObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            string playerTag = stats.isPlayer ? " [TU]" : "";
            textComponent.text = $"#{rank} {stats.entityName}{playerTag} | K/D: {stats.kills}/{stats.deaths} | Score: {stats.GetScore()}";
        }
    }
    
    // Pulisce tutte le righe della leaderboard
    private void ClearLeaderboardRows()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null)
                Destroy(row);
        }
        spawnedRows.Clear();
    }

    // Nasconde le UI di gameplay durante il game over
    private void HideGameplayUI()
    {
        // Salva stato originale e disattiva barra vita
        if (healthBarUI != null)
        {
            healthBarOriginalState = healthBarUI.activeSelf;
            healthBarUI.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Barra vita nascosta");
        }

        // Salva stato originale e disattiva munizioni
        if (ammoDisplayUI != null)
        {
            ammoDisplayOriginalState = ammoDisplayUI.activeSelf;
            ammoDisplayUI.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Munizioni nascoste");
        }

        // Salva stato originale e disattiva nome player
        if (playerNameUI != null)
        {
            playerNameOriginalState = playerNameUI.activeSelf;
            playerNameUI.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Nome player nascosto");
        }

        // Salva stato originale e disabilita script leaderboard (tasto TAB)
        if (leaderboardUIScript != null)
        {
            leaderboardScriptOriginalState = leaderboardUIScript.enabled;
            leaderboardUIScript.enabled = false;
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Leaderboard script disabilitato (TAB bloccato)");
        }
    }

    // Ripristina le UI di gameplay dopo il game over
    private void ShowGameplayUI()
    {
        // Ripristina barra vita
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(healthBarOriginalState);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Barra vita ripristinata");
        }

        // Ripristina munizioni
        if (ammoDisplayUI != null)
        {
            ammoDisplayUI.SetActive(ammoDisplayOriginalState);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Munizioni ripristinate");
        }

        // Ripristina nome player
        if (playerNameUI != null)
        {
            playerNameUI.SetActive(playerNameOriginalState);
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Nome player ripristinato");
        }

        // Ripristina script leaderboard (tasto TAB)
        if (leaderboardUIScript != null)
        {
            leaderboardUIScript.enabled = leaderboardScriptOriginalState;
            if (showDebugLogs)
                Debug.Log("[GameOverUI] Leaderboard script riabilitato (TAB sbloccato)");
        }
    }

    // Ritrova i riferimenti UI nella nuova scena dopo un cambio scena
    private void RefreshUIReferences()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Tentativo di ritrovare i riferimenti UI nella nuova scena...");

        // Cerca HealthBarUI
        HealthBarUI healthBar = FindObjectOfType<HealthBarUI>();
        if (healthBar != null)
        {
            healthBarUI = healthBar.gameObject;
            if (showDebugLogs)
                Debug.Log($"[GameOverUI] HealthBarUI trovato: {healthBarUI.name}");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[GameOverUI] HealthBarUI non trovato nella scena!");
        }

        // Cerca AmmoDisplay
        AmmoDisplay ammoDisplay = FindObjectOfType<AmmoDisplay>();
        if (ammoDisplay != null)
        {
            ammoDisplayUI = ammoDisplay.gameObject;
            if (showDebugLogs)
                Debug.Log($"[GameOverUI] AmmoDisplay trovato: {ammoDisplayUI.name}");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[GameOverUI] AmmoDisplay non trovato nella scena!");
        }

        // Cerca il GameObject del nome player (cerca per tag o nome specifico)
        // Provo a trovarlo come child di HealthBarUI se esiste
        if (healthBarUI != null)
        {
            TextMeshProUGUI playerNameText = healthBarUI.GetComponentInChildren<TextMeshProUGUI>();
            if (playerNameText != null && playerNameText.gameObject != healthBarUI)
            {
                playerNameUI = playerNameText.gameObject;
                if (showDebugLogs)
                    Debug.Log($"[GameOverUI] PlayerName UI trovato: {playerNameUI.name}");
            }
        }

        // Cerca LeaderboardUI script
        LeaderboardUI leaderboard = FindObjectOfType<LeaderboardUI>();
        if (leaderboard != null)
        {
            leaderboardUIScript = leaderboard;
            if (showDebugLogs)
                Debug.Log($"[GameOverUI] LeaderboardUI script trovato: {leaderboard.gameObject.name}");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[GameOverUI] LeaderboardUI script non trovato nella scena!");
        }

        if (showDebugLogs)
            Debug.Log("[GameOverUI] Refresh UI completato");
    }

    // Handler per il click sul pulsante "Gioca Ancora"
    private void OnPlayAgainClicked()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Click su 'Gioca Ancora'");
        
        //nascondere il game over prima di tutto per ripristinare Time.timeScale
        HideGameOver();
        
        // Blocca il cursore per il gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ottieni la prossima mappa
        if (MapRotationManager.Instance != null)
        {
            string nextMap = MapRotationManager.Instance.GetNextMap();
            
            // Carica la nuova mappa
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadNewMap(nextMap);
            }
            else
            {
                Debug.LogError("[GameOverUI] GameManager.Instance non trovato!");
                // Fallback: ricarica la scena attuale
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            Debug.LogWarning("[GameOverUI] MapRotationManager.Instance non trovato! Ricarico la scena corrente.");
            // Fallback: ricarica la scena attuale se MapRotationManager non esiste
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadNewMap(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }
    
    // Handler per il click sul pulsante "Menu"
    private void OnMenuClicked()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Click su 'Menu'");
        
        // Nascondi game over
        HideGameOver();
        
        // Mostra il cursore per il menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Ritorna al menu principale
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogWarning("[GameOverUI] GameManager.Instance non trovato! Carico MainMenu direttamente.");
            // Fallback: carica direttamente la scena MainMenu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe dagli eventi
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnMatchEnd -= OnMatchEnded;
        }
        
        // Cleanup singleton
        if (Instance == this)
        {
            Instance = null;
        }
        
        // Cleanup pulsanti
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
        }
    }
    
    // Metodi per debug
    [ContextMenu("Test Show Game Over")]
    public void DebugShowGameOver()
    {
        ShowGameOver();
    }
    
    [ContextMenu("Test Hide Game Over")]
    public void DebugHideGameOver()
    {
        HideGameOver();
    }
    
    [ContextMenu("Test Play Again")]
    public void DebugPlayAgain()
    {
        OnPlayAgainClicked();
    }
}