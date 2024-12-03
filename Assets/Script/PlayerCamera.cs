using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    public float MouseSensitivity = 1f;
    public Vector2 VerticalLimits = new Vector2(-30f, 60f); // (min, max)
    public float RotationSmoothing = 10f;

    [Header("Body Rotation")]
    public Transform BodyTransform;
    public float MaxBodyAngle = 90f;

    [Header("Realistic Rotation")]
    public float MaxRealisticTilt = 2f; // Reduzido para evitar exageros
    public float TiltIntensity = 1.2f; // Reduzido para menor impacto

    private float currentTiltAngle = 0f;

    [SerializeField] SphereCollider detector;

    private void OnEnable()
    {
        detector = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        RotateHead();
        RotateBody();
        FixRotation();
        RotationTilt();
        LimitRotation();
    }

    private void RotateHead()
    {
        float mouseY = Mouse.current.delta.ReadValue().y;
        Quaternion newRotation =
            transform.rotation * Quaternion.AngleAxis(-mouseY * MouseSensitivity, Vector3.right);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.1f);
    }

    private void RotateBody()
    {
        float mouseX = Mouse.current.delta.ReadValue().x;
        Quaternion newRotation =
            transform.parent.transform.rotation
            * Quaternion.AngleAxis(-mouseX * MouseSensitivity, Vector3.down);
        transform.parent.transform.rotation = Quaternion.Lerp(
            transform.parent.transform.rotation,
            newRotation,
            0.1f
        );
    }

    private void FixRotation()
    {
        //Head
        Vector3 vector = transform.localRotation.eulerAngles;
        vector.y = 0;
        vector.z = 0;
        transform.localRotation = Quaternion.Euler(vector);
    }

    private void RotationTilt()
    {
        Vector3 vector = transform.rotation.eulerAngles;
        vector.z = Mathf.Clamp(
            Mathf.Lerp(currentTiltAngle, Mouse.current.delta.ReadValue().x, 0.03f),
            -MaxRealisticTilt,
            MaxRealisticTilt
        );
        currentTiltAngle = vector.z;
        transform.rotation = Quaternion.Euler(vector);
    }

    private void LimitRotation()
    {
        Vector3 cRotation = transform.rotation.eulerAngles;

        if (cRotation.x < 180)
        {
            if (cRotation.x > 60)
                cRotation.x = 60;
        }
        else
        {
            if (cRotation.x < 360 - 60)
                cRotation.x = 360 - 60;
        }

        transform.rotation = Quaternion.Euler(cRotation);
    }



}

