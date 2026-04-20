using UnityEngine;

public class WheelSkid : MonoBehaviour
{
    private TrailRenderer trail;
    private CarController car;
    public int wheelIndex;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        car = GetComponentInParent<CarController>();
    }

    void Update()
    {
        if (car == null) return;

        if (car.IsWheelGrounded(wheelIndex) && car.IsSkidding(wheelIndex))
        {
            trail.emitting = true;
        }
        else
        {
            trail.emitting = false;
        }
    }
}