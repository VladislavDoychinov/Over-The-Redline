using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

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
        Time.timeScale = 0f;

        // This forces Unity to process a frame before locking the cursor
        yield return new WaitForSecondsRealtime(0.1f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisableCar();
    }

    private void DisableCar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<CarController>();
            if (controller != null) controller.enabled = false;
        }
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