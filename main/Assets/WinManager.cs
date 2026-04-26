using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinManager : MonoBehaviour
{
    public GameObject winUI;
    public GameObject loseUI;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void ShowWinScreen() => StartCoroutine(DisplayUI(winUI));
    public void ShowLoseScreen() => StartCoroutine(DisplayUI(loseUI));

    private IEnumerator DisplayUI(GameObject ui)
    {
        ui.SetActive(true);
        
        DisableCarAndCamera();

        yield return null; 

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DisableCarAndCamera()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<CarController>();
            if (controller != null) controller.enabled = false;
        }
        CameraViewSwitcher cam = Object.FindFirstObjectByType<CameraViewSwitcher>();
        if (cam != null) cam.enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}