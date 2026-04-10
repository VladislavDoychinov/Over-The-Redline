using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void GarageScene()
    {
        SceneManager.LoadScene("GarageScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void TrackGame()
    {
        SceneManager.LoadScene("TrackScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}