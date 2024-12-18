using UnityEngine;

public interface IPuzzle
{
    public abstract bool IsRunning();
    public abstract void StartRunning();
    public abstract void StopRunning();
    public abstract void OnExitPuzzle();
    public abstract void OnFinishPuzzle();
}
