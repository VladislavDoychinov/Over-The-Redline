using UnityEngine;

public class CarSpawner1 : MonoBehaviour
{
    [Header("Car Prefabs")]
    public GameObject car1Prefab;
    public GameObject car2Prefab;

    [Header("Spawn Configuration")]
    public Transform spawnPoint;

    void Awake()
    {
        if (car1Prefab == null || car2Prefab == null)
        {
            Debug.LogError("Car prefabs are missing! Drag them into the CarSpawner inspector slots.");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject spawnedCar;

        if (CarSwitcher.SelectedCarID == 1)
        {
            spawnedCar = Instantiate(car1Prefab, spawnPos, spawnRot);
        }
        else
        {
            spawnedCar = Instantiate(car2Prefab, spawnPos, spawnRot);
        }

        spawnedCar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        Rigidbody rb = spawnedCar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        CameraViewSwitcher cam = Object.FindFirstObjectByType<CameraViewSwitcher>();
        if (cam != null)
        {
            cam.carTransform = spawnedCar.transform;
        }
        else
        {
            Debug.LogWarning("Car spawned, but CameraViewSwitcher was not found in the scene.");
        }
    }
}