using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axon : MonoBehaviour
{
    public Axon ConnectedAxon { get; private set; }
    public Neuron thisNeuron => GetComponentInParent<Neuron>();
    public SphereCollider collider;

    Info info;
    public float Size { get; private set; }
    public bool Connected => ConnectedAxon != null;

    [SerializeField]
    public bool isSmallNeuron = false;
    public bool isAttachedAxon = false;
    public Axon MainNeuron { get; private set; } = null;
    public float Energy =>
        Mathf.Clamp(
            (thisNeuron.Energy * 5f / info.NEURON_INITIAL_ENERGY) + 1,
            1,
            info.AXON_MAX_ENERGY
        );

    float AnimationOffset = 0;

    LineRenderer lineRenderer;

    private void Awake()
    {
        info = FindObjectOfType<Info>();
        lineRenderer = GetComponent<LineRenderer>();
        AnimationOffset = Random.Range(5, 20);

        collider = GetComponent<SphereCollider>();

        Size = lineRenderer.GetPosition(lineRenderer.positionCount - 1).y;

        if (
            isSmallNeuron && transform.parent && transform.parent.GetComponent<Axon>()
            || isAttachedAxon && transform.parent && transform.parent.GetComponent<Axon>()
        )
            MainNeuron = transform.parent.GetComponent<Axon>();
    }

    void Update()
    {
        ControlAxon();
    }

    public void Connect(Axon axon) => ConnectedAxon = axon;

    public void Disconnect() => ConnectedAxon = null;

    void ControlAxon()
    {
        if (!isSmallNeuron)
        {
            if (!isAttachedAxon)
            {
                if (Size > 25)
                {
                    if (Connected)
                    {
                        Vector3 initialLocalPosition = Vector3.zero;
                        Vector3 finalLocalPosition = new Vector3(0, Size, 0);

                        float midPoint = (float)lineRenderer.positionCount / 2;
                        for (int i = 0; i < lineRenderer.positionCount; i++)
                        {
                            float normalizedValue = (float)i / (lineRenderer.positionCount - 1);
                            float multiplier = Mathf.Clamp01(
                                Mathf.Abs(midPoint - Mathf.Abs(i - midPoint)) / midPoint
                            );

                            float widthMagnitude =
                                Mathf.Sin(
                                    normalizedValue * 180 * Mathf.Deg2Rad
                                        + (Time.time * Energy + normalizedValue / 3)
                                )
                                * multiplier
                                * 4f; // Use PI para o arco completo

                            Vector3 arcOffset = new Vector3(widthMagnitude, 0, 0);

                            Vector3 interpolatedPosition = Vector3.Lerp(
                                initialLocalPosition,
                                finalLocalPosition,
                                (float)i / (lineRenderer.positionCount - 1)
                            );

                            lineRenderer.SetPosition(i, interpolatedPosition + arcOffset);
                        }

                        lineRenderer.SetPosition(0, initialLocalPosition);
                        lineRenderer.SetPosition(
                            lineRenderer.positionCount - 1,
                            finalLocalPosition
                        );
                    }
                    else
                    { //small inactive axon
                        Vector3 initialLocalPosition = Vector3.zero;
                        Vector3 finalLocalPosition = new Vector3(0, Size, 0);

                        float midPoint = (float)lineRenderer.positionCount / 2;
                        for (int i = 0; i < lineRenderer.positionCount; i++)
                        {
                            float normalizedValue = (float)i / (lineRenderer.positionCount - 1);

                            float widthMagnitude =
                                Mathf.Sin(
                                    normalizedValue * 180 * Mathf.Deg2Rad
                                        + (Time.time * Energy - normalizedValue * i / 3)
                                        + AnimationOffset
                                )
                                * normalizedValue
                                * i
                                / 2; // Use PI para o arco completo

                            Vector3 arcOffset = new Vector3(widthMagnitude, 0, 0);

                            Vector3 interpolatedPosition = Vector3.Lerp(
                                initialLocalPosition,
                                finalLocalPosition,
                                (float)i / (lineRenderer.positionCount - 1)
                            );

                            lineRenderer.SetPosition(i, interpolatedPosition + arcOffset);
                        }

                        lineRenderer.SetPosition(0, initialLocalPosition);
                    }
                }
                else
                {
                    Vector3 initialLocalPosition = Vector3.zero;
                    Vector3 finalLocalPosition = new Vector3(0, Size, 0);

                    float midPoint = (float)lineRenderer.positionCount / 2;
                    for (int i = 0; i < lineRenderer.positionCount; i++)
                    {
                        float normalizedValue = (float)i / (lineRenderer.positionCount - 1);

                        float widthMagnitude =
                            Mathf.Sin(
                                normalizedValue * 180 * Mathf.Deg2Rad
                                    + (Time.time * Energy - normalizedValue * i / 3)
                                    + AnimationOffset
                            )
                            * normalizedValue
                            * i
                            / 2; // Use PI para o arco completo

                        Vector3 arcOffset = new Vector3(widthMagnitude, 0, 0);

                        Vector3 interpolatedPosition = Vector3.Lerp(
                            initialLocalPosition,
                            finalLocalPosition,
                            (float)i / (lineRenderer.positionCount - 1)
                        );

                        lineRenderer.SetPosition(i, interpolatedPosition + arcOffset);
                    }

                    lineRenderer.SetPosition(0, initialLocalPosition);
                }
            }
            else
            { // Small neuron not final attached (visual)
                Vector3 initialLocalPosition = new Vector3(
                    0,
                    -MainNeuron.GetComponent<LineRenderer>().GetPosition(10).x,
                    0
                );
                Vector3 finalLocalPosition = new Vector3(
                    0,
                    -MainNeuron.GetComponent<LineRenderer>().GetPosition(10).x + Size,
                    0
                );

                float midPoint = (float)lineRenderer.positionCount / 2;
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    float normalizedValue = (float)i / (lineRenderer.positionCount - 1);

                    float widthMagnitude =
                        Mathf.Sin(
                            normalizedValue * 180 * Mathf.Deg2Rad
                                + (Time.time * Energy - normalizedValue * i / 3)
                                + AnimationOffset
                        )
                        * normalizedValue
                        * i
                        / 2; // Use PI para o arco completo

                    Vector3 arcOffset = new Vector3(widthMagnitude, 0, 0);

                    Vector3 interpolatedPosition = Vector3.Lerp(
                        initialLocalPosition,
                        finalLocalPosition,
                        (float)i / (lineRenderer.positionCount - 1)
                    );

                    lineRenderer.SetPosition(i, interpolatedPosition + arcOffset);
                }

                lineRenderer.SetPosition(0, initialLocalPosition);
            }
        }
        else // Small neuron (visual)
        {
            Vector3 initialLocalPosition = MainNeuron.lineRenderer.GetPosition(5);
            Vector3 finalLocalPosition = MainNeuron.lineRenderer.GetPosition(15);

            float typeMultiplier = 4;
            if (MainNeuron.Connected)
                typeMultiplier = 6;

            float midPoint = (float)lineRenderer.positionCount / 2;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float normalizedValue = (float)i / (lineRenderer.positionCount - 1);
                float multiplier = Mathf.Clamp01(
                    Mathf.Abs(midPoint - Mathf.Abs(i - midPoint)) / midPoint
                );

                float widthMagnitude =
                    Mathf.Sin(
                        normalizedValue * 180 * Mathf.Deg2Rad
                            + (Time.time * Energy + normalizedValue / 3)
                    )
                    * multiplier
                    * typeMultiplier; // Use PI para o arco completo

                Vector3 arcOffset = new Vector3(widthMagnitude, 0, 0);

                Vector3 interpolatedPosition = Vector3.Lerp(
                    initialLocalPosition,
                    finalLocalPosition,
                    (float)i / (lineRenderer.positionCount - 1)
                );

                lineRenderer.SetPosition(i, interpolatedPosition + arcOffset);
            }

            lineRenderer.SetPosition(0, initialLocalPosition);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, finalLocalPosition);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Axon>())
        {
            Connect(other.gameObject.GetComponent<Axon>());
            if (thisNeuron.Movable)
                thisNeuron.ConnectToNeuron(other.gameObject.GetComponent<Axon>().thisNeuron);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Axon otherAxon = other.gameObject.GetComponent<Axon>();

        if (!otherAxon)
            return;

        Disconnect();
        if (thisNeuron.Movable)
            thisNeuron.DisconnectFromNeuron(otherAxon.thisNeuron);
    }
}
