using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Quaternion HeadRealRotation { get; private set; }

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
    MovementShaker movementShaker;
    PlayerMovement playerMovement;
    Vector3 cameraMovementShakeHistory = Vector3.zero;

    [SerializeField]
    SphereCollider detector;
    public bool CameraLocked { get; private set; }

    private void OnEnable()
    {
        HeadRealRotation = transform.parent.transform.rotation;
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        movementShaker = FindAnyObjectByType<MovementShaker>();
        detector = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (CameraLocked)
            return;

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
        float mouseX = Mouse.current.delta.ReadValue().x * 0.1f;
        HeadRealRotation =
            HeadRealRotation * Quaternion.AngleAxis(-mouseX * MouseSensitivity, Vector3.down);
        transform.parent.transform.rotation = Quaternion.Lerp(
            transform.parent.transform.rotation,
            HeadRealRotation,
            0.4f
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
        Vector3 vector = transform.rotation.eulerAngles - cameraMovementShakeHistory;
        float angleDifference = Mathf.DeltaAngle(
            playerMovement.BodyRotationY,
            transform.rotation.eulerAngles.y
        );

        float finalAngle = angleDifference;

        float m = angleDifference / Mathf.Abs(angleDifference);

        if (playerMovement.Speed < 0.01f) // body static
        {
            if (Mathf.Abs(angleDifference) > 90) // Auto rotate body
            {
                playerMovement.SetBodyRotationY(
                    Mathf.Lerp(
                        transform.rotation.eulerAngles.y - 180 + m * 90f, // [ 1 / -1 ] * deltaTime
                        playerMovement.BodyRotationY,
                        Mathf.Abs(finalAngle) / 180
                    )
                );
            }
        }

        angleDifference = Mathf.Lerp(angleDifference * 2, 0, Mathf.Abs(finalAngle) / 180);

        vector.z = Mathf.Clamp(
            Mathf.Lerp(currentTiltAngle, -angleDifference * 0.1f, 0.3f),
            -MaxRealisticTilt,
            MaxRealisticTilt
        );
        currentTiltAngle = vector.z;
        cameraMovementShakeHistory = movementShaker.CameraShakeAngle;
        transform.rotation = Quaternion.Euler(vector);

        Debug.Log("BodyRotationY: " + playerMovement.BodyRotationY);
        Debug.Log("RealRotationY: " + transform.rotation.eulerAngles.y);
        Debug.Log("AngleDifference: " + angleDifference);
    }

    private float Deg2DualDeg(float degAngle) => 180 - LimitAngle360(degAngle); //  90 => 90 | 270 => -90

    private float DualDeg2Deg(float dualAngle) => LimitAngle360(180 + dualAngle);

    private float LimitAngle360(float angle)
    {
        if (angle > 360)
            angle = 360 - angle;
        else if (angle < 0)
            angle += 360;
        return angle;
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

    public void LockCamera() => CameraLocked = true;

    public void UnlockCamera() => CameraLocked = false;
}
