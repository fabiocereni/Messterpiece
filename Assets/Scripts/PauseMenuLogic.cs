using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLogic : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;     // Il menu di pausa principale
    public GameObject commandsPanel;  // IL NUOVO Pannello con l'immagine dei comandi

    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Controlli di sicurezza (se morto o match finito non fa nulla)
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead()) return;

            if (MatchManager.Instance != null && !MatchManager.Instance.IsMatchActive) return;

            // LOGICA ESC:
            if (isPaused)
            {
                // Se il menu comandi è aperto, chiudilo e torna al menu pausa
                if (commandsPanel.activeSelf)
                {
                    CloseCommands();
                }
                else
                {
                    // Altrimenti torna al gioco
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        commandsPanel.SetActive(false); // Assicuriamoci che i comandi siano chiusi all'inizio
        
        Time.timeScale = 0f; 
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        commandsPanel.SetActive(false); // Chiudiamo tutto
        
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- NUOVE FUNZIONI PER I COMANDI ---

    // Collegalo al bottone "COMANDI" nel menu di pausa
    public void OpenCommands()
    {
        pausePanel.SetActive(false);   // Nascondo il menu pausa
        commandsPanel.SetActive(true); // Mostro l'immagine comandi
    }

    // Collegalo a un bottone "INDIETRO" dentro la schermata dei comandi
    public void CloseCommands()
    {
        commandsPanel.SetActive(false); // Nascondo i comandi
        pausePanel.SetActive(true);     // Riapro il menu pausa
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}