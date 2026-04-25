using UnityEngine;
using UnityEngine.Splines;
using System.Linq;

public class PhysicsSplineBot : MonoBehaviour
{
    public SplineContainer trackSpline;
    public float topSpeed = 20f;
    public float steeringPower = 10f;
    public float arrivalDistance = 3f;

    private Rigidbody rb;
    private int currentKnotIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        FindNearestKnot();
    }

    void FixedUpdate()
    {
        if (trackSpline == null || trackSpline.Spline.Count == 0) return;

        Vector3 targetPoint = trackSpline.transform.TransformPoint(trackSpline.Spline[currentKnotIndex].Position);

        float distanceToDot = Vector3.Distance(transform.position, targetPoint);

        if (distanceToDot < arrivalDistance)
        {
            currentKnotIndex++;

            if (currentKnotIndex >= trackSpline.Spline.Count)
            {
                currentKnotIndex = 0;
            }

            targetPoint = trackSpline.transform.TransformPoint(trackSpline.Spline[currentKnotIndex].Position);
        }

        Vector3 direction = (targetPoint - transform.position).normalized;

        if (rb.linearVelocity.magnitude < topSpeed)
        {
            rb.AddForce(transform.forward * topSpeed, ForceMode.Acceleration);
        }

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, lookRotation, steeringPower * Time.fixedDeltaTime));
        }
    }

    void FindNearestKnot()
    {
        float minDistance = float.MaxValue;
        for (int i = 0; i < trackSpline.Spline.Count; i++)
        {
            Vector3 knotPos = trackSpline.transform.TransformPoint(trackSpline.Spline[i].Position);
            float dist = Vector3.Distance(transform.position, knotPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                currentKnotIndex = i;
            }
        }
    }
}