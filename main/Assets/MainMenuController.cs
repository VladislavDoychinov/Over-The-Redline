using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private GameObject settings;
    private GameObject selectMap;

    void Awake()
    {
        settings = GameObject.FindWithTag("Settings");
        selectMap = GameObject.FindWithTag("SelectMap"); 

        selectMap.SetActive(false);

        if (settings != null)
        {
            settings.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MainMenuController: No GameObject found with tag 'Settings'!");
        }
    }

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

    public void s1()
    {
        SceneManager.LoadScene("s1");
    }

    public void SelectMap()
    {
        if (selectMap != null)
        {
            selectMap.SetActive(true);
        }
    }

    public void CloseSelectMap()
    {
        if (selectMap != null) 
            selectMap.SetActive(false);
    }

    public void ShowSettings()
    {
        if (settings != null) settings.SetActive(true);
    }

    public void HideSettings()
    {
        if (settings != null) settings.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}