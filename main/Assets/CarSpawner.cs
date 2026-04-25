using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    public Transform spawnPoint;

    void Start()
    {
        int selectedIndex = CarSelectionData.selectedCarIndex;

        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned to the CarSpawner!");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= carPrefabs.Length)
        {
            Debug.LogWarning("Invalid car index, defaulting to first car.");
            selectedIndex = 0;
        }

        GameObject spawnedCar = Instantiate(
            carPrefabs[selectedIndex],
            spawnPoint.position,
            spawnPoint.rotation
        );

        spawnedCar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.target = spawnedCar.transform;
        }
        else
        {
            Debug.LogWarning("No CameraFollow component found in the scene.");
        }
    }
}