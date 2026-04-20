using UnityEngine;

public class RPMGauge : MonoBehaviour
{
    [Header("Connections")]
    public CarController car;

    [Header("Angles")]
    public float minRPMAngle = 120f;
    public float maxRPMAngle = -120f;

    [Header("Smoothing")]
    [Tooltip("Higher values make the needle faster. Lower values make it slower and heavier.")]
    public float needleSmoothness = 2f;

    void Update()
    {
        if (car == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) car = player.GetComponent<CarController>();
            return;
        }

        float rpmPercent = Mathf.InverseLerp(0, car.maxRPM, car.currentRPM);
        float targetZRotation = Mathf.Lerp(minRPMAngle, maxRPMAngle, rpmPercent);

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * needleSmoothness);
    }
}