using UnityEngine;

public class CarHealth : MonoBehaviour
{
    public int health = 10;
    public WinManager uiManager;
    void Start()
    {
        uiManager = Object.FindAnyObjectByType<WinManager>();

        //if (uiManager == null)
        //{
        //    Debug.LogError("CarHealth: Could not find WinManager in the scene! Make sure WinCanvas is in the hierarchy.");
        //}
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Health left: " + health);

        if (health <= 0)
        {
            if (uiManager != null)
            {
                uiManager.ShowLoseScreen();
            }
            else
            {
                Debug.Log("GAME OVER (But no UI Manager found to show screen)");
            }
        }
    }

    //void GameOver()
    //{
    //    Debug.Log("GAME OVER");
    //}
}