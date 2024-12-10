using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public bool Interacting { get; private set; } = false;
    public bool MouseVisible { get; private set; } = false;
    public bool MouseLocked { get; private set; } = false;
    Image aimImage;
    public Vector3 TargetSize { get; private set; } = Vector3.one * 0.00005f;

    public void Awake()
    {
        aimImage = GetComponent<Image>();
        HideMouse();
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

    public void ShowMouse()
    {
        Cursor.visible = true;
        MouseVisible = true;

        aimImage.color = Color.clear;
        Debug.Log("MOUSE - ShowMouse()");
    }

    public void HideMouse()
    {
        Cursor.visible = false;
        MouseVisible = false;

        aimImage.color = new Color(1, 1, 1, 0.5f);
        Debug.Log("MOUSE - HideMouse()");
    }

    public void HideAll()
    {
        Cursor.visible = false;
        MouseVisible = false;
        aimImage.color = Color.clear;
        Debug.Log("MOUSE - HideAll()");
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Confined;
        MouseLocked = true;
    }

    public void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        MouseLocked = false;
    }
}
