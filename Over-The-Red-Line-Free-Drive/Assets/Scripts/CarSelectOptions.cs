using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelectOptions : MonoBehaviour
{
    public TMP_Text selectedCarText;
    public void SelectCar(int carIndex) {
        CarSelectionData.selectedCarIndex = carIndex;
        selectedCarText.text = "Selected: Car " + (carIndex + 1);
        Debug.Log("Selected Car: " + carIndex);
    }
    public void StartGame()
    {
        if (CarSelectionData.selectedCarIndex == -1) {
            Debug.Log("No car selected");
            selectedCarText.text = "Select a car first.";
            return;
        }
        Debug.Log("S1");
        SceneManager.LoadScene("s1");
        
    }

    public void BacktoMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("main menu");
    }
}
