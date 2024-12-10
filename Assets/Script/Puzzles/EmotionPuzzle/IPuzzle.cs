using UnityEngine;

public interface IPuzzle
{
    public abstract void OnExitPuzzle();
    public abstract void StartRunning();

    public abstract void StopRunning();
}
