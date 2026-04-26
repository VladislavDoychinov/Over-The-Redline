using UnityEngine;

public class CameraViewSwitcher : MonoBehaviour
{
    [Header("Targets")]
    public Transform carTransform;

    [Header("UI Elements")]
    public GameObject gaugeUI;
    public GameObject gaugeUI1;

    [Header("Offsets")]
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -4.4f);
    public Vector3 firstPersonOffset = new Vector3(-0.15f, 0.3f, 0.1f);
    public Vector3 secondPersonOffset = new Vector3(-0.6f, 0f, 0.4f);

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2.0f;
    public float maxLookAngle = 60f;
    private float yaw = 0f;
    private float pitch = 0f;

    [Header("Speed")]
    public float snapStiffness = 100f;

    private int cameraMode = 0;
    private CarController carController;

    void Start()
    {
        FindActiveCar();
        ApplyCarSpecificOffsets();
        UpdateUIVisibility();
    }

    private void ApplyCarSpecificOffsets()
    {
        if (CarSwitcher.SelectedCarID == 1)
        {
            thirdPersonOffset = new Vector3(0, 1.5f, -4.4f);
            firstPersonOffset = new Vector3(-0.2f, 0.3f, -0.24f);
            secondPersonOffset = new Vector3(-0.6f, -0.1f, 0.1f);
        }
        else
        {
            thirdPersonOffset = new Vector3(0, 1.5f, -4.0f);
            firstPersonOffset = new Vector3(-0.15f, 0.3f, 0.1f);
            secondPersonOffset = new Vector3(-0.6f, 0f, 0.4f);
        }
    }

    void Update()
    {
        if (carTransform == null || !carTransform.gameObject.activeInHierarchy)
        {
            FindActiveCar();
        }

        HandleCursorState();

        if (carController != null && carController.isPaused) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraMode = (cameraMode + 1) % 3;
            yaw = 0f;
            pitch = 0f;
            UpdateUIVisibility();
            SnapToTarget();
        }

        snapStiffness = (cameraMode == 0) ? 50f : 100f;

        if (cameraMode != 0)
        {
            HandleMouseLook();
        }
    }

    private void HandleCursorState(){
    if (Time.timeScale == 0)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return; 
    }

    if (carController == null) return;

    if (carController.isPaused)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    else
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
    private void FindActiveCar()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.activeInHierarchy)
            {
                carTransform = p.transform;
                carController = p.GetComponent<CarController>();
                ApplyCarSpecificOffsets();
                break;
            }
        }
    }

    private void HandleMouseLook()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        yaw = Mathf.Clamp(yaw, -90, 90);
    }

    private void UpdateUIVisibility()
    {
        if (gaugeUI != null) gaugeUI.SetActive(cameraMode == 0);
        if (gaugeUI1 != null) gaugeUI1.SetActive(cameraMode == 0);
    }

    void LateUpdate()
    {
        if (carTransform == null) return;

        Vector3 currentOffset;
        Quaternion targetRot;

        if (cameraMode == 1 || cameraMode == 2)
        {
            currentOffset = (cameraMode == 1) ? firstPersonOffset : secondPersonOffset;
            Quaternion mouseRotation = Quaternion.Euler(pitch, yaw, 0);
            targetRot = carTransform.rotation * mouseRotation;
        }
        else
        {
            currentOffset = thirdPersonOffset;
            Vector3 lookAtPoint = carTransform.position + (carTransform.up * 1.0f) + (carTransform.forward * 5.0f);
            targetRot = Quaternion.LookRotation(lookAtPoint - transform.position);
        }

        Vector3 targetPos = carTransform.position + (carTransform.rotation * currentOffset);

        if (snapStiffness >= 100f)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, snapStiffness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, snapStiffness * Time.deltaTime);
        }
    }

    private void SnapToTarget()
    {
        if (carTransform == null) return;
        Vector3 offset = (cameraMode == 1) ? firstPersonOffset : (cameraMode == 2) ? secondPersonOffset : thirdPersonOffset;
        transform.position = carTransform.position + (carTransform.rotation * offset);
        transform.rotation = carTransform.rotation;
    }
}