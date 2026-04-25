using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TrackManager : MonoBehaviour
{
    [Header("UI Reference")]
    private bool hasHitCheckpoint = false;
    private WinManager uiManager;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        TrackManager tmanager = player.GetComponent<TrackManager>();

        if (SceneManager.GetActiveScene().name == "TrackScene")
        {
            tmanager.enabled = true;
        }
        else
        {
            tmanager.enabled = false;
        }

        uiManager = Object.FindAnyObjectByType<WinManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if (!hasHitCheckpoint)
            {
                hasHitCheckpoint = true;
            }
        }

        if (other.CompareTag("Finish"))
        {
            if (hasHitCheckpoint)
            {
                WinGame();
            }
        }
    }

    void WinGame()
    {

        if (uiManager != null)
        {
            uiManager.ShowWinScreen();
        }
        else
        {
            Time.timeScale = 0;
            Debug.LogError("WinManager not found! Game paused but no UI shown.");
        }
    }
}