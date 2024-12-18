using System.Collections;
using UnityEngine;

public class VisualAttachedAxon : Axon
{
    public LineRenderer Render { get; private set; }
    private AxonNormal mainAxon;

    public void Awake()
    {
        Render = GetComponent<LineRenderer>();
    }

    public override IEnumerator Animation()
    {
        while (true)
        {
            if (!Controller.WaveActiveAnimator)
                yield return null; // wait for animator

            Vector3[] positions = Controller.WaveActiveAnimator.GetPositions();
            Render.SetPositions(positions);

            yield return null;
        }
    }
}
