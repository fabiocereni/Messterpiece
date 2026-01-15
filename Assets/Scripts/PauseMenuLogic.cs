using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLogic : MonoBehaviour
{

    public GameObject pausePanel;
    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if player is dead before allowing pause
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead())
            {
                return;
            }

            // Check if match is over before allowing pause
            if (MatchManager.Instance != null && !MatchManager.Instance.IsMatchActive)
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
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
        Time.timeScale = 0f; // congelo il tempo (nemici fermi, animazioni ferme)
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // mostro il cursore
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false); 
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
