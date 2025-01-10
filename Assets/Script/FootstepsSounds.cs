using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource footstepsSound;

    void Update()
    {
        ///Plays footsteps sounds when in motion
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){
            footstepsSound.enabled = true;
        }
        else
        {
            footstepsSound.enabled = false;
        }
    }
}
