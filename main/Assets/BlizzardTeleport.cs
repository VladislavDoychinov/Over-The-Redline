using UnityEngine;

public class BlizzardTeleport : MonoBehaviour
{
    [Header("Settings")]
    public Transform destination;
    public bool destroyOnUse = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                //rb.position to avoid "stuttering"
                rb.position = destination.position;
                other.transform.rotation = destination.rotation;

                Debug.Log("Teleported");

                if (destroyOnUse)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}