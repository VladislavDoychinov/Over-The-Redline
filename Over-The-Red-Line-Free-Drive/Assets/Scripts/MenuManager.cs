using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("CarSelect");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }
}