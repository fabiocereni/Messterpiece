using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLogic : MonoBehaviour
{
    [Header("Pannelli UI")]
    public GameObject pausePanel;     
    public GameObject commandsPanel;
    
    [Header("Interfaccia Giocatore")]
    public GameObject playerHUD; // NUOVO: Trascina qui il Canvas con Vita, Mirino, ecc.

    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead()) return;
            if (MatchManager.Instance != null && !MatchManager.Instance.IsMatchActive) return;

            if (isPaused)
            {
                if (commandsPanel.activeSelf)
                {
                    CloseCommands();
                }
                else
                {
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
        commandsPanel.SetActive(false);
        
        // NUOVO: Nascondi l'interfaccia di gioco (vita, mirino...)
        if (playerHUD != null) playerHUD.SetActive(false);

        Time.timeScale = 0f; 
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        commandsPanel.SetActive(false);
        
        // NUOVO: Riaccendi l'interfaccia di gioco
        if (playerHUD != null) playerHUD.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenCommands()
    {
        pausePanel.SetActive(false);   
        commandsPanel.SetActive(true); 
        // L'HUD è già spento perché siamo in pausa, quindi non serve fare nulla qui
    }

    public void CloseCommands()
    {
        commandsPanel.SetActive(false); 
        pausePanel.SetActive(true);     
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
}