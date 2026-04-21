using UnityEngine;

public class HellFog : MonoBehaviour
{
    [Header("Fog Settings")]
    public Color hellColor = Color.red;
    public float speed = 2.5f;

    [Tooltip("How close the red wall gets")]
    public float minDistance = 20f;
    [Tooltip("How far away the red wall goes")]
    public float maxDistance = 150f;

    void Update()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = hellColor;
        RenderSettings.fogMode = FogMode.Linear;

        float pulse = Mathf.Lerp(minDistance, maxDistance, (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f);
        RenderSettings.fogEndDistance = pulse;

        RenderSettings.fogStartDistance = 5f;
    }
}