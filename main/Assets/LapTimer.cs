using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LapTimer : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestLapText;

    private float currentTime;
    private float bestTime;
    private bool timerActive = false;
    private bool hasHitCheckpoint = false;


    private string saveKey = "BestLapTime";

    void Start()
    {
        bestTime = PlayerPrefs.GetFloat(saveKey, 0f);

        if (bestTime > 0)
        {
            bestLapText.text = "Best: " + FormatTime(bestTime);
        }
        else
        {
            bestLapText.text = "Best: --:--.--";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (timerActive)
        {
            currentTime += Time.deltaTime;
            timerText.text = FormatTime(currentTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (this.CompareTag("Checkpoint"))
        {
            hasHitCheckpoint = true;
            Debug.Log("Checkpoint Validated!");
        }

        if (this.CompareTag("Finish"))
        {
            if (timerActive && hasHitCheckpoint)
            {
                CheckBestLap();
                currentTime = 0f;
                hasHitCheckpoint = false;
            }
            else if (!timerActive)
            {
                timerActive = true;
                currentTime = 0f;
            }
            else
            {
                Debug.Log("Cheat Attempted: You didn't hit the checkpoint!");
            }
        }
    }

    void CheckBestLap()
    {
        if (bestTime == 0 || currentTime < bestTime)
        {
            bestTime = currentTime;
            bestLapText.text = "Best: " + FormatTime(bestTime);

            PlayerPrefs.SetFloat(saveKey, bestTime);
            PlayerPrefs.Save();
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int fraction = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
    }

    [ContextMenu("Reset Best Time")]
    public void ResetBestTime()
    {
        PlayerPrefs.DeleteKey(saveKey);
        bestTime = 0f;
        bestLapText.text = "Best: --:--.--";
    }
}