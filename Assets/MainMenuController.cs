using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Loads MainScene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); 
    }

    // Quits Game
    public void QuitGame()
    {
        Debug.Log("The game closed"); 
        Application.Quit(); 
    }
}