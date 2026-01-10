using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    //Public method to start the game
    public void PlayGame()
    {
        int randomLevelIndex = Random.Range(1, 5); // get a random level index between 1 and 4
        Debug.Log("Loading Level numero: " + randomLevelIndex);
        SceneManager.LoadScene(randomLevelIndex);
    }

    //Public method to start the tutorial
    public void PlayTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
