using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSwitcher : MonoBehaviour
{
    public GameObject car1;
    public GameObject car2;

    public static int SelectedCarID = 1;

    void Start()
    {
        ShowCar1();
    }

    public void ShowCar1()
    {
        car1.SetActive(true);
        car2.SetActive(false);
        SelectedCarID = 1;
    }

    public void ShowCar2()
    {
        car1.SetActive(false);
        car2.SetActive(true);
        SelectedCarID = 2;
    }

    public void StartRace()
    {
        SceneManager.LoadScene("TrackScene");
    }
}