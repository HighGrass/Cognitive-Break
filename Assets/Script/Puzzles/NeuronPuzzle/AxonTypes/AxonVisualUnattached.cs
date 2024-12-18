using System.Collections;
using UnityEngine;

public class AxonVisualUnattached : Axon
{
    public LineRenderer Render { get; private set; }
    private AxonNormal mainAxon;

    public void Awake()
    {
        Render = GetComponent<LineRenderer>();
        mainAxon = GetComponentInParent<AxonNormal>();
    }

    public override IEnumerator Animation()
    {
        while (true)
        {
            if (!Controller.WaveActiveAnimator)
                yield return null; // wait for animator

            float rootPosition = mainAxon.Render.GetPosition(10).x;

            Vector3[] positions = Controller.WaveActiveAnimator.GetPositions();
            Vector3[] newPositions = new Vector3[positions.Length];
            for (int i = 0; i < newPositions.Length; i++)
            {
                newPositions[i] += new Vector3(rootPosition, 0, 0);
            }
            Render.SetPositions(positions);

            yield return null;
        }
    }
}
