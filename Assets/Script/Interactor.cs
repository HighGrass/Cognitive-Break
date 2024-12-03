using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    // Start is called before the first frame update
    SceneChanger sceneChanger;

    private void Start()
    {
        sceneChanger = FindAnyObjectByType<SceneChanger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            sceneChanger.LoadSceneByName("EmotionScene");
        }
    }
}
