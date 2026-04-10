using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    public Transform spawnPoint;

    void Start()
    {
        int selectedIndex = CarSelectionData.selectedCarIndex;

        if (selectedIndex < 0 || selectedIndex >= carPrefabs.Length)
        {
            Debug.LogWarning("Invalid car index, spawning first car.");
            selectedIndex = 0;
        }

        GameObject spawnedCar = Instantiate(
            carPrefabs[selectedIndex],
            spawnPoint.position,
            spawnPoint.rotation
        );

        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.target = spawnedCar.transform;
        }
        else
        {
            Debug.LogWarning("No CameraFollow found in scene.");
        }
    }
}