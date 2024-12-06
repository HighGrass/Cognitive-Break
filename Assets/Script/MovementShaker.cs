using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MovementShaker : MonoBehaviour
{
    enum Foot
    {
        RightFoot,
        LeftFoot,
    }

    PlayerMovement playerMovement;
    float animationTime;
    bool finishingAnimation = false;
    Rigidbody rb;
    public Vector2 ShakeIntensity { get; private set; }

    [SerializeField]
    [Range(0, 0.6f)]
    float UpDownShakeMultiplier = 0.3f;

    [SerializeField]
    [Range(0, 2)]
    float SidesShakeMultiplier = 1f;
    public Vector3 CameraShakeAngle;
    float timeScale = 1f; // step time = 1s
    Vector3 defaultLocalPosition;
    public float Speed // player speed == animation speed
    {
        get => rb.velocity.magnitude;
    }

    Foot FootActive = Foot.RightFoot; // for good luck

    void SwitchFoot() // for restart animation
    {
        if (FootActive == Foot.RightFoot)
            StartFootAnimation(Foot.LeftFoot);
        else
            StartFootAnimation(Foot.RightFoot);
    }

    void Start()
    {
        defaultLocalPosition = transform.localPosition;
        rb = GetComponentInParent<Rigidbody>();
    }

    void Update(){
        ShakeIntensity = new Vector2(SidesShakeMultiplier, UpDownShakeMultiplier);
    }

    void FixedUpdate()
    {
        if (Speed < 1)
        {
            if (!finishingAnimation && animationTime * timeScale > 0.1f)
            {
                StartCoroutine(FinishAnimation());
                Debug.Log("FINISH ANIMATION: started");
            }

            return;
        }

        animationTime += (float)Speed / 200;
        if (animationTime * timeScale > 1)
            SwitchFoot();

        ShakeBody();
    }

    void StartFootAnimation(Foot foot)
    {
        animationTime = 0;
        FootActive = foot;
    }

    void ShakeBody()
    {
        float sin =
            Mathf.Sin(animationTime * timeScale * 360 * Mathf.Deg2Rad)
            * ShakeIntensity.y
            * Mathf.Clamp01((float)Speed / 15);
        float cos =
            Mathf.Sin(animationTime * timeScale * 180 * Mathf.Deg2Rad)
            * ShakeIntensity.x
            * Mathf.Clamp01((float)Speed / 15);

        Vector3 newVector = defaultLocalPosition + new Vector3(0, sin, 0);

        newVector.x =
            defaultLocalPosition.x
            + cos
                * Mathf.Cos(
                    Vector3.Angle(transform.localPosition, defaultLocalPosition) * Mathf.Deg2Rad
                );
        newVector.z =
            defaultLocalPosition.z
            + cos
                * Mathf.Sin(
                    Vector3.Angle(transform.localPosition, defaultLocalPosition) * Mathf.Deg2Rad
                );

        if (FootActive == Foot.LeftFoot)
        {
            newVector.x = -newVector.x;
            newVector.z = -newVector.z;
        }

        //float CameraShakeAngle =
        transform.localPosition = newVector;
        return;
    }

    IEnumerator FinishAnimation()
    {
        finishingAnimation = true;
        while (Vector3.Distance(transform.localPosition, defaultLocalPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Slerp(
                transform.localPosition,
                defaultLocalPosition,
                0.2f
            );
            yield return null;
        }
        SwitchFoot();
        finishingAnimation = false;
        Debug.Log("FINISH ANIMATION: finished");
        yield break;
    }
}
