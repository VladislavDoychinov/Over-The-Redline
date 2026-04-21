using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Toggle mirrorToggle;
    public Toggle autoToggle;

    void Start()
    {
        if (mirrorToggle != null)
            mirrorToggle.isOn = PlayerPrefs.GetInt("ShowMirrors", 1) == 1;

        if (autoToggle != null)
            autoToggle.isOn = PlayerPrefs.GetInt("IsAutomatic", 1) == 1;
    }

    public void SetMirrors(bool isChecked)
    {
        PlayerPrefs.SetInt("ShowMirrors", isChecked ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetAutomatic(bool isChecked)
    {
        PlayerPrefs.SetInt("IsAutomatic", isChecked ? 1 : 0);
        PlayerPrefs.Save();
    }
}