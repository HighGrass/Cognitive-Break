using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNeuron : MonoBehaviour
{
    NeuronPuzzle gameManager;
    Neuron thisNeuron;
    Info info;
    bool gameFinished = false;

    private void Awake()
    {
        info = FindObjectOfType<Info>();
        thisNeuron = GetComponent<Neuron>();
        gameManager = FindObjectOfType<NeuronPuzzle>();
    }

    private void Start()
    {
        gameManager.StartRunning();
    }

    private void Update()
    {
        if (!thisNeuron)
        {
            Debug.LogError("Invalid Big Neuron");
            return;
        }
        if (thisNeuron.Active && thisNeuron.Energy < info.NEURON_INITIAL_ENERGY * 50)
        {
            if (!gameFinished)
            {
                StartCoroutine(FinishLevelWithDelay(7f));
            }
            gameFinished = true;
            thisNeuron.Energy += Time.deltaTime * 5;
            thisNeuron.Activate();
        }
    }

    IEnumerator FinishLevelWithDelay(float time)
    {
        yield return new WaitForSeconds(0.1f);

        Neuron[] allNeurons = FindObjectsOfType<Neuron>();
        foreach (Neuron neuron in allNeurons)
        {
            neuron.Movable = false;
        }
        yield return new WaitForSeconds(time);

        gameManager.FinishLevel();

        yield return new WaitForSeconds(4);
        Application.Quit();
    }
}
