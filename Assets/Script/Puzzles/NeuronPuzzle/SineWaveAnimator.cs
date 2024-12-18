using UnityEngine;

public class SineWaveActiveAnimator : MonoBehaviour // list with positions normalized
{
    public bool Initialized { get; private set; } = false;
    public bool Running { get; private set; } = true;
    private Vector3[] positions = new Vector3[0];
    float rotation = 0;
    float speed = 1;
    int length = 0;
    float size = 0;

    public Vector3[] GetPositions() => positions;

    public Vector3 GetPosition(int index)
    {
        if (index >= length)
        {
            Debug.LogError("Position index out of bounds");
            return Vector3.zero;
        }

        return positions[index];
    }

    public void Init(BrainLibrary.AxonType type, bool startRunning = true, bool active = false)
    {
        Running = startRunning;

        switch (type)
        {
            case BrainLibrary.AxonType.NORMAL:
                rotation = 180;
                length = 20;
                size = 30;
                speed = 1;
                break;

            case BrainLibrary.AxonType.VISUAL_ATTACHED:
                rotation = 180;
                length = 10;
                size = 15;
                speed = 1;
                break;

            case BrainLibrary.AxonType.VISUAl_UNATTACHED:
                rotation = 180;
                length = 10;
                size = 15;
                speed = 1;
                break;

            default:
                rotation = 180;
                length = 20;
                size = 30;
                speed = 1;
                break;
        }

        positions = new Vector3[length];

        float midPoint = (float)length / 2;

        for (int i = 0; i < length; i++)
        {
            float normalizedValue = (float)i / (length - 1);
            float multiplier = Mathf.Clamp01(
                Mathf.Abs(midPoint - Mathf.Abs(i - midPoint)) / midPoint
            );

            float widthMagnitude;
            if (active)
            {
                widthMagnitude =
                    Mathf.Sin(
                        normalizedValue * rotation * Mathf.Deg2Rad + (Time.time * speed * i / 3)
                    )
                    * multiplier
                    * 4f; // PI for complete arc
            }
            else
            {
                widthMagnitude =
                    Mathf.Sin(normalizedValue * rotation * Mathf.Deg2Rad + (Time.time * speed))
                    * multiplier
                    * 4f; // PI for complete arc
            }
            Vector3 interpolatedPosition = Vector3.Lerp(
                Vector3.zero,
                new Vector3(0, size, 0),
                (float)i / (length - 1)
            );

            positions[i] = interpolatedPosition + new Vector3(widthMagnitude, 0, 0);
        }

        Initialized = true;
    }

    public void Update()
    {
        if (!Running || !Initialized)
            return;

        positions = new Vector3[length];

        float midPoint = (float)length / 2;

        for (int i = 0; i < length; i++)
        {
            float normalizedValue = (float)i / (length - 1);
            float multiplier = Mathf.Clamp01(
                Mathf.Abs(midPoint - Mathf.Abs(i - midPoint)) / midPoint
            );

            float widthMagnitude =
                Mathf.Sin(normalizedValue * rotation * Mathf.Deg2Rad + (Time.time * speed))
                * multiplier
                * 4f; // PI for complete arc

            Vector3 interpolatedPosition = Vector3.Lerp(
                Vector3.zero,
                new Vector3(0, size, 0),
                (float)i / (length - 1)
            );

            positions[i] = interpolatedPosition + new Vector3(widthMagnitude, 0, 0);
        }
        return;
    }

    public void Play() => Running = true;

    public void Stop() => Running = false;
}
