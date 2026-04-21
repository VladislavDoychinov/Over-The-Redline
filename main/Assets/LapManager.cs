using UnityEngine;
using TMPro;
using System.Collections;

public class LapManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI checkpointText;
    private bool hasHitCheckpoint = false;

    void Start()
    {
        //find text object
        GameObject textObj = GameObject.Find("CheckpointText");

        if (textObj != null)
        {
            checkpointText = textObj.GetComponent<TextMeshProUGUI>();

            // hide text
            checkpointText.text = "";
            checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, 0);
        }
        else
        {
            Debug.LogError("LapManager: Couldn't find CheckpointText in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if (!hasHitCheckpoint)
            {
                hasHitCheckpoint = true;
                StartCoroutine(FadeText("CHECKPOINT REACHED"));
            }
        }

        if (other.CompareTag("Finish") && hasHitCheckpoint)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("YOU ESCAPED!");
        Time.timeScale = 0;
    }

    IEnumerator FadeText(string message)
    {
        if (checkpointText == null) yield break;

        checkpointText.text = message;
        checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, 1);

        yield return new WaitForSeconds(2);

        float duration = 1.0f;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, currentTime / duration);
            checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, alpha);
            yield return null;
        }
        checkpointText.text = "";
    }
}