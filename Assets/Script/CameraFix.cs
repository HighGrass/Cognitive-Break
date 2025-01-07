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
    public bool IsActive { get; private set; } = true;

    public void SetActivity(bool activity) => IsActive = activity;

    public enum CameraState
    {
        Locking,
        Unlocking,
        Locked,
        Unlocked,
    }

    public CameraState State { get; private set; } = CameraState.Unlocked;
    MouseSystem mouseSystem;
    PlayerCamera playerCamera;
    PlayerMovement playerMovement;
    PlayerInteraction playerInteraction;
    IPuzzle thisPuzzle;

    bool stateHistoryUpdated = false;

    public void Start()
    {
        playerInteraction = FindObjectOfType<PlayerInteraction>();
        mouseSystem = FindObjectOfType<MouseSystem>();
        playerCamera = FindObjectOfType<PlayerCamera>();
        playerMovement = FindObjectOfType<PlayerMovement>();

        if (GetComponent<EmotionGameManager>())
        {
            thisPuzzle = GetComponent<EmotionGameManager>();
        }
        else if (GetComponent<NeuronPuzzle>())
        {
            thisPuzzle = GetComponent<NeuronPuzzle>();
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("State: " + State);
            if (State == CameraState.Locked || State == CameraState.Locking)
            {
                SetState(CameraState.Unlocking);
            }
        }

        switch (State)
        {
            case CameraState.Locked:
                if (stateHistoryUpdated)
                    break;

                // start puzzle
                thisPuzzle.StartRunning();
                mouseSystem.ShowMouse();
                stateHistoryUpdated = true;

                break;

            case CameraState.Unlocked:
                if (stateHistoryUpdated)
                    break;

                mouseSystem.HideMouse();
                mouseSystem.UnlockMouse();

                playerCamera.UnlockCamera();
                playerMovement.UnlockMovement();
                playerInteraction.StartRunning();
                stateHistoryUpdated = true;

                break;

            case CameraState.Locking:
                if (!stateHistoryUpdated) // first frame
                {
                    mouseSystem.HideAll();
                    playerCamera.LockCamera();
                    playerMovement.LockMovement();
                    playerInteraction.StopRunning();
                    stateHistoryUpdated = true;
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
                {
                    SetState(CameraState.Locked);
                    Debug.LogWarning("Locked");
                }

                break;

            case CameraState.Unlocking:
                if (!stateHistoryUpdated)
                {
                    playerCamera.LockCamera();
                    playerMovement.LockMovement();
                    mouseSystem.HideAll();

                    thisPuzzle.OnExitPuzzle();
                    stateHistoryUpdated = true;
                }

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
                {
                    SetState(CameraState.Unlocked);
                    Debug.LogWarning("Unlocked");
                }

                break;

            default:
                break;
        }
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

        SetState(CameraState.Locking);
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

        SetState(CameraState.Unlocking);
    }

    void SetState(CameraState state)
    {
        State = state;
        stateHistoryUpdated = false;
    }
}
