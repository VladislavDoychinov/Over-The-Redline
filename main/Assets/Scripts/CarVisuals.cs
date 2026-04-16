using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;
    public Transform steeringWheel;

    public float wheelSpinSpeed = 500f;
    public float maxWheelSteerAngle = 30f;
    public float steeringWheelMaxAngle = 70f;

    public Vector3 steeringWheelAxis = new Vector3(0f, 0f, 1f);

    private Quaternion flBaseRotation;
    private Quaternion frBaseRotation;
    private Quaternion blBaseRotation;
    private Quaternion brBaseRotation;
    private Quaternion steeringWheelBaseRotation;

    private float wheelSpinAngle = 0f;

    void Start()
    {
        if (frontLeftWheel != null)
        {
            flBaseRotation = frontLeftWheel.localRotation;
        }

        if (frontRightWheel != null)
        {
            frBaseRotation = frontRightWheel.localRotation;
        }

        if (backLeftWheel != null)
        {
            blBaseRotation = backLeftWheel.localRotation;
        }

        if (backRightWheel != null)
        {
            brBaseRotation = backRightWheel.localRotation;
        }

        if (steeringWheel != null)
        {
            steeringWheelBaseRotation = steeringWheel.localRotation;
        }
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        wheelSpinAngle += moveInput * wheelSpinSpeed * Time.deltaTime;
        float steerAngle = turnInput * maxWheelSteerAngle;

        Quaternion wheelSpin = Quaternion.Euler(wheelSpinAngle, 0f, 0f);
        Quaternion wheelSteer = Quaternion.Euler(0f, steerAngle, 0f);

        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = flBaseRotation * wheelSteer * wheelSpin;
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = frBaseRotation * wheelSteer * wheelSpin;
        }

        if (backLeftWheel != null)
        {
            backLeftWheel.localRotation = blBaseRotation * wheelSpin;
        }

        if (backRightWheel != null)
        {
            backRightWheel.localRotation = brBaseRotation * wheelSpin;
        }

        if (steeringWheel != null)
        {
            float steeringAngle = -turnInput * steeringWheelMaxAngle;
            Quaternion steeringRotation = Quaternion.AngleAxis(steeringAngle, steeringWheelAxis.normalized);
            steeringWheel.localRotation = steeringWheelBaseRotation * steeringRotation;
        }
    }
}