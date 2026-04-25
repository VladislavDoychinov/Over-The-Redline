using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject icePrefab;
    public float throwForce = 30f;
    public float fireRate = 1.5f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //throw every x seconds
        InvokeRepeating("ThrowAtPlayer", 2f, fireRate);
    }

    void ThrowAtPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        GameObject ghostObj = GameObject.FindGameObjectWithTag("Ghost");

        Transform finalTarget = null;

        if (playerObj != null && ghostObj != null)
        {
            finalTarget = (Random.value > 0.5f) ? playerObj.transform : ghostObj.transform;
        }
        else if (playerObj != null)
        {
            finalTarget = playerObj.transform;
        }

        if (finalTarget == null) return;

        GameObject bolt = Instantiate(icePrefab, transform.position, Quaternion.identity);
        Vector3 direction = (finalTarget.position - transform.position).normalized;

        Rigidbody rb = bolt.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * throwForce, ForceMode.Impulse);
        }

        Destroy(bolt, 4f);
    }
}