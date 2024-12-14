using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info : MonoBehaviour
{
    public float NEURON_INITIAL_ENERGY { get; private set; } = 24f;
    public float AXON_MAX_ENERGY { get; private set; } = 6f;
    public float ENERGY_LOSS { get; private set; } = 1f;

    public float MIN_NORMAL_AXON_ENERGY { get; private set; } = 1;
    public float MAX_NORMAL_AXON_ENERGY { get; private set; } = 3;
    public float MIN_SMALL_AXON_ENERGY { get; private set; } = 1;
    public float MAX_SMALL_AXON_ENERGY { get; private set; } = 5;
    public float MIN_CONNECTED_AXON_ENERGY { get; private set; } = 3f;
    public float MAX_CONNECTED_AXON_ENERGY { get; private set; } = 4f;
}
