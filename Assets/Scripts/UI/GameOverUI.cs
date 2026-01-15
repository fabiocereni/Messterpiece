using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controller principale per la schermata di fine partita
/// Si iscrive a MatchManager.OnMatchEnd e gestisce UI game over
/// </summary>
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
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    // Enum per ordinamento
    public enum SortMethod { ByScore, ByKills, ByKDRatio }
    
    // Stato interno
    private List<GameObject> spawnedRows = new List<GameObject>();
    private bool isGameOverActive = false;
    
    private void Awake()
    {
        // Singleton pattern
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
        
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Inizializzato e pronto per gli eventi");
    }
    
    private void OnEnable()
    {
        // Iscriviti all'evento di cambio scena per ri-iscriversi a MatchManager
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Chiamato quando una nuova scena viene caricata - ri-iscriviti a MatchManager
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"[GameOverUI] Scena caricata: {scene.name}, tentativo di ri-iscrizione a MatchManager");
        
        // Aspetta un frame per permettere a MatchManager di inizializzarsi
        StartCoroutine(ResubscribeAfterDelay());
    }
    
    private System.Collections.IEnumerator ResubscribeAfterDelay()
    {
        // Aspetta la fine del frame per permettere a tutti gli Awake/Start di eseguire
        yield return null;
        
        SubscribeToEvents();
    }
    
    /// <summary>
    /// Inizializza gli elementi UI
    /// </summary>
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
    
    /// <summary>
    /// Si iscrive agli eventi rilevanti
    /// </summary>
    private void SubscribeToEvents()
    {
        // Iscriviti all'evento di fine partita
        if (MatchManager.Instance != null)
        {
            // Rimuovi prima per evitare duplicati (importante per ri-iscrizione dopo cambio scena)
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
    
    /// <summary>
    /// Handler per l'evento di fine partita
    /// </summary>
    private void OnMatchEnded()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Ricevuto evento OnMatchEnd");
        
        ShowGameOver();
    }
    
    /// <summary>
    /// Mostra la schermata di game over con le statistiche finali
    /// </summary>
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
    
    /// <summary>
    /// Nasconde la schermata di game over
    /// NOTA: Non gestisce il cursore - i singoli handler lo gestiscono
    /// </summary>
    public void HideGameOver()
    {
        if (!isGameOverActive)
            return;
        
        isGameOverActive = false;
        
        // Riavvia il gioco
        Time.timeScale = 1f;
        
        // NOTA: Il cursore viene gestito dai singoli handler (Menu vs Gioca Ancora)
        // perché hanno bisogno di comportamenti diversi
        
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
    
    /// <summary>
    /// Popola la leaderboard finale con le statistiche
    /// </summary>
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
    
    /// <summary>
    /// Ottiene la leaderboard ordinata secondo il metodo selezionato
    /// </summary>
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
    
    /// <summary>
    /// Crea una riga della leaderboard
    /// </summary>
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
    
    /// <summary>
    /// Crea una riga leaderboard fallback se LeaderboardRow non è disponibile
    /// </summary>
    private void CreateFallbackLeaderboardRow(GameObject rowObj, int rank, PlayerStats stats)
    {
        TextMeshProUGUI textComponent = rowObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            string playerTag = stats.isPlayer ? " [TU]" : "";
            textComponent.text = $"#{rank} {stats.entityName}{playerTag} | K/D: {stats.kills}/{stats.deaths} | Score: {stats.GetScore()}";
        }
    }
    
    /// <summary>
    /// Pulisce tutte le righe della leaderboard
    /// </summary>
    private void ClearLeaderboardRows()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null)
                Destroy(row);
        }
        spawnedRows.Clear();
    }
    
    /// <summary>
    /// Handler per il click sul pulsante "Gioca Ancora"
    /// </summary>
    private void OnPlayAgainClicked()
    {
        if (showDebugLogs)
            Debug.Log("[GameOverUI] Click su 'Gioca Ancora'");
        
        // SEMPRE nascondere il game over prima di tutto per ripristinare Time.timeScale
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
    
    /// <summary>
    /// Handler per il click sul pulsante "Menu"
    /// </summary>
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