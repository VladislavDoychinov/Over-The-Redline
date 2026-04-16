using UnityEngine;

public class CameraFollow2 : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;

    [Header("Offsets")]
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -4.4f);
    public Vector3 firstPersonOffset = new Vector3(-0.2f, 0.3f, -0.24f);
    public Vector3 secondPersonOffset = new Vector3(-0.6f, -0.1f, 0.1f);

    [Header("Speed")]
    [Tooltip("Set to 50+ for a locked-on feel. Set to 100+ for zero lag.")]
    public float snapStiffness = 100f;

    private int cameraMode = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraMode = (cameraMode + 1) % 3;

            SnapToTarget();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        Vector3 currentOffset;
        Quaternion targetRot;

        if (cameraMode == 1)
        {
            currentOffset = firstPersonOffset;
            targetRot = target.rotation;
        }
        else if (cameraMode == 2)
        {
            currentOffset = secondPersonOffset;
            targetRot = target.rotation;
        }
        else
        {
            currentOffset = thirdPersonOffset;
            Vector3 lookAtPoint = target.position + (target.up * 1.0f) + (target.forward * 5.0f);
            targetRot = Quaternion.LookRotation(lookAtPoint - transform.position);
        }

        Vector3 targetPos = target.position + (target.rotation * currentOffset);

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
        if (target == null) return;
        Vector3 offset = (cameraMode == 1) ? firstPersonOffset : (cameraMode == 2) ? secondPersonOffset : thirdPersonOffset;
        transform.position = target.position + (target.rotation * offset);
        transform.rotation = target.rotation;
    }
}