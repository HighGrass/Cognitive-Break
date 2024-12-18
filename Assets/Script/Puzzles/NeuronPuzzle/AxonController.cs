using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrainLibrary;
using Unity.VisualScripting;
using UnityEngine;

namespace BrainLibrary
{
    public enum AxonType
    {
        NORMAL,
        VISUAL_ATTACHED,
        VISUAl_UNATTACHED,
    }

    public enum NeuronType
    {
        NORMAL,
        INITIAL,
        FINAL,
    }
}

public class AxonController : MonoBehaviour
{
    List<Axon> allAxons;

    public SineWaveAnimator WaveActiveAnimator { get; private set; }
    public SineWaveAnimator WaveInactiveAnimator { get; private set; }

    public List<Axon> GetAllAxons() => allAxons;

    public void Start()
    {
        allAxons = FindObjectsOfType<Axon>().ToList();

        WaveActiveAnimator = gameObject.AddComponent<SineWaveAnimator>();
        WaveActiveAnimator.Init(BrainLibrary.AxonType.NORMAL, false, true); // create and stop

        WaveInactiveAnimator = gameObject.AddComponent<SineWaveAnimator>();
        WaveInactiveAnimator.Init(BrainLibrary.AxonType.NORMAL, false, false); // create and stop
    }
}
