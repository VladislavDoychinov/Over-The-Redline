using UnityEngine;

public class GhostCar : MonoBehaviour
{
    public Transform mainCar;
    public CarHealth mainHealth;

    public Vector3 followOffset = new Vector3(-7f, 0f, 0f);
    public float followStiffness = 10f;

    void LateUpdate()
    {
        if (mainCar == null) return;

        Vector3 targetPos = mainCar.position + (mainCar.rotation * followOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, followStiffness * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, mainCar.rotation, followStiffness * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard") && mainHealth != null)
        {
            mainHealth.TakeDamage(1);
            Debug.Log("Ghost hit! Main car damaged.");
        }
    }
}