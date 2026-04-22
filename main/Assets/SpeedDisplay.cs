using UnityEngine;
using TMPro;

public class SpeedDisplay : MonoBehaviour
{
    private TextMeshProUGUI speedText;
    private GameObject currentTarget;
    private Vector3 lastPosition;

    void Start()
    {
        speedText = GetComponent<TextMeshProUGUI>();
        FindActivePlayer();
    }

    void Update()
    {
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            FindActivePlayer();
            return;
        }

        float distance = Vector3.Distance(currentTarget.transform.position, lastPosition);
        float kmh = (distance / Time.deltaTime) * 3.6f;

        CarController car = currentTarget.GetComponent<CarController>();

        if (speedText != null && car != null)
        {
            if (!car.isEngineOn)
            {
                speedText.text = "Press 'E' to start the engine";
            }
            else
            {
                string gearName = car.currentGear == 0 ? "N" : car.currentGear.ToString();
                speedText.text = $"Gear: {gearName}\n";
            }
        }

        lastPosition = currentTarget.transform.position;
    }

    void FindActivePlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            currentTarget = player;
            lastPosition = currentTarget.transform.position;
        }
    }
}