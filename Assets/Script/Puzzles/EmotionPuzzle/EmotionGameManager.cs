using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmotionGameManager : MonoBehaviour
{
    public bool Running { get; private set; } = false;
    bool LevelCompleted = false;

    [SerializeField]
    CapsuleCollider[] PlayerPieces;

    [SerializeField]
    CapsuleCollider[] MiddlePieces;

    [SerializeField]
    CapsuleCollider FinalPiece;

    [SerializeField]
    public CapsuleCollider[] PickPieces;

    [SerializeField]
    CapsuleCollider HoldingPiece;

    [SerializeField]
    CapsuleCollider IndicatorPiece;
    Color overlayColor;
    Image IndicatorCoverPiece;

    [SerializeField]
    float MIN_COLOR_DIFFERENCE;
    static Color[] LevelColors = new Color[4]
    {
        new Color(0.870f, 0.432f, 0.167f, 1.000f), // orange
        new Color(0.793f, 0.374f, 0.793f, 1.000f), // purple
        new Color(0.490f, 0.000f, 0.490f, 1.000f), // magenta
        new Color(0.350f, 0.805f, 0.805f, 1.000f), // cian
    };

    Color GetMaterialColor(GameObject obj)
    {
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            Debug.LogError("EMOTIONS PUZZLE : Object doesn't has a MeshRenderer");
            return Color.clear;
        }
        if (meshRenderer.material == null)
        {
            Debug.LogError("EMOTIONS PUZZLE : Object doesn't has a Material");
            return Color.clear;
        }
        return meshRenderer.material.GetColor("_Color");
    }

    public void StartRunning() => Running = true;

    public void StopRunning() => Running = false;
}
