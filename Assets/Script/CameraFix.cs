using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFix : MonoBehaviour
{
    [SerializeField]
    Vector3 targetWorldPosition;

    [SerializeField]
    Quaternion targetWorldRotation;
    Vector3 bodyWorldPosition;
    Quaternion bodyWorldRotation;

    public enum CameraState
    {
        Locking,
        Unlocking,
        Locked,
        Unlocked,
    }

    public CameraState State { get; private set; } = CameraState.Unlocked;
    CameraState stateHistory;
    MouseSystem mouseSystem;
    PlayerCamera playerCamera;
    PlayerMovement playerMovement;
    PlayerInteraction playerInteraction;
    IPuzzle thisPuzzle;

    public void Start()
    {
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        mouseSystem = FindAnyObjectByType<MouseSystem>();
        playerCamera = FindAnyObjectByType<PlayerCamera>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (GetComponent<EmotionGameManager>())
        {
            thisPuzzle = GetComponent<EmotionGameManager>();
        }
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (State == CameraState.Locked || State == CameraState.Locking)
            {
                State = CameraState.Unlocking;
            }
        }

        switch (State)
        {
            case CameraState.Locked:
                if (stateHistory != CameraState.Locked) // first frame
                { // start puzzle
                    thisPuzzle.StartRunning();
                    mouseSystem.ShowMouse();
                }
                break;

            case CameraState.Unlocked:
                if (mouseSystem.MouseVisible)
                    mouseSystem.HideMouse();
                mouseSystem.UnlockMouse();

                playerCamera.UnlockCamera();
                playerMovement.UnlockMovement();
                playerInteraction.StartRunning();

                break;

            case CameraState.Locking:
                if (stateHistory != CameraState.Locking) // first frame
                {
                    mouseSystem.HideAll();
                    playerCamera.LockCamera();
                    playerMovement.LockMovement();
                    playerInteraction.StopRunning();
                }

                Camera.main.transform.position = Vector3.Slerp(
                    Camera.main.transform.position,
                    targetWorldPosition,
                    0.05f
                );

                Camera.main.transform.rotation = Quaternion.Slerp(
                    Camera.main.transform.rotation,
                    targetWorldRotation,
                    0.05f
                );

                if (Vector3.Distance(Camera.main.transform.position, targetWorldPosition) < 0.1f)
                    State = CameraState.Locked;
                Debug.Log("CameraLocked");

                break;

            case CameraState.Unlocking:
                playerCamera.LockCamera();
                playerMovement.LockMovement();
                mouseSystem.HideAll();

                thisPuzzle.OnExitPuzzle();

                Camera.main.transform.position = Vector3.Slerp(
                    Camera.main.transform.position,
                    bodyWorldPosition,
                    0.05f
                );

                Camera.main.transform.rotation = Quaternion.Slerp(
                    Camera.main.transform.rotation,
                    bodyWorldRotation,
                    0.05f
                );

                if (Vector3.Distance(Camera.main.transform.position, bodyWorldPosition) < 0.01f)
                    State = CameraState.Unlocked;

                break;

            default:
                break;
        }
        stateHistory = State;
    }

    public void LockCamera()
    {
        if (State == CameraState.Locked || State == CameraState.Locking)
        {
            Debug.LogError("CAMERA FIX : Camera is already locked or locking");
            return;
        }

        bodyWorldPosition = Camera.main.transform.position;
        bodyWorldRotation = Camera.main.transform.rotation;

        playerCamera.LockCamera();
        playerMovement.LockMovement();
        mouseSystem.HideAll();

        State = CameraState.Locking;
    }

    public void UnlockCamera()
    {
        if (State == CameraState.Unlocked || State == CameraState.Unlocking)
        {
            Debug.LogError("CAMERA FIX : Camera is already unlocked or unlocking");
            return;
        }

        mouseSystem.HideAll();

        playerCamera.LockCamera();
        playerMovement.LockMovement();

        State = CameraState.Unlocking;
    }
}
