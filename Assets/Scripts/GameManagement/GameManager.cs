using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState currentState = GameState.Menu;
    private int killCount = 0;

    private void Awake()
    {
        // Singleton: assicura che esista un solo GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager Awake() invoked");
    }

    private void Start()
    {
        Debug.Log("GameManager Start() invoked");
        // All'avvio mostra il menu
        // UIManager.Instance.ShowMainMenu();

        // PER ADESSO VADO AD ISTANZIARE SUBITO LA SCENA CHE VOGLIO IO PER PROVARE
        // QUANDO AVREMO IL CANVA CON IL PULSANTE PER STARTARE FARA' TUTTO QUELLO
        StartGame();
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        // QUA CARICO LA MAPPA, che è presente in una scena
        // magari abbiamo una lista di mappe e randomicamente ne viene scelta una
        SceneManager.LoadSceneAsync("PlayerMovementTestScene");
    }

    public void OpenOptions()
    {
        UIManager.Instance.ShowOptionsMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Gioco chiuso.");
    }

    public void AddKill()
    {
        killCount++;
        Debug.Log($"Uccisioni totali: {killCount}");
    }
    
    /// <summary>
    /// Carica una nuova mappa (usato da "Gioca Ancora")
    /// </summary>
    public void LoadNewMap(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("[GameManager] Nome mappa non valido!");
            return;
        }
        
        Debug.Log($"[GameManager] Caricamento nuova mappa: {mapName}");
        
        // Aggiorna stato
        currentState = GameState.Playing;
        
        // Resetta contatore uccisioni
        killCount = 0;
        
        // Carica la nuova scena
        SceneManager.LoadSceneAsync(mapName);
    }
    
    /// <summary>
    /// Ritorna al menu principale (usato dal pulsante "Menu")
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[GameManager] Ritorno al menu principale");
        
        // Aggiorna stato
        currentState = GameState.Menu;
        
        // Resetta contatore uccisioni
        killCount = 0;
        
        // Carica la scena del menu principale
        // Assume che la scena del menu si chiami "MainMenu" o simile
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
