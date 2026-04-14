using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Engine State")]
    public bool isEngineOn = true;
    public float stallThreshold = 0.2f;

    [Header("Gears & RPM")]
    public int currentGear = 1;
    public int maxGears = 6;
    public float[] gearMultipliers = { 0.0f, 0.4f, 0.6f, 0.8f, 1.0f, 1.15f, 1.3f };
    public float currentRPM;
    public float maxRPM = 8000f;
    public float idleRPM = 1200f;

    [Header("Engine & Speed")]
    public float engineForce = 35000f;
    public float maxSpeed = 75f;
    public float brakeForce = 50000f;

    [Header("Steering")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 15f;
    public float turnSharpness = 12.0f;
    public float steerSpeedDamping = 0.05f;
    public float steerThreshold = 0.5f;
    [Range(0f, 1f)] public float stationarySteerAbility = 0.1f;

    [Header("High-Response Suspension")]
    public float rideHeight = 0.15f;
    public float springDamper = 6000f;
    public float springStrength = 250000f;
    public float antiRollForce = 40000f;
    public float wheelRadius = 0.3f;
    public LayerMask groundLayer = ~0;

    [Header("Handling")]
    public float tireGrip = 2.5f;
    public float velocityAlignStrength = 15f;
    public float centerOfMassY = -0.6f;

    [Header("Wheel Names")]
    public string frontLeftName = "WheelFL_LOD0";
    public string frontRightName = "WheelFR_LOD0";
    public string backLeftName = "WheelBL_LOD0";
    public string backRightName = "WheelBR_LOD0";

    public Transform wheelFL, wheelFR, wheelBL, wheelBR;

    private Collider[] selfColliders;
    private Rigidbody rb;
    private Transform[] wheels = new Transform[4];
    private float[] wheelAngles = new float[4];
    private float smoothSteer;
    private bool[] grounded = new bool[4];
    private float[] wheelCompression = new float[4];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0f, centerOfMassY, 0.1f);
        selfColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        wheels[0] = wheelFL != null ? wheelFL : FindChildRecursive(transform, frontLeftName);
        wheels[1] = wheelFR != null ? wheelFR : FindChildRecursive(transform, frontRightName);
        wheels[2] = wheelBL != null ? wheelBL : FindChildRecursive(transform, backLeftName);
        wheels[3] = wheelBR != null ? wheelBR : FindChildRecursive(transform, backRightName);
    }

    void Update()
    {
        HandleGears();
        if (!isEngineOn && Input.GetKeyDown(KeyCode.E)) isEngineOn = true;
    }

    private void HandleGears()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentGear = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) currentGear = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) currentGear = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) currentGear = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) currentGear = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) currentGear = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha0)) currentGear = 0;
        }
    }

    void FixedUpdate()
    {
        rb.centerOfMass = new Vector3(0f, centerOfMassY, 0.1f);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float speed = rb.linearVelocity.magnitude;
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        bool clutchPressed = Input.GetKey(KeyCode.LeftShift);

        if (isEngineOn && speed < stallThreshold && !clutchPressed && currentGear != 0 && Mathf.Abs(v) < 0.1f)
            isEngineOn = false;

        float gearMaxSpeed = maxSpeed * (currentGear / (float)maxGears);
        if (clutchPressed || currentGear == 0)
            currentRPM = Mathf.Lerp(currentRPM, Mathf.Abs(v) > 0.1f ? maxRPM : idleRPM, Time.fixedDeltaTime * 5f);
        else
            currentRPM = Mathf.Lerp(idleRPM, maxRPM, speed / Mathf.Max(gearMaxSpeed, 0.01f));
        if (!isEngineOn) currentRPM = 0;

        float steerAbility = Mathf.Lerp(stationarySteerAbility, 1f, speed / 5f);
        float speedFactor = 1f / (1f + speed * steerSpeedDamping);
        smoothSteer = Mathf.Lerp(smoothSteer, h * maxSteerAngle * speedFactor * steerAbility, Time.fixedDeltaTime * steerSpeed);

        int groundedCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i] == null) continue;
            Vector3 rayOrigin = wheels[i].position + transform.up * 0.5f;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, -transform.up, rideHeight + 0.5f + wheelRadius, groundLayer);

            grounded[i] = false;
            wheelCompression[i] = 0f;

            foreach (var hit in hits)
            {
                bool isSelf = false;
                foreach (var c in selfColliders) if (hit.collider == c) isSelf = true;
                if (isSelf) continue;

                grounded[i] = true;
                groundedCount++;

                wheelCompression[i] = 1f - ((hit.distance - 0.5f) / (rideHeight + wheelRadius));
                float upVel = Vector3.Dot(transform.up, rb.GetPointVelocity(wheels[i].position));

                float sForce = (wheelCompression[i] * springStrength) - (upVel * springDamper);
                rb.AddForceAtPosition(transform.up * Mathf.Max(0f, sForce), wheels[i].position);

                if (isEngineOn && !clutchPressed && i >= 2 && Mathf.Abs(v) > 0.01f && currentGear > 0 && speed < gearMaxSpeed)
                    rb.AddForceAtPosition(transform.forward * v * engineForce * gearMultipliers[currentGear], wheels[i].position);

                if (Input.GetKey(KeyCode.Space) || (Mathf.Abs(v) < 0.01f && speed > 0.1f))
                    rb.AddForceAtPosition(-rb.linearVelocity.normalized * Mathf.Min(brakeForce, speed * rb.mass) * 0.25f, wheels[i].position);

                float lateralVel = Vector3.Dot(transform.right, rb.GetPointVelocity(wheels[i].position));
                rb.AddForceAtPosition(-transform.right * lateralVel * rb.mass * tireGrip * 0.25f, wheels[i].position, ForceMode.Impulse);
                break;
            }
        }

        ApplyAntiRoll(0, 1);
        ApplyAntiRoll(2, 3);

        if (Mathf.Abs(h) < 0.1f && groundedCount > 0)
        {
            float rollAngle = transform.localEulerAngles.z;
            if (rollAngle > 180) rollAngle -= 360;
            rb.AddRelativeTorque(Vector3.forward * -rollAngle * 5000f);
        }

        if (groundedCount > 0)
        {
            float movingDir = Mathf.Sign(localVel.z);
            if (speed < 0.1f) movingDir = 0;
            float targetAngularY = (smoothSteer / maxSteerAngle) * turnSharpness * Mathf.Clamp01(speed / steerThreshold) * movingDir;
            rb.angularVelocity = new Vector3(rb.angularVelocity.x * 0.8f, targetAngularY, rb.angularVelocity.z * 0.8f);

            if (speed > 1f)
            {
                Vector3 velocityTarget = transform.forward * localVel.z;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(velocityTarget.x, rb.linearVelocity.y, velocityTarget.z), velocityAlignStrength * Time.fixedDeltaTime);
            }
        }

        UpdateWheelVisuals(speed, localVel.z);
    }

    private void ApplyAntiRoll(int leftIndex, int rightIndex)
    {
        if (grounded[leftIndex] && grounded[rightIndex])
        {
            float travelL = wheelCompression[leftIndex];
            float travelR = wheelCompression[rightIndex];
            float antiRollForceAmount = (travelL - travelR) * antiRollForce;

            rb.AddForceAtPosition(transform.up * -antiRollForceAmount, wheels[leftIndex].position);
            rb.AddForceAtPosition(transform.up * antiRollForceAmount, wheels[rightIndex].position);
        }
    }

    private void UpdateWheelVisuals(float speed, float forwardVel)
    {
        float dir = forwardVel >= 0f ? 1f : -1f;
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i] == null) continue;
            wheelAngles[i] += speed * 18f * dir * Time.fixedDeltaTime;
            wheels[i].localRotation = Quaternion.Euler(wheelAngles[i], (i < 2) ? smoothSteer : 0f, 0f);
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) { Transform res = FindChildRecursive(child, name); if (res != null) return res; }
        return null;
    }
}