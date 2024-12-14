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

    private void Awake()
    {
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

    void Update()
    {
        RotateIfNeeded();
        //GetNextNeuron();

        if (Active)
        {
            if (!EnergySourceNeuron || EnergySourceNeuron.Energy <= 0)
                Deactivate();
            EnergySourceNeuron = FindPossibleEnergySource();
        }
        else
        {
            EnergySourceNeuron = FindPossibleEnergySource();
        }
    }

    public void Activate()
    {
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
    }

    public void Deactivate()
    {
        Energy = 0;
        EnergySourceNeuron = null;
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.startColor = neuronColor;
            lineRenderer.endColor = neuronColor;
        }
        Active = false;
    }

    void RotateIfNeeded()
    {
        if (!Movable)
            return;

        if (Mathf.Abs(rotationLeft) < 0.001)
        {
            if (NeuronMoving)
                NeuronMoving = false;
            return;
        }

        float rotationChange =
            RotationSpeed
                * Time.deltaTime
                * (
                    Mathf.Abs(Mathf.Sin(180 - Mathf.Clamp(Mathf.Abs(rotationLeft) * 2, 5, 180)))
                    * 10
                )
            + 0.1f * Time.deltaTime;

        if (rotationLeft - rotationChange < 0)
            rotationChange = rotationLeft;
        rotationLeft -= rotationChange;
        transform.rotation = Quaternion.Euler(
            new Vector3(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z - rotationChange
            )
        );
        //DeactivateNeuronConnections();
        return;
    }

    public void OnNeuronClick()
    {
        if (!Movable)
            return;

        Debug.Log("Neuron pressed");
        StartCoroutine(CheckNeuron(0.5f));
        if (Mathf.Abs(rotationLeft) > 0)
            return;

        rotationLeft = 90;
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

        if (neuron.Energy - info.ENERGY_LOSS > 0 && neuron.Energy - info.ENERGY_LOSS > Energy)
        {
            EnergySourceNeuron = neuron;

            if (info != null)
            {
                Energy = EnergySourceNeuron.Energy - info.ENERGY_LOSS;
            }
            else
            {
                Debug.LogError("Info object is null");
            }

            Activate();
        }

        return;
    }

    public void DisconnectFromNeuron(Neuron neuron)
    {
        Debug.LogWarning("Disconnected from neuron: " + neuron.name);

        EnergySourceNeuron = FindPossibleEnergySource();
        if (!EnergySourceNeuron)
            Deactivate();
        else
            Energy = EnergySourceNeuron.Energy - info.ENERGY_LOSS;

        return;
    }

    Neuron[] GetConnectedNeurons()
    {
        int connections = 0;

        foreach (Axon axon in Axons)
        {
            if (!axon.collider || !axon.gameObject.activeSelf)
                continue;
            if (axon.Connected)
                connections++;
        }

        Neuron[] ConnectedNeurons = new Neuron[connections];

        int index = 0;

        foreach (Axon axon in Axons)
        {
            if (!axon.collider || !axon.gameObject.activeSelf)
                continue;
            if (axon.Connected && index < connections)
            {
                ConnectedNeurons[index] = axon.ConnectedAxon.thisNeuron;
                index++;
            }
        }

        return ConnectedNeurons;
    }

    Neuron FindPossibleEnergySource()
    {
        float maxEnergy = 0;
        Neuron newEnergySource = null;

        foreach (Neuron n in ConnectedNeurons)
        {
            if (
                n.EnergySourceNeuron != null
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
        if (maxEnergy - info.ENERGY_LOSS <= 0 && EnergySourceNeuron != this)
        {
            return null;
        }

        if (EnergySourceNeuron != this)
        {
            EnergySourceNeuron = newEnergySource;
            if (!GetComponent<BigNeuron>())
                Energy = EnergySourceNeuron.Energy - info.ENERGY_LOSS;
        }
        else
        {
            if (maxEnergy <= Energy)
                newEnergySource = this;
        }
        Activate();
        return newEnergySource;
    }
}
