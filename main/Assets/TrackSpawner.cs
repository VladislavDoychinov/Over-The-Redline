using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;

public class TrackSpawner : MonoBehaviour
{
    public GameObject car1Prefab;
    public GameObject car2Prefab;
    public SplineContainer trackSpline;

    void Awake()
    {
        GameObject playerPrefab = (CarSwitcher.SelectedCarID == 1) ? car1Prefab : car2Prefab;

        GameObject playerCar = Instantiate(playerPrefab, transform.position, transform.rotation);
        playerCar.tag = "Player";
        SetupPhysics(playerCar);

        CameraViewSwitcher cam = Object.FindFirstObjectByType<CameraViewSwitcher>();
        if (cam != null) cam.carTransform = playerCar.transform;

        Vector3 spawnOffset = transform.right * -4f;
        GameObject botCar = Instantiate(playerPrefab, transform.position + spawnOffset, transform.rotation);
        botCar.name = "AI_Clone";
        botCar.tag = "Untagged";
        TrackManager tmanager = botCar.GetComponent<TrackManager>();
        tmanager.enabled = false;
        LapManager lapManager = botCar.GetComponent<LapManager>();
        lapManager.enabled = false;

        if (SceneManager.GetActiveScene().name != "TrackScene")
        {
            botCar.SetActive(false);
        }

        SetupPhysics(botCar);
        AttachAIDriver(botCar);
    }

    void SetupPhysics(GameObject car)
    {
        car.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        CarController controller = car.GetComponent<CarController>();
        if (controller != null && car.tag == "Player")
        {
            controller.isAutomatic = false;
        }
        else if (controller != null)
        {
            controller.isAutomatic = true;
        }
    }

    void AttachAIDriver(GameObject car)
    {
        car.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        AICarDriver driver = car.AddComponent<AICarDriver>();
        driver.trackSpline = trackSpline;

        driver.baseTargetSpeed = 160f;
        driver.lookAheadDistance = 25f;

        foreach (Renderer r in car.GetComponentsInChildren<Renderer>())
        {
            r.material.color = Color.red;
        }
    }
}