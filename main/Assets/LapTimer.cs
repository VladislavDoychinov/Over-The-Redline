using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Splines;
using Unity.Mathematics;

public class LapTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestLapText;
    public TextMeshProUGUI checkpointText;

    [Header("Reset Settings")]
    public SplineContainer roadSpline;

    private static float currentTime;
    private static float bestTime;
    private static bool timerActive = false;
    private static bool hasHitCheckpoint = false;
    private string saveKey = "BestLapTime";

    void Start()
    {
        bestTime = PlayerPrefs.GetFloat(saveKey, 0f);
        UpdateBestLapUI();

        if (checkpointText != null)
        {
            checkpointText.text = "";
            checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, 0);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayerToNearestSplinePoint();
        }

        if (timerActive)
        {
            currentTime += Time.deltaTime;
            if (timerText != null) timerText.text = FormatTime(currentTime);
        }
    }

    void ResetPlayerToNearestSplinePoint()
    {
        if (roadSpline == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float3 localPlayerPos = roadSpline.transform.InverseTransformPoint(player.transform.position);

            SplineUtility.GetNearestPoint(roadSpline.Spline, localPlayerPos, out float3 nearestLocalPoint, out float t);

            Vector3 worldPos = roadSpline.transform.TransformPoint((Vector3)nearestLocalPoint);

            player.transform.position = worldPos + Vector3.up * 1.5f;

            float3 localForward = roadSpline.EvaluateTangent(t);
            Vector3 worldForward = roadSpline.transform.TransformDirection((Vector3)localForward);
            player.transform.rotation = Quaternion.LookRotation(worldForward);

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (gameObject.CompareTag("Checkpoint"))
        {
            hasHitCheckpoint = true;
            StopAllCoroutines();
            StartCoroutine(FadeCheckpointText("CHECKPOINT REACHED"));
        }

        if (gameObject.CompareTag("Finish"))
        {
            if (!timerActive)
            {
                timerActive = true;
                currentTime = 0f;
            }
            else if (hasHitCheckpoint)
            {
                CheckBestLap();
                currentTime = 0f;
                hasHitCheckpoint = false;
            }
        }
    }

    void CheckBestLap()
    {
        if (bestTime <= 0 || currentTime < bestTime)
        {
            bestTime = currentTime;
            UpdateBestLapUI();
            PlayerPrefs.SetFloat(saveKey, bestTime);
            PlayerPrefs.Save();
        }
    }

    void UpdateBestLapUI()
    {
        if (bestLapText != null)
            bestLapText.text = (bestTime > 0) ? "Best: " + FormatTime(bestTime) : "Best: --:--.--";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int fraction = Mathf.FloorToInt((time * 100) % 100);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
    }

    IEnumerator FadeCheckpointText(string message)
    {
        if (checkpointText == null) yield break;
        checkpointText.text = message;
        checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, 1);
        yield return new WaitForSeconds(1.5f);
        float duration = 1.0f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            checkpointText.color = new Color(checkpointText.color.r, checkpointText.color.g, checkpointText.color.b, alpha);
            yield return null;
        }
    }
}