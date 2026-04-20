using UnityEngine;

public class WheelSkid : MonoBehaviour
{
    private TrailRenderer trail;
    private CarController car;
    public int wheelIndex; // 0=FL, 1=FR, 2=RL, 3=RR

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        car = GetComponentInParent<CarController>();
    }

    void Update()
    {
        if (car == null) return;

        // Checks the functions inside CarController
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