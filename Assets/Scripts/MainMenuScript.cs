using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    //Public method to start the game
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
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
