using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeuronPuzzle : MonoBehaviour, IPuzzle
{
    [SerializeField]
    Color currentColor;
    bool Running = false;
    int gameState = 0; // ( começar (1) , ignorar (0) , terminar(-1) )
    float colorChange = 0.33f; //3s
    AxonController axonController;

    public Dictionary<Neuron, List<Axon>> NeuronsCache { get; private set; } =
        new Dictionary<Neuron, List<Axon>>();

    public void Start()
    {
        axonController = FindObjectOfType<AxonController>();
    }

    public bool IsRunning() => Running;

    public void OnExitPuzzle()
    {
        StopRunning();
    }

    public void StartRunning()
    {
        Running = true;
        axonController.WaveActiveAnimator.Play();
        axonController.WaveInactiveAnimator.Play();
        axonController.GetAllAxons().ForEach(x => x.Activate());
    }

    public void StopRunning()
    {
        Running = false;
        axonController.WaveActiveAnimator.Stop();
        axonController.WaveInactiveAnimator.Stop();
        axonController.GetAllAxons().ForEach(x => x.Deactivate());
    }

    public void OnFinishPuzzle() { }

    public void UpdateNeuronCache(Neuron neuron)
    {
        if (!NeuronsCache.Keys.Contains(neuron))
        { // create cache
            NeuronsCache.Add(neuron, neuron.Axons.ToList());
        }
        else
        { // update cache
            NeuronsCache[neuron] = neuron.Axons.ToList();
        }
    }
}
