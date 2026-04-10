using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Engine & Speed")]
    public float engineForce = 28000f;
    public float maxSpeed = 75f;
    public float brakeForce = 40000f;

    [Header("Steering (Moving Only)")]
    public float maxSteerAngle = 38f;
    public float steerSpeed = 20f;
    public float turnSharpness = 6.0f;
    public float steerSpeedDamping = 0.01f;
    [Tooltip("Car must be moving this fast to rotate.")]
    public float steerThreshold = 1.0f;

    [Header("Suspension (Ultra Low & Planted)")]
    public float rideHeight = 0.1f;
    public float springStrength = 90000f;
    public float springDamper = 10000f;
    public float wheelRadius = 0.3f;
    public LayerMask groundLayer = ~0;

    [Header("Handling")]
    public float tireGrip = 1.8f;
    public float velocityAlignStrength = 12f;
    public float centerOfMassY = -1.1f;

    [Header("Wheel & Interior Names")]
    public string steeringWheelName = "SteeringWhell_LOD0";
    public string frontLeftName = "WheelFL_LOD0";
    public string frontRightName = "WheelFR_LOD0";
    public string backLeftName = "WheelBL_LOD0";
    public string backRightName = "WheelBR_LOD0";

    [Header("Optional Manual Assignments")]
    public Transform wheelFL; public Transform wheelFR;
    public Transform wheelBL; public Transform wheelBR;
    public Transform steeringWheelObj;

    private Collider[] selfColliders;
    private Rigidbody rb;
    private Transform[] wheels = new Transform[4];
    private float[] wheelAngles = new float[4];
    private float smoothSteer;
    private bool[] grounded = new bool[4];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0f, centerOfMassY, 0.2f);
        selfColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        wheels[0] = wheelFL != null ? wheelFL : FindChildRecursive(transform, frontLeftName);
        wheels[1] = wheelFR != null ? wheelFR : FindChildRecursive(transform, frontRightName);
        wheels[2] = wheelBL != null ? wheelBL : FindChildRecursive(transform, backLeftName);
        wheels[3] = wheelBR != null ? wheelBR : FindChildRecursive(transform, backRightName);

        if (steeringWheelObj == null)
            steeringWheelObj = FindChildRecursive(transform, steeringWheelName);
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float speed = rb.linearVelocity.magnitude;
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float speedFactor = 1f / (1f + speed * steerSpeedDamping);
        smoothSteer = Mathf.Lerp(smoothSteer, h * maxSteerAngle * speedFactor, Time.fixedDeltaTime * steerSpeed);

        int groundedCount = 0;

        for (int i = 0; i < 4; i++)
        {
            grounded[i] = false;
            if (wheels[i] == null) continue;

            Vector3 rayOrigin = wheels[i].position + transform.up * 0.5f;
            float totalDist = rideHeight + 0.5f + wheelRadius;

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, -transform.up, totalDist, groundLayer);
            RaycastHit hit = default;
            float closest = Mathf.Infinity;
            bool validHit = false;

            foreach (var h2 in hits)
            {
                bool isSelf = false;
                foreach (var c in selfColliders) if (h2.collider == c) { isSelf = true; break; }
                if (isSelf) continue;
                if (h2.distance < closest) { closest = h2.distance; hit = h2; validHit = true; }
            }

            if (!validHit) continue;
            grounded[i] = true;
            groundedCount++;

            float compression = rideHeight - (hit.distance - 0.5f - wheelRadius);
            float upVel = Vector3.Dot(transform.up, rb.GetPointVelocity(wheels[i].position));
            float sForce = (compression * springStrength) - (upVel * springDamper);
            rb.AddForceAtPosition(transform.up * Mathf.Max(0f, sForce), wheels[i].position);

            if (i >= 2 && Mathf.Abs(v) > 0.01f && speed < maxSpeed)
                rb.AddForceAtPosition(transform.forward * v * engineForce, wheels[i].position);

            if (Input.GetKey(KeyCode.Space) || (Mathf.Abs(v) < 0.01f && speed > 0.1f))
            {
                float bForce = Mathf.Min(brakeForce, speed * rb.mass);
                rb.AddForceAtPosition(-rb.linearVelocity.normalized * bForce * 0.25f, wheels[i].position);
            }

            float lateralVel = Vector3.Dot(transform.right, rb.GetPointVelocity(wheels[i].position));
            rb.AddForceAtPosition(-transform.right * lateralVel * rb.mass * tireGrip * 0.25f, wheels[i].position, ForceMode.Impulse);
        }

        if (groundedCount > 0)
        {
            float speedRatio = Mathf.Clamp01(speed / steerThreshold);
            float movingForward = Mathf.Sign(localVel.z);

            float targetAngularY = (smoothSteer / maxSteerAngle) * turnSharpness * speedRatio * movingForward;

            if (steeringWheelObj != null)
                steeringWheelObj.localRotation = Quaternion.Euler(0f, 0f, -smoothSteer * 5f);

            rb.angularVelocity = new Vector3(rb.angularVelocity.x * 0.5f, targetAngularY, rb.angularVelocity.z * 0.5f);

            if (speed > 0.5f)
            {
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 targetVel = transform.forward * flatVel.magnitude * movingForward;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z), velocityAlignStrength * Time.fixedDeltaTime);
            }
        }

        UpdateWheelVisuals(speed, localVel.z);
    }

    private void UpdateWheelVisuals(float speed, float forwardVel)
    {
        float dir = forwardVel >= 0f ? 1f : -1f;
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i] == null) continue;
            wheelAngles[i] += speed * 18f * dir * Time.fixedDeltaTime;
            float steer = (i < 2) ? smoothSteer : 0f;
            wheels[i].localRotation = Quaternion.Euler(wheelAngles[i], steer, 0f);
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) { Transform res = FindChildRecursive(child, name); if (res != null) return res; }
        return null;
    }
}