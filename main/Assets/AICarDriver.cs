using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class AICarDriver : MonoBehaviour
{
    public SplineContainer trackSpline;
    private bool hasHitCheckpoint = false;
    private WinManager uiManager;

    [Header("Smoothness Settings")]
    public float baseTargetSpeed = 70f;
    public float lookAheadDistance = 25f;
    public float trackStiffness = 3f;

    private CarController controller;
    private Rigidbody rb;
    private float currentPathProgress = 0f;

    private System.Reflection.FieldInfo throttleField;
    private System.Reflection.FieldInfo steerField;
    private System.Reflection.FieldInfo brakeField;

    void Start()
    {
        uiManager = Object.FindAnyObjectByType<WinManager>();
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CarController>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        throttleField = typeof(CarController).GetField("throttle", flags);
        steerField = typeof(CarController).GetField("steer", flags);
        brakeField = typeof(CarController).GetField("brake", flags);
    }

    void FixedUpdate()
    {
        if (trackSpline == null || controller == null) return;

        float3 localPos = trackSpline.transform.InverseTransformPoint(transform.position);

        SplineUtility.GetNearestPoint(trackSpline.Spline, localPos, out float3 nearestLocal, out float t);
        currentPathProgress = t;

        Vector3 nearestWorld = trackSpline.transform.TransformPoint((Vector3)nearestLocal);
        Vector3 offsetToSpline = nearestWorld - transform.position;
        rb.AddForce(offsetToSpline * trackStiffness * rb.mass, ForceMode.Force);

        float splineLength = trackSpline.CalculateLength();
        float steerT = (currentPathProgress + (lookAheadDistance / splineLength)) % 1f;
        Vector3 targetWorldPos = trackSpline.EvaluatePosition(steerT);
        Vector3 localTarget = transform.InverseTransformPoint(targetWorldPos);

        float angleToTarget = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steerInput = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        float previewT = (currentPathProgress + (30f / splineLength)) % 1f;
        Vector3 previewDir = (Vector3)trackSpline.EvaluateTangent(previewT);
        float forwardDot = Vector3.Dot(transform.forward, previewDir.normalized);
        float dynamicSpeed = baseTargetSpeed * Mathf.Clamp(forwardDot, 0.5f, 1.0f);

        ApplyInputs(steerInput, dynamicSpeed);
    }

    private void ApplyInputs(float steerInput, float targetSpeed)
    {
        steerField?.SetValue(controller, steerInput * 2.0f);

        if (controller.currentSpeed < targetSpeed)
        {
            throttleField?.SetValue(controller, 1f);
            brakeField?.SetValue(controller, 0f);
        }
        else
        {
            throttleField?.SetValue(controller, 0f);
            if (controller.currentSpeed > targetSpeed + 5f)
                brakeField?.SetValue(controller, 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint")) hasHitCheckpoint = true;

        if (other.CompareTag("Finish") && hasHitCheckpoint)
        {
            LoseGame();
        }
    }

    void LoseGame()
    {

        if (uiManager != null)
        {
            uiManager.ShowLoseScreen();
        }
        else
        {
            Time.timeScale = 0;
            Debug.LogError("WinManager not found! Game paused but no UI shown.");
        }
    }
}