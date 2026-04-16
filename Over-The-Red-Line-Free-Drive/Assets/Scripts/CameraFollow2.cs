using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Offsets")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, -4.4f);
    public Vector3 firstPersonOffset = new Vector3(-0.4f, 0.6f, -1f);
    public Vector3 secondPersonOffset = new Vector3(-1.4f, -0.1f, -0.3f);

    [Header("Look Offsets")]
    public Vector3 thirdPersonLookOffset = new Vector3(0f, 1f, 2f);
    public Vector3 firstPersonLookOffset = new Vector3(0f, 0.4f, 4f);
    public Vector3 secondPersonLookOffset = new Vector3(0f, 0.4f, 4f);

    [Header("Speed")]
    public float followSpeed = 5f;
    public float lookSpeed = 5f;

    private int currentCameraIndex = 0;
    // 0 = third person
    // 1 = first person
    // 2 = second person

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            currentCameraIndex++;

            if (currentCameraIndex > 2)
            {
                currentCameraIndex = 0;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 currentOffset = thirdPersonOffset;
        Vector3 currentLookOffset = thirdPersonLookOffset;

        if (currentCameraIndex == 1)
        {
            currentOffset = firstPersonOffset;
            currentLookOffset = firstPersonLookOffset;
        }
        else if (currentCameraIndex == 2)
        {
            currentOffset = secondPersonOffset;
            currentLookOffset = secondPersonLookOffset;
        }

        Vector3 desiredPosition = target.position + target.TransformDirection(currentOffset);
        Vector3 lookPoint = target.position + target.TransformDirection(currentLookOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - desiredPosition);

        if (currentCameraIndex == 1 || currentCameraIndex == 2)
        {
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSpeed * Time.deltaTime);
        }
    }
}