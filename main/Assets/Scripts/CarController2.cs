using UnityEngine;

public class CarController2 : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 10f;
    public float deceleration = 2.5f;
    public float brakeStrength = 14f;
    public float engineBraking = 4f;

    [Header("Steering")]
    public float turnSpeed = 110f;
    public float highSpeedTurnReduction = 0.6f;
    public float reverseTurnMultiplier = 0.7f;

    [Header("Transmission")]
    public int currentGear = 0; // -1 = R, 0 = N, 1..5 = forward
    public float reverseMaxSpeed = 6f;
    public float[] gearMaxSpeeds = { 0f, 10f, 16f, 23f, 30f, 38f };
    public float[] gearAccelerationMultipliers = { 0f, 1.8f, 1.35f, 1.0f, 0.8f, 0.65f };

    private float moveInput;
    private float turnInput;
    private float currentSpeed;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        HandleGearInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleTurning();
    }

    void HandleGearInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentGear = -1;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            currentGear = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentGear = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentGear = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentGear = 3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentGear = 4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentGear = 5;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
        {
            if (currentGear < 5)
            {
                currentGear++;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q))
        {
            if (currentGear > -1)
            {
                currentGear--;
            }
        }
    }

    void HandleMovement()
    {
        float targetSpeed = 0f;

        if (currentGear > 0)
        {
            if (moveInput > 0f)
            {
                targetSpeed = gearMaxSpeeds[currentGear];
            }
        }
        else if (currentGear == -1)
        {
            if (moveInput > 0f)
            {
                targetSpeed = -reverseMaxSpeed;
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            ApplyBraking();
        }
        else
        {
            if (currentGear > 0 && moveInput > 0f)
            {
                AccelerateForward(targetSpeed);
            }
            else if (currentGear == -1 && moveInput > 0f)
            {
                AccelerateReverse(targetSpeed);
            }
            else
            {
                ApplyCoasting();
            }
        }

        Vector3 movement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    void AccelerateForward(float targetSpeed)
    {
        float gearAccel = acceleration * gearAccelerationMultipliers[currentGear];

        if (currentSpeed < targetSpeed)
        {
            currentSpeed += gearAccel * Time.fixedDeltaTime;

            if (currentSpeed > targetSpeed)
            {
                currentSpeed = targetSpeed;
            }
        }
        else if (currentSpeed > targetSpeed)
        {
            currentSpeed -= engineBraking * Time.fixedDeltaTime;

            if (currentSpeed < targetSpeed)
            {
                currentSpeed = targetSpeed;
            }
        }
    }

    void AccelerateReverse(float targetSpeed)
    {
        float reverseAcceleration = acceleration * 0.8f;

        if (currentSpeed > targetSpeed)
        {
            currentSpeed -= reverseAcceleration * Time.fixedDeltaTime;

            if (currentSpeed < targetSpeed)
            {
                currentSpeed = targetSpeed;
            }
        }
        else if (currentSpeed < targetSpeed)
        {
            currentSpeed += engineBraking * Time.fixedDeltaTime;

            if (currentSpeed > targetSpeed)
            {
                currentSpeed = targetSpeed;
            }
        }
    }

    void ApplyBraking()
    {
        if (currentSpeed > 0f)
        {
            currentSpeed -= brakeStrength * Time.fixedDeltaTime;

            if (currentSpeed < 0f)
            {
                currentSpeed = 0f;
            }
        }
        else if (currentSpeed < 0f)
        {
            currentSpeed += brakeStrength * Time.fixedDeltaTime;

            if (currentSpeed > 0f)
            {
                currentSpeed = 0f;
            }
        }
    }

    void ApplyCoasting()
    {
        if (currentSpeed > 0f)
        {
            currentSpeed -= deceleration * Time.fixedDeltaTime;

            if (currentSpeed < 0f)
            {
                currentSpeed = 0f;
            }
        }
        else if (currentSpeed < 0f)
        {
            currentSpeed += deceleration * Time.fixedDeltaTime;

            if (currentSpeed > 0f)
            {
                currentSpeed = 0f;
            }
        }
    }

    void HandleTurning()
    {
        if (currentGear == 0)
        {
            return;
        }

        if (Mathf.Abs(currentSpeed) < 0.1f)
        {
            return;
        }

        if (Mathf.Abs(turnInput) < 0.01f)
        {
            return;
        }

        float speedAbs = Mathf.Abs(currentSpeed);
        float maxReferenceSpeed = gearMaxSpeeds[gearMaxSpeeds.Length - 1];
        float speedFactor = Mathf.Clamp01(speedAbs / maxReferenceSpeed);

        float actualTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * highSpeedTurnReduction, speedFactor);

        if (currentSpeed < 0f)
        {
            actualTurnSpeed *= reverseTurnMultiplier;
        }

        float turn = turnInput * actualTurnSpeed * Time.fixedDeltaTime;

        if (currentSpeed < 0f)
        {
            turn *= -1f;
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }
}