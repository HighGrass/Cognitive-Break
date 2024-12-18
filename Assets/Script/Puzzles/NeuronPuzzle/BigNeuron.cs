using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNeuron : MonoBehaviour
{
    NeuronPuzzle gameManager;
    Neuron ThisNeuron;
    Info info;
    bool gameFinished = false;

    private void Awake()
    {
        info = FindObjectOfType<Info>();
        ThisNeuron = GetComponent<Neuron>();
        gameManager = FindObjectOfType<NeuronPuzzle>();
    }

    private void Start()
    {
        gameManager.StartRunning();
    }

    private void Update()
    {
        if (!ThisNeuron)
        {
            Debug.LogError("Invalid Big Neuron");
            return;
        }
        if (ThisNeuron.Active && ThisNeuron.Energy < info.NEURON_INITIAL_ENERGY)
        {
            if (!gameFinished)
            {
                StartCoroutine(StopRunningWithDelay(7f));
            }
            gameFinished = true;
            ThisNeuron.Energy += Time.deltaTime * 5;
        }
    }

    void DisplayFinalAnimation() => StartCoroutine(FinalAnimation());

    IEnumerator FinalAnimation()
    {
        while (ThisNeuron.Active && ThisNeuron.Energy < info.NEURON_INITIAL_ENERGY)
        {
            yield return null;
        }
        yield break;
    }

    IEnumerator StopRunningWithDelay(float time)
    {
        yield return new WaitForSeconds(0.1f);

        Neuron[] allNeurons = FindObjectsOfType<Neuron>();
        foreach (Neuron neuron in allNeurons)
        {
            neuron.Movable = false;
        }
        yield return new WaitForSeconds(time);

        gameManager.StopRunning();

        yield return new WaitForSeconds(4);
        Application.Quit();
    }
}
