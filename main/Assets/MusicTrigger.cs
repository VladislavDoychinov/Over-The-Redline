using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AudioSource songToPlay; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            songToPlay.Play();

            //// prevent second trigger
            //Destroy(gameObject);
        }
    }
}