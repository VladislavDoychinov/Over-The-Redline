using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSwitcher : MonoBehaviour
{
    public GameObject car1;
    public GameObject car2;

    public static int SelectedCarID = 1;

    void Start()
    {
        if (car1 == null || car2 == null)
        {
            Debug.LogError("CarSwitcher: Please assign both Car 1 and Car 2 in the Inspector!");
            return;
        }

        UpdateCarVisibility();
    }

    public void ShowCar1()
    {
        SelectedCarID = 1;
        UpdateCarVisibility();
    }

    public void ShowCar2()
    {
        SelectedCarID = 2;
        UpdateCarVisibility();
    }

    private void UpdateCarVisibility()
    {
        if (SelectedCarID == 1)
        {
            car1.SetActive(true);
            car2.SetActive(false);
        }
        else
        {
            car1.SetActive(false);
            car2.SetActive(true);
        }

        Debug.Log($"Car {SelectedCarID} is now active.");
    }

    public void StartRace()
    {
        SceneManager.LoadScene("TrackScene");
    }
}