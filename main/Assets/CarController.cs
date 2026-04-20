using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Engine & Power")]
    public bool isEngineOn = true;
    public float engineTorque = 1100f;
    public float idleRPM = 800f;
    public float maxRPM = 7000f;
    public float rpmLimiter = 6500f;
    public float currentRPM;

    [Header("Manual Transmission")]
    public float[] gearRatios = { -3.0f, 3.2f, 1.9f, 1.3f, 1.0f, 0.8f, 0.65f };
    public float finalDriveRatio = 3.6f;
    [Range(-1, 6)] public int currentGear = 0;

    [Header("Brakes")]
    public float brakeTorque = 4000f;
    public float handbrakeTorque = 8000f;

    [Header("Steering & Physics")]
    public float maxSteerAngle = 40f;
    public float steerSpeed = 5f;
    public float speedSteerDamping = 0.05f;
    public float frontGrip = 1.15f;
    public float baseRearGrip = 0.65f;
    [Range(0, 1)] public float sideFrictionDamping = 0.22f;
    public float oversteerFactor = 0.4f;

    [Header("Suspension")]
    public float rideHeight = 0.22f;
    public float springStrength = 50000f;
    public float springDamper = 5000f;
    public float wheelRadius = 0.33f;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.6f, -0.1f);

    [Header("Visuals & Effects")]
    public Transform wheelFL; public Transform wheelFR;
    public Transform wheelRL; public Transform wheelRR;
    [Tooltip("Drag your 4 TrailRenderers here. MUST be children of the CAR, not wheels.")]
    public TrailRenderer[] tireTrails = new TrailRenderer[4];
    public float skidThreshold = 2.5f;

    private Rigidbody rb;
    private float throttleInput, steerInput, brakeInput, smoothSteer;
    private bool handbrakeInput;
    private bool[] grounded = new bool[4];
    private float[] springCompression = new float[4];
    private float[] wheelSpinAngle = new float[4];
    private Vector3[] wheelRestPos = new Vector3[4];
    private Transform[] wheels = new Transform[4];
    private float[] lateralSlip = new float[4];

    private const int FL = 0, FR = 1, RL = 2, RR = 3;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.centerOfMass = centerOfMassOffset;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        wheels[FL] = wheelFL; wheels[FR] = wheelFR;
        wheels[RL] = wheelRL; wheels[RR] = wheelRR;

        for (int i = 0; i < 4; i++)
            if (wheels[i] != null)
                wheelRestPos[i] = transform.InverseTransformPoint(wheels[i].position);
    }

    void Update()
    {
        HandleInputs();
        UpdateEffects();
    }

    void FixedUpdate()
    {
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        ApplySuspension();
        ApplyGearsAndEngine(speed);
        ApplySteering(speed);
        ApplyTireForces(speed);
        UpdateWheelVisuals(speed);
    }

    void HandleInputs()
    {
        throttleInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        brakeInput = Mathf.Clamp01(-Input.GetAxis("Vertical"));
        steerInput = Input.GetAxis("Horizontal");
        handbrakeInput = Input.GetKey(KeyCode.Space);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.B)) currentGear = -1;
            else if (Input.GetKeyDown(KeyCode.Alpha0)) currentGear = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha1)) currentGear = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) currentGear = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) currentGear = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) currentGear = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) currentGear = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) currentGear = 6;
        }
    }

    void ApplySuspension()
    {
        for (int i = 0; i < 4; i++)
        {
            RaycastHit hit;
            Vector3 origin = transform.TransformPoint(wheelRestPos[i]) + transform.up * 0.1f;

            if (Physics.Raycast(origin, -transform.up, out hit, rideHeight + wheelRadius + 0.1f))
            {
                grounded[i] = true;
                float compress = rideHeight - (hit.distance - 0.1f - wheelRadius);
                rb.AddForceAtPosition(transform.up * (compress * springStrength - Vector3.Dot(transform.up, rb.GetPointVelocity(wheels[i].position)) * springDamper), wheels[i].position);
                springCompression[i] = Mathf.Clamp01(compress / rideHeight);

                if (tireTrails[i] != null)
                {
                    tireTrails[i].transform.position = hit.point + (hit.normal * 0.015f);
                    tireTrails[i].transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
                }
            }
            else grounded[i] = false;
        }
    }

    void UpdateEffects()
    {
        for (int i = 0; i < 4; i++)
        {
            if (tireTrails[i] == null) continue;
            bool skidding = Mathf.Abs(lateralSlip[i]) > skidThreshold || (handbrakeInput && i >= 2 && rb.linearVelocity.magnitude > 1f);
            tireTrails[i].emitting = grounded[i] && skidding;
        }
    }

    void ApplyGearsAndEngine(float speed)
    {
        if (!isEngineOn) { currentRPM = 0; return; }
        if (currentGear == 0)
        {
            currentRPM = Mathf.Lerp(currentRPM, Mathf.Lerp(idleRPM, maxRPM, throttleInput), Time.fixedDeltaTime * 5f);
        }
        else
        {
            float gearRatio = (currentGear == -1) ? gearRatios[0] : gearRatios[currentGear];
            float wheelRPM = (Mathf.Abs(speed) * 60f) / (2f * Mathf.PI * wheelRadius);
            currentRPM = Mathf.Clamp(wheelRPM * Mathf.Abs(gearRatio) * finalDriveRatio, idleRPM, maxRPM);

            if (currentRPM < rpmLimiter && throttleInput > 0.01f)
            {
                float wheelForce = (throttleInput * engineTorque * gearRatio * finalDriveRatio) / wheelRadius;
                rb.AddForceAtPosition(transform.forward * wheelForce, (wheels[RL].position + wheels[RR].position) / 2f);
            }
        }
    }

    void ApplySteering(float speed)
    {
        float speedFactor = 1f / (1f + Mathf.Abs(speed) * speedSteerDamping);
        smoothSteer = Mathf.Lerp(smoothSteer, steerInput * maxSteerAngle * speedFactor, Time.fixedDeltaTime * steerSpeed);
    }

    void ApplyTireForces(float speed)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!grounded[i]) { lateralSlip[i] = 0; continue; }

            Quaternion wRot = transform.rotation * Quaternion.Euler(0, (i < 2) ? smoothSteer : 0, 0);
            Vector3 right = wRot * Vector3.right;
            lateralSlip[i] = Vector3.Dot(right, rb.GetPointVelocity(wheels[i].position));

            float friction = (i < 2) ? frontGrip : baseRearGrip;
            if (i >= 2 && throttleInput > 0.5f && Mathf.Abs(smoothSteer) > 5f)
                friction -= oversteerFactor;

            if (handbrakeInput && i >= 2) friction = 0.1f;

            rb.AddForceAtPosition(-right * lateralSlip[i] * friction * sideFrictionDamping * rb.mass, wheels[i].position);
        }
    }

    void UpdateWheelVisuals(float speed)
    {
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i] == null) continue;
            wheelSpinAngle[i] += (speed / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
            wheels[i].localRotation = Quaternion.Euler(wheelSpinAngle[i], (i < 2) ? smoothSteer : 0, 0);
            if (grounded[i])
            {
                Vector3 pos = wheelRestPos[i];
                pos.y -= (rideHeight * (1f - springCompression[i]));
                wheels[i].localPosition = pos;
            }
        }
    }

    public bool IsWheelGrounded(int index) => grounded[index];
    public bool IsSkidding(int index) => Mathf.Abs(lateralSlip[index]) > skidThreshold || (handbrakeInput && index >= 2);
}