using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron : MonoBehaviour
{
    [SerializeField]
    public bool Active = false;

    [SerializeField]
    public bool Movable = true;

    [HideInInspector]
    public Neuron[] CloseNeurons;
    public Axon[] Axons { get; private set; }
    public Neuron EnergySourceNeuron = null;
    public bool NeuronMoving { get; private set; } = false;
    Neuron[] ConnectedNeurons => GetConnectedNeurons();
    Info info;
    public float Energy = 0f;
    public float RotationSpeed { get; private set; } = 100f; //   90 graus / s
    private float rotationLeft = 0;
    LineRenderer[] lineRenderers;
    Color neuronColor;
    Coroutine RotationCoroutine = null;
    NeuronPuzzle neuronPuzzle;

    bool IsRotating => RotationCoroutine != null;

    private void Start()
    {
        neuronPuzzle = FindObjectOfType<NeuronPuzzle>();
        Axons = GetComponentsInChildren<Axon>();
        info = FindObjectOfType<Info>();
        lineRenderers = GetComponentsInChildren<LineRenderer>();
        neuronColor = lineRenderers[0].startColor;

        if (Active)
        {
            Active = true;
            Color sColor = Color.Lerp(
                neuronColor,
                Color.white,
                Mathf.Clamp01(Energy / info.NEURON_INITIAL_ENERGY)
            );
            EnergySourceNeuron = this;
            Energy = info.NEURON_INITIAL_ENERGY;
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.startColor = sColor;
                lineRenderer.endColor = sColor;
            }
        }
    }

    public void Activate()
    {
        if (EnergySourceNeuron == null)
            return;

        Color sColor = Color.Lerp(
            neuronColor,
            Color.white,
            Mathf.Clamp01(Energy / info.NEURON_INITIAL_ENERGY)
        );
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.startColor = sColor;
            lineRenderer.endColor = sColor;
        }

        Active = true;

        foreach (Axon axon in Axons)
        {
            if (
                axon.ThisNeuron.EnergySourceNeuron == this
                || EnergySourceNeuron != null
                    && axon.ThisNeuron.EnergySourceNeuron.Energy < EnergySourceNeuron.Energy
            ) // important
                continue;

            axon.ThisNeuron.FindPossibleEnergySource();
        }

        neuronPuzzle.UpdateNeuronCache(this);
        return;
    }

    public void Deactivate()
    {
        Debug.Log("Neuron deactivated: " + gameObject.name);
        Energy = 0;
        EnergySourceNeuron = null;
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.startColor = neuronColor;
            lineRenderer.endColor = neuronColor;
        }
        Active = false;
        neuronPuzzle.UpdateNeuronCache(this);
    }

    public void OnNeuronClick()
    {
        Debug.Log("NeuronClick()");
        if (!Movable)
            return;

        StartCoroutine(CheckNeuron(0.5f));

        if (IsRotating)
            return;

        RotateNeuron();
        NeuronMoving = true;
        return;
    }

    void DeactivateNeuronConnections()
    {
        Axon[] axons = GetComponentsInChildren<Axon>();
        foreach (Axon axon in axons)
        {
            if (axon.ConnectedAxon)
                axon.ConnectedAxon.Disconnect();
            axon.Disconnect();
        }
    }

    IEnumerator CheckNeuron(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("-------- Neuron Debug --------");
        Debug.Log("Neuron: " + gameObject.name);
        Debug.Log("Active: " + Active);
        Debug.Log("Energy: " + Energy);
        Debug.Log("Energy Source: " + EnergySourceNeuron);
        Debug.Log("Connections: " + ConnectedNeurons.Length);
        yield break;
    }

    public void ConnectToNeuron(Neuron neuron)
    {
        if (neuron == null)
        {
            Debug.Log("FATAL ERROR: Neuronio não definido");
            return;
        }
        FindPossibleEnergySource();

        return;
    }

    public void DisconnectFromNeuron(Neuron neuron)
    {
        FindPossibleEnergySource();

        Debug.LogWarning("Disconnected from neuron: " + neuron.name);
        return;
    }

    Neuron[] GetConnectedNeurons()
    {
        List<Neuron> cNeurons = new List<Neuron>();
        foreach (Axon axon in Axons)
        {
            if (!axon.GetComponent<Collider>() || !axon.gameObject.activeSelf)
                continue;
            if (axon.Connected)
                cNeurons.Add(axon.ThisNeuron);
        }

        return cNeurons.ToArray();
    }

    public Neuron FindPossibleEnergySource()
    {
        float maxEnergy = 0;
        Neuron newEnergySource = null;

        foreach (Neuron n in ConnectedNeurons)
        {
            if (
                n.EnergySourceNeuron != null
                && n.EnergySourceNeuron != this
                && n.EnergySourceNeuron.Active
                && n.EnergySourceNeuron.Energy - info.ENERGY_LOSS > 0
            )
            {
                if (n.Energy > maxEnergy)
                {
                    maxEnergy = n.Energy;
                    newEnergySource = n;
                }
            }
        }

        SetEnergySource(newEnergySource);
        return newEnergySource;
    }

    bool SetEnergySource(Neuron newEnergySource) // return if a new valid energy source was set
    {
        EnergySourceNeuron = newEnergySource;
        if (newEnergySource != null)
        {
            if (newEnergySource != this)
                Energy = info.NEURON_INITIAL_ENERGY - info.ENERGY_LOSS;
            else
                Energy = Mathf.Clamp(newEnergySource.Energy - info.ENERGY_LOSS, 0, float.MaxValue);
            Activate();
            return true;
        }
        else
            Deactivate();

        return false;
    }

    void RotateNeuron(float speed = 360f)
    {
        if (!IsRotating)
            RotationCoroutine = StartCoroutine(RotateNeuronCoroutine(speed));
    }

    IEnumerator RotateNeuronCoroutine(float rotationSpeed)
    {
        if (transform == null)
        {
            Debug.LogError("RotateNeuronCoroutine: transform is null");
            yield break;
        }

        Quaternion targetRotation =
            transform.localRotation * Quaternion.Euler(new Vector3(0, 0, 90));

        transform.localRotation *= Quaternion.Euler(0, 0, 2);

        while (true)
        {
            float yRotation = transform.localRotation.eulerAngles.z;
            if (Quaternion.Dot(transform.localRotation, targetRotation) > 0.999f)
            {
                transform.localRotation = targetRotation;
                break;
            }

            transform.localRotation *= Quaternion.Euler(0, 0, Time.deltaTime * rotationSpeed);

            yield return null;
        }
        if (RotationCoroutine != null)
        {
            RotationCoroutine = null;
        }
        RotationCoroutine = null;
        FindPossibleEnergySource();
        Debug.Log("Neuron finished rotation");
    }

    //void DetectNeuronBehavior() => FindPossibleEnergySource();
}
