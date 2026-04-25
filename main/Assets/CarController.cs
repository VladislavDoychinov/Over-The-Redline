using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public enum DriveType { FWD, RWD, AWD }

    [Header("Game State")]
    private GameObject garageBtn;
    public bool isPaused = false;
    public bool isAutomatic = true;
    public GameObject mirrorObject;

    [Header("Drive Setup")]
    public DriveType driveTrain = DriveType.AWD;
    [Range(0, 1)] public float driftLevel = 0.3f;

    [Header("Engine & RPM Power")]
    public bool isEngineOn = true;
    public float maxTorqueNM = 580f;
    public float peakTorqueRPM = 4500f;
    public float idleRPM = 800f;
    public float maxRPM = 7500f;
    public float rpmLimiter = 7200f;
    public float currentRPM;

    [Header("Manual Transmission")]
    public float[] gearRatios = { -3.2f, 3.5f, 2.1f, 1.45f, 1.1f, 0.85f, 0.68f };
    public float finalDriveRatio = 3.42f;
    [Range(-1, 6)] public int currentGear = 1;

    [Header("Physics & Braking")]
    public float currentSpeed;
    public float brakeTorque = 12000f;
    public float handbrakeTorque = 22000f;
    public float stopThreshold = 0.6f;
    public float maxSteerAngle = 35f;
    public float steerSpeed = 8f;
    public float speedSteerDamping = 0.05f;

    [Header("Suspension & Center of Mass")]
    public float rideHeight = 0.25f;
    public float springStrength = 45000f;
    public float springDamper = 4500f;
    public float wheelRadius = 0.34f;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.75f, 0);

    [Header("Wheel Assignments")]
    public Transform wheelFL; public Transform wheelFR;
    public Transform wheelRL; public Transform wheelRR;
    public TrailRenderer[] tireTrails = new TrailRenderer[4];

    private Rigidbody rb;
    private float throttle, steer, brake, smoothSteer;
    private bool isHandbraking;
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
        rb.centerOfMass = centerOfMassOffset;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (rb.mass < 100) rb.mass = 1500f;

        garageBtn = GameObject.FindWithTag("GarageBtn");
        if (garageBtn != null)
        {
            garageBtn.SetActive(false);
        }

        if (mirrorObject != null)
        {
            mirrorObject.SetActive(PlayerPrefs.GetInt("ShowMirrors", 1) == 1);
        }
        isAutomatic = PlayerPrefs.GetInt("IsAutomatic", 1) == 1;
    }

    void Start()
    {
        wheels[FL] = wheelFL; wheels[FR] = wheelFR;
        wheels[RL] = wheelRL; wheels[RR] = wheelRR;
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i] != null)
                wheelRestPos[i] = transform.InverseTransformPoint(wheels[i].position);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) TogglePause();
        if (isPaused) return;

        HandleInputs();
        if (isAutomatic) HandleAutomaticShifting();
        UpdateVisualEffects();
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        float speedMS = Vector3.Dot(rb.linearVelocity, transform.forward);
        currentSpeed = speedMS * 3.6f;

        ApplySuspension();
        ApplySteering(speedMS);
        ApplyEnginePhysics(speedMS);
        ApplyTireForces(speedMS);
        ApplyBrakingLogic(speedMS);
        UpdateWheelPositions(speedMS);
    }

    public bool IsWheelGrounded(int index) => grounded[index];
    public bool IsSkidding(int index) => Mathf.Abs(lateralSlip[index]) > (1.15f - (driftLevel * 0.7f));

    private void HandleAutomaticShifting()
    {
        if (currentGear > 0 && currentRPM > rpmLimiter - 500 && currentGear < gearRatios.Length - 1)
            currentGear++;
        else if (currentGear > 1 && currentRPM < idleRPM + 1000)
            currentGear--;
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (isPaused) { throttle = 0; steer = 0; brake = 0; isHandbraking = false; }

        if (garageBtn == null)
        {
            garageBtn = GameObject.FindWithTag("GarageBtn");
        }

        if (garageBtn != null)
        {
            garageBtn.SetActive(isPaused);
        }
    }

    private void HandleInputs()
    {
        throttle = Mathf.Clamp01(Input.GetAxis("Vertical"));
        brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
        steer = Input.GetAxis("Horizontal");
        isHandbraking = Input.GetKey(KeyCode.Space);

        if (!isAutomatic && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            if (Input.GetKeyDown(KeyCode.B)) currentGear = -1;
            if (Input.GetKeyDown(KeyCode.Alpha0)) currentGear = 0;
            for (int i = 1; i <= 6; i++)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) currentGear = i;
        }
    }

    private void ApplyEnginePhysics(float speed)
    {
        if (!isEngineOn || currentGear == 0)
        {
            currentRPM = Mathf.Lerp(currentRPM, idleRPM + (throttle * 1200f), Time.fixedDeltaTime * 5f);
            return;
        }

        float gearRatio = (currentGear == -1) ? gearRatios[0] : gearRatios[currentGear];
        float wheelRPM = (Mathf.Abs(speed) * 60f) / (2f * Mathf.PI * wheelRadius);
        currentRPM = Mathf.Clamp(wheelRPM * Mathf.Abs(gearRatio) * finalDriveRatio, idleRPM, maxRPM);

        if (currentRPM < rpmLimiter && throttle > 0.01f)
        {
            float rpmNormalized = Mathf.Abs(currentRPM - peakTorqueRPM) / maxRPM;
            float torqueCurve = Mathf.Max(0.1f, 1.0f - rpmNormalized);
            float totalForce = (maxTorqueNM * torqueCurve * throttle * gearRatio * finalDriveRatio) / wheelRadius;

            if (driveTrain == DriveType.FWD) DistributeForce(FL, FR, totalForce);
            else if (driveTrain == DriveType.RWD) DistributeForce(RL, RR, totalForce);
            else { DistributeForce(FL, FR, totalForce * 0.5f); DistributeForce(RL, RR, totalForce * 0.5f); }
        }
    }

    private void DistributeForce(int w1, int w2, float force)
    {
        rb.AddForceAtPosition(transform.forward * (force * 0.5f), wheels[w1].position);
        rb.AddForceAtPosition(transform.forward * (force * 0.5f), wheels[w2].position);
    }

    private void ApplyTireForces(float speed)
    {
        for (int i = 0; i < 4; i++)
        {
            if (!grounded[i]) continue;
            Quaternion wRot = transform.rotation * Quaternion.Euler(0, (i < 2) ? smoothSteer : 0, 0);
            Vector3 sideDir = wRot * Vector3.right;
            lateralSlip[i] = Vector3.Dot(sideDir, rb.GetPointVelocity(wheels[i].position));
            float friction = (driftLevel <= 0.05f) ? 4.5f : (1.7f - driftLevel);
            if (driveTrain == DriveType.AWD) friction *= 1.2f;
            rb.AddForceAtPosition(-sideDir * lateralSlip[i] * friction * rb.mass * 0.25f, wheels[i].position);
        }
    }

    private void ApplyBrakingLogic(float speed)
    {
        if (brake > 0.1f || isHandbraking)
        {
            float bForce = isHandbraking ? handbrakeTorque : brakeTorque * brake;
            rb.AddForce(-transform.forward * bForce * Mathf.Sign(speed));
            if (Mathf.Abs(speed) < stopThreshold) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        }
    }

    private void ApplySteering(float speed)
    {
        float factor = 1f / (1f + Mathf.Abs(speed) * speedSteerDamping);
        smoothSteer = Mathf.Lerp(smoothSteer, steer * maxSteerAngle * factor, Time.fixedDeltaTime * steerSpeed);
    }

    private void ApplySuspension()
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
            }
            else grounded[i] = false;
        }
    }

    private void UpdateVisualEffects()
    {
        for (int i = 0; i < 4; i++)
        {
            if (tireTrails[i] != null) tireTrails[i].emitting = grounded[i] && IsSkidding(i);
        }
    }

    private void UpdateWheelPositions(float speed)
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
}