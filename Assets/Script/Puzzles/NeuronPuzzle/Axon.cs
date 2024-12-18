using System.Collections;
using UnityEngine;

public abstract class Axon : MonoBehaviour
{
    public bool Active { get; private set; } = false;
    public bool Connected => ConnectedAxon != null;
    public float Energy { get; private set; }
    public Neuron ThisNeuron { get; private set; }
    public Axon ConnectedAxon { get; private set; }
    public Coroutine AnimationCoroutine { get; private set; }

    public AxonController Controller { get; private set; }
    private NeuronPuzzle neuronPuzzle;

    public void Start()
    {
        Controller = FindObjectOfType<AxonController>();
        neuronPuzzle = FindObjectOfType<NeuronPuzzle>();
        ThisNeuron = transform.parent.GetComponentInParent<Neuron>();

        if (!ThisNeuron)
        {
            Debug.LogWarning("Neuron script not found in " + transform.parent.name);
            return;
        }

        if (Active)
            Activate();
    }

    public void Activate()
    {
        if (Active)
        {
            Debug.LogWarning("Axon is already active");
            return;
        }
        Active = true;
        AnimationCoroutine = StartCoroutine(Animation());
    }

    public void Deactivate()
    {
        Active = false;

        try
        {
            if (AnimationCoroutine != null)
                StopCoroutine(AnimationCoroutine);
        }
        catch (System.Exception) { }

        AnimationCoroutine = null;
    }

    public abstract IEnumerator Animation();

    public void Connect(Axon axon) => ConnectedAxon = axon;

    public void Disconnect() => ConnectedAxon = null;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Axon>())
        {
            Connect(other.gameObject.GetComponent<Axon>());
            if (ThisNeuron.Movable)
                ThisNeuron.ConnectToNeuron(other.gameObject.GetComponent<Axon>().ThisNeuron);
            neuronPuzzle.UpdateNeuronCache(ThisNeuron);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Axon otherAxon = other.gameObject.GetComponent<Axon>();

        if (!otherAxon)
            return;

        Disconnect();
        if (ThisNeuron.Movable)
            ThisNeuron.DisconnectFromNeuron(otherAxon.ThisNeuron);

        neuronPuzzle.UpdateNeuronCache(ThisNeuron);
    }
}
