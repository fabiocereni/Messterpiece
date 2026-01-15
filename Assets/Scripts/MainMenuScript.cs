using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{

    public GameObject mainMenuPanel;
    public GameObject selectionPanel;
    
    [Header("Opzioni")]
    [Tooltip("Riferimento al controller del menu opzioni")]
    public OptionsMenuController optionsMenu;

    public void OpenSelectionMenu()
    {
        mainMenuPanel.SetActive(false);
        selectionPanel.SetActive(true);
    }
    
    /// <summary>
    /// Apre il menu delle opzioni
    /// </summary>
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
        int randomLevelIndex = Random.Range(1, 5); // get a random level index between 1 and 4
        Debug.Log("Loading Level numero: " + randomLevelIndex);
        SceneManager.LoadScene(randomLevelIndex);
    }

    public void BackToMainMenu()
    {
        selectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
