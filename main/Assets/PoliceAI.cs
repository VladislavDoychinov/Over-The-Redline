using UnityEngine;

public class PoliceAI : MonoBehaviour
{
    public Transform playerCar;
    public Vector3 followOffset = new Vector3(0, -0.5f, -15f); // Stay further back to start
    public float followStiffness = 5f;
    public float catchUpSpeed = 0.2f;

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        // This looks for the actual car driving in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerCar = player.transform;
    }

    void LateUpdate()
    {
        if (playerCar == null)
        {
            FindPlayer(); // Keep looking if player is missing
            return;
        }

        // Calculate where the police car SHOULD be
        Vector3 targetPos = playerCar.position + (playerCar.rotation * followOffset);

        // Creep closer
        if (followOffset.z < -2.5f)
        {
            followOffset.z += catchUpSpeed * Time.deltaTime;
        }

        // THE FIX: Directly set position if too far, otherwise Lerp
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist > 100f)
        {
            // If the player is way ahead, just teleport the police car behind them
            transform.position = targetPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followStiffness * Time.deltaTime);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, playerCar.rotation, followStiffness * Time.deltaTime);

        // Kill check
        if (Vector3.Distance(transform.position, playerCar.position) < 3f)
        {
            Debug.Log("YOU ARE DEAD!");
        }
    }
}