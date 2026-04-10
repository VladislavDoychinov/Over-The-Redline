using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    public GameObject car1Prefab;
    public GameObject car2Prefab;

    void Start()
    {
        GameObject spawnedCar;

        if (CarSwitcher.SelectedCarID == 1)
        {
            spawnedCar = Instantiate(car1Prefab, transform.position, transform.rotation);
        }
        else
        {
            spawnedCar = Instantiate(car2Prefab, transform.position, transform.rotation);
        }

        spawnedCar.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}