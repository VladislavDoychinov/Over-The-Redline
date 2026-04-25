using UnityEngine;

public class BlizzardMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 5, 15);
    public float smoothSpeed = 5f;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        //calculates the target position to the car
        Vector3 targetPos = player.position + (player.forward * offset.z) + (Vector3.up * offset.y);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        transform.LookAt(player);
    }
}