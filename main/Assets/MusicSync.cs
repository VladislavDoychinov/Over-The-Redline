using UnityEngine;

public class MusicSync : MonoBehaviour
{
    public AudioSource musicSource;
    public GameObject spawner;

    public float startTime = 30.0f;
    public float endTime = 150.0f;

    private bool isBossActive = false;

    [Header("Ghost Settings")]
    public GameObject ghostObject;
    public float ghostStartTime = 60.0f;
    public float ghostEndTime = 90.0f; 
    private bool ghostActivated = false;
    private bool ghostDeactivated = false;
    public GameObject DirectionalLightFog;
    public float fogStartTime = 41.0f;
    public float fogEndTime = 104.0f;
    private bool fogActivated = false;
    private bool fogDeactivated = false;

    void Update()
    {
        if (musicSource.isPlaying)
        {
            float currentTime = musicSource.time;

            if (currentTime >= startTime && currentTime < endTime && !isBossActive)
            {
                ActivateBoss(true);
            }
            else if (currentTime >= endTime && isBossActive)
            {
                ActivateBoss(false);
            }

            if (currentTime >= ghostStartTime && currentTime < ghostEndTime && !ghostActivated)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    GhostCar gScript = ghostObject.GetComponent<GhostCar>();
                    gScript.mainCar = player.transform;
                    gScript.mainHealth = player.GetComponent<CarHealth>();

                    ghostObject.SetActive(true);
                    ghostActivated = true;
                    Debug.Log("GHOST CLONE ACTIVATED!");
                }
            }

            if (currentTime >= ghostEndTime && ghostActivated && !ghostDeactivated)
            {
                ghostObject.SetActive(false);
                ghostDeactivated = true; // Mark as done so it doesn't loop
                Debug.Log("GHOST CLONE DISAPPEARED!");
            }
            if (currentTime >= fogStartTime && currentTime < fogEndTime && !fogActivated)
            {
                DirectionalLightFog.GetComponent<BlizzardFog>().enabled = true;
                fogActivated = true;
                Debug.Log("Blizzard Fog Pulse Started!");
            }
            if (currentTime >= fogEndTime && fogActivated && !fogDeactivated)
            {
                DirectionalLightFog.GetComponent<BlizzardFog>().enabled = false;
                RenderSettings.fogEndDistance = 300f;
                fogDeactivated = true;
                Debug.Log("Blizzard Fog Pulse Ended.");
            }
        }
    }

    void ActivateBoss(bool state)
    {
        isBossActive = state;
        spawner.SetActive(state);
        RenderSettings.fogDensity = state ? 0.05f : 0.01f;
        Debug.Log(state ? "Music Drop! Boss Start." : "Song Outro! Boss Stop.");
    }
}