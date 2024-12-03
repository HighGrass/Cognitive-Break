using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    // Start is called before the first frame update
    SceneChanger sceneChanger;
    SphereCollider sphereCollider;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sceneChanger = FindAnyObjectByType<SceneChanger>();
    }

}
