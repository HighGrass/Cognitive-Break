using UnityEngine;

public class NeuronPuzzle : MonoBehaviour, IPuzzle
{
    [SerializeField]
    Color currentColor;
    bool Running = false;
    int gameState = 0; // ( começar (1) , ignorar (0) , terminar(-1) )
    float colorChange = 0.33f; //3s

    public bool IsRunning() => Running;

    private void Update()
    {
        if (!IsRunning())
            return;
    }

    public void StartLevel() => Running = true;

    public void FinishLevel() => Running = false;

    public void OnExitPuzzle() { }

    public void StartRunning()
    {
        gameState = 1;
    }

    public void StopRunning()
    {
        gameState = -1;
    }
}
