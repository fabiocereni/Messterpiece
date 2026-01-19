using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{

    public GameObject mainMenuPanel;
    public GameObject playMenuPanel;
    public GameObject mapSelectionPanel;
    
    [Header("Opzioni")]
    [Tooltip("Riferimento al controller del menu opzioni")]
    public OptionsMenuController optionsMenu;

    public void OpenPlayMenu()
    {
        mainMenuPanel.SetActive(false);
        playMenuPanel.SetActive(true);
    }
    
    public void OpenMapSelection()
    {
        playMenuPanel.SetActive(false);
        mapSelectionPanel.SetActive(true);
    }
    
    // Apre il menu delle opzioni
    public void OpenOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.ShowOptions();
        }
        else
        {
            Debug.LogError("[MainMenuScript] OptionsMenuController non assegnato!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    public void PlayTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void PlayGame()
    {
        int randomLevelIndex = Random.Range(1, 4); // get a random level index between 1 and 3
        Debug.Log("Loading Level numero: " + randomLevelIndex);
        SceneManager.LoadScene(randomLevelIndex);
    }

    public void BackToMainMenu()
    {
        mapSelectionPanel.SetActive(false);
        playMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
