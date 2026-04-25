using UnityEngine;

public class PoliceAI : MonoBehaviour
{
    public Transform playerCar;
    public Vector3 followOffset = new Vector3(0, -0.5f, -15f);
    public float followStiffness = 5f;
    public float catchUpSpeed = 0.2f;

    private WinManager uiManager;

    void Start()
    {
        FindPlayer();
        uiManager = Object.FindAnyObjectByType<WinManager>();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerCar = player.transform;
    }

    void LateUpdate()
    {
        if (playerCar == null)
        {
            FindPlayer();
            return;
        }

        Vector3 targetPos = playerCar.position + (playerCar.rotation * followOffset);

        if (followOffset.z < -2.5f)
        {
            followOffset.z += catchUpSpeed * Time.deltaTime;
        }

        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist > 100f)
        {
            transform.position = targetPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followStiffness * Time.deltaTime);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, playerCar.rotation, followStiffness * Time.deltaTime);

        if (Vector3.Distance(transform.position, playerCar.position) < 3f)
        {
            if (uiManager != null)
            {
                uiManager.ShowLoseScreen();
            }
            else
            {
                Debug.Log("YOU ARE DEAD! (But WinManager was not found in the scene)");
                Time.timeScale = 0;
            }
        }
    }
}