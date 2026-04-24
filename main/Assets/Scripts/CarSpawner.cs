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
        // 1. Safety Check: Make sure prefabs are assigned in the Inspector
        if (car1Prefab == null || car2Prefab == null)
        {
            Debug.LogError("Car prefabs are missing! Drag them into the CarSpawner inspector slots.");
            return;
        }

        // 2. Determine Position/Rotation
        // If spawnPoint is null, it uses the Spawner's own position
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject spawnedCar;

        // 3. Selection Logic 
        // Note: This assumes you have a static class/variable named CarSwitcher.SelectedCarID
        if (CarSwitcher.SelectedCarID == 1)
        {
            spawnedCar = Instantiate(car1Prefab, spawnPos, spawnRot);
        }
        else
        {
            spawnedCar = Instantiate(car2Prefab, spawnPos, spawnRot);
        }

        // 4. Apply Scale
        spawnedCar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // 5. Physics Setup
        Rigidbody rb = spawnedCar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // 6. Camera Setup
        // Using FindFirstObjectByType (Newer Unity version standard)
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