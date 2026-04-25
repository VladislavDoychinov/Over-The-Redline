using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LapManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI checkpointText;
    private bool hasHitCheckpoint = false;
    private WinManager uiManager;

    void Start()
    {
        uiManager = Object.FindAnyObjectByType<WinManager>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        LapManager tmanager = player.GetComponent<LapManager>();

        if (SceneManager.GetActiveScene().name == "HellMode")
        {
            tmanager.enabled = true;
        }
        else
        {
            tmanager.enabled = false;
        }


        GameObject textObj = GameObject.Find("CheckpointText");

        if (textObj != null)
        {
            checkpointText = textObj.GetComponent<TextMeshProUGUI>();

            checkpointText.text = "";
            checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, 0);
        }
        else
        {
            Debug.LogError("LapManager: Couldn't find CheckpointText in the scene! Make sure the UI object is named correctly.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if (!hasHitCheckpoint)
            {
                hasHitCheckpoint = true;
                StartCoroutine(FadeText("CHECKPOINT REACHED", Color.red));
            }
        }

        if (other.CompareTag("Finish"))
        {
            if (hasHitCheckpoint)
            {
                WinGame();
            }
            else
            {
                StartCoroutine(FadeText("CHECKPOINT MISSING!", Color.yellow));
            }
        }
    }

    void WinGame()
    {
        Debug.Log("YOU ESCAPED HELL!");
        if (uiManager != null)
        {
            uiManager.ShowWinScreen();
        }
        else
        {
            Time.timeScale = 0;
            //Debug.LogError("WinManager not found! Game paused but no UI shown.");
        }
    }

    IEnumerator FadeText(string message, Color textColor)
    {
        if (checkpointText == null) yield break;

        checkpointText.text = message;
        checkpointText.color = new Color(textColor.r, textColor.g, textColor.b, 1);

        yield return new WaitForSeconds(2);

        float duration = 1.0f;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, currentTime / duration);
            checkpointText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }
        checkpointText.text = "";
    }
}