using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [Header("Connections")]
    public CarController car;

    [Header("Angles")]
    public float minAngle = 120f;
    public float maxAngle = -120f;

    [Header("Smoothing")]
    [Tooltip("Higher values make the needle faster. Lower values make it slower and heavier.")]
    public float needleSmoothness = 5f;

    void Update()
    {
        if (car == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) car = player.GetComponent<CarController>();
            return;
        }

        float speedPercent = Mathf.InverseLerp(0, 260, Mathf.Abs(car.currentSpeed));
        float targetZRotation = Mathf.Lerp(minAngle, maxAngle, speedPercent);

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * needleSmoothness);
    }
}