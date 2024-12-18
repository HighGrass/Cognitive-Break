using System.Collections;
using UnityEngine;

public class AxonNormal : Axon
{
    public LineRenderer Render { get; private set; }

    public void Awake()
    {
        Render = GetComponent<LineRenderer>();
    }

    public override IEnumerator Animation()
    {
        while (true)
        {
            if (Connected && Controller.WaveActiveAnimator)
            {
                Vector3[] positions = Controller.WaveActiveAnimator.GetPositions();
                Render.SetPositions(positions);
            }
            else if (!Connected && Controller.WaveInactiveAnimator)
            {
                Vector3[] positions = Controller.WaveInactiveAnimator.GetPositions();
                Render.SetPositions(positions);
            }
            yield return null;
        }
    }
}
