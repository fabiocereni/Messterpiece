using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestisce il menu di pausa del tutorial.
/// Permette al giocatore di uscire premendo ESC e tornare al MainMenu.
/// </summary>
public class TutorialPauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused = false;

    void Start()
    {
        // Assicurati che il menu sia nascosto all'inizio
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Sblocca e mostra il cursore durante il gioco
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle pausa con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;

        // Blocca di nuovo il cursore
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;

        // Sblocca il cursore per il menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Importante! Resetta il time scale
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartTutorial()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
