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
    }

    private void Start()
    {
        // All'avvio mostra il menu
        UIManager.Instance.ShowMainMenu();
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        // QUA CARICO LA MAPPA, che è presente in una scena
        // magari abbiamo una lista di mappe e randomicamente ne viene scelta una
        // SceneManager.LoadScene("Scena1");
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
}
