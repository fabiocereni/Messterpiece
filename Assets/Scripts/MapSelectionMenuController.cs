using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectionMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mapSelectionPanel;
    public GameObject mainMenuPanel;

    public void LoadMapByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMapByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadRandomMap()
    {
        int randomLevelIndex = Random.Range(1, 4); // get a random level index between 1 and 3
        Debug.Log("Loading Level numero: " + randomLevelIndex);
        SceneManager.LoadScene(randomLevelIndex);
    }

    public void BackToMainMenu()
    {
        mapSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}