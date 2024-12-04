using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public bool Interacting { get; private set; } = false;
    Image aimImage;
    public Vector3 TargetSize { get; private set; } = Vector3.one * 0.00005f;

    public void Awake()
    {
        Cursor.visible = false;
        aimImage = GetComponent<Image>();
    }

    public void StartInteraction()
    {
        Interacting = true;
        TargetSize = Vector3.one * 0.0001f;
    }

    public void FinishInteraction()
    {
        Interacting = false;
        TargetSize = Vector3.one * 0.00005f;
    }

    public void Update()
    {
        if (Vector3.Distance(aimImage.transform.localScale, TargetSize) > 0.0001)
            aimImage.transform.localScale = Vector3.Slerp(
                aimImage.transform.localScale,
                TargetSize,
                0.1f
            );
        else
            aimImage.transform.localScale = TargetSize;
    }
}
