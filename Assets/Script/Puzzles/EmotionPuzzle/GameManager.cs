using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    bool LevelCompleted = false;

    [SerializeField]
    Image[] PlayerPieces;

    [SerializeField]
    Image[] MiddlePieces;

    [SerializeField]
    Image FinalPiece;

    [SerializeField]
    public Image[] PickPieces;

    [SerializeField]
    Image HoldingPiece;

    [SerializeField]
    Image IndicatorPiece;

    [SerializeField]
    Image Overlay;
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
    Color TargetColor = LevelColors[0];
    float TimeDebugger = 0.5f;
    private float colorSum;

    bool Updated = false;
    Color holdingColor = Color.white;
    Image holdingPiece;
    bool started = true;

    void Start()
    {
        for (int i = 0; i < LevelColors.Length; i++)
        {
            LevelColors[i] = VectorToColor(
                NormalizeColorVector(ColorToVector(LevelColors[i]))
                    * GetColorSaturation(LevelColors[i])
            );
        }
        IndicatorPiece.color = LevelColors[0];
        TargetColor = LevelColors[0];
        StartCoroutine(RestartMapBoard(0));

        IndicatorCoverPiece = IndicatorPiece.transform.parent.GetComponentInChildren<Image>();
        overlayColor = Overlay.color;

        StartCoroutine(StartRunning(2, 100));
        //HoldingPiece.enabled = false;
    }

    private void Update()
    {
        if (LevelCompleted)
            return;
        TimeDebugger = Mathf.Clamp(TimeDebugger - Time.deltaTime, 0, 0.5f);
        if (Input.GetMouseButtonDown(0))
        {
            if (holdingColor == Color.white)
            {
                foreach (Image image in PickPieces)
                {
                    float distance = Vector3.Distance(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition),
                        image.transform.parent.position
                    );

                    if (distance <= 11f)
                    {
                        holdingColor = image.color;
                        HoldingPiece.color = holdingColor;
                        Vector3 vector = new Vector3(
                            Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                            Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
                            -1
                        );
                        HoldingPiece.transform.parent.position = vector;
                    }
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (holdingColor != Color.white)
            {
                Vector3 vector = new Vector3(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                    Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
                    -1
                );
                HoldingPiece.transform.parent.position = vector;

                foreach (Image image in PlayerPieces)
                {
                    float distance = Vector3.Distance(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition),
                        image.transform.parent.position
                    );

                    if (distance <= 11f)
                    {
                        HoldingPiece.transform.parent.position = image.transform.parent.position;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (TimeDebugger > 0)
                return;
            if (holdingColor != Color.white)
            {
                for (int i = 0; i < PlayerPieces.Length; i++)
                {
                    float distance = Vector3.Distance(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition),
                        PlayerPieces[i].transform.parent.position
                    );
                    if (distance <= 11f)
                    {
                        PlayerPieces[i].color = holdingColor;
                        UpdateGraph();
                    }
                }
                holdingColor = Color.white;

                HoldingPiece.color = Color.white;
                HoldingPiece.transform.parent.position = new Vector3(10000, 10000, 0);
            }
            else
            {
                for (int i = 0; i < PlayerPieces.Length; i++)
                {
                    float distance = Vector3.Distance(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition),
                        PlayerPieces[i].transform.parent.position
                    );
                    if (distance <= 11f)
                    {
                        PlayerPieces[i].color = Color.white;
                        UpdateGraph();
                    }
                }
            }
        }
    }

    private Color GetColorDifference(Color c1, Color c2) =>
        VectorToColor(Vector3Abs(ColorToVector(c1) - ColorToVector(c2)));

    private IEnumerator RestartMapBoard(float timeMultiplier, bool showIndicator = true)
    {
        Color middleColor = new Color(1, 1, 1, 0.1f);
        Color centralColor = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSeconds(1 * timeMultiplier);
        foreach (Image image in PlayerPieces)
        {
            image.color = Color.white;
        }
        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[0].color = middleColor;
        MiddlePieces[1].color = middleColor;

        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[2].color = middleColor;
        MiddlePieces[5].color = middleColor;

        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[4].color = middleColor;
        MiddlePieces[3].color = middleColor;

        yield return new WaitForSeconds(0.1f * timeMultiplier);

        Color deltaColor = VectorToColor(
            ColorToVector(
                GetColorDifference(
                    VectorToColor(NormalizeColorVector(new Vector3(70, 70, 70))),
                    Color.white
                )
            ) / 50
        );
        FinalPiece.color = centralColor;

        if (showIndicator)
        {
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.0005f); // 2secs
                if (i >= 50) //down
                {
                    IndicatorCoverPiece.color = VectorToColor(
                        ColorToVector(IndicatorCoverPiece.color - deltaColor)
                    );
                }
                else // up
                {
                    IndicatorCoverPiece.color = VectorToColor(
                        ColorToVector(IndicatorCoverPiece.color + deltaColor)
                    );
                }
            }
        }
    }

    private void UpdateGraph()
    {
        TimeDebugger = 0.5f;
        float playerPiecesSum = 0;
        foreach (Image image in PlayerPieces)
        {
            playerPiecesSum += VectorSum(ColorToVector(image.color));
        }

        if (playerPiecesSum == 3 * 4)
        {
            StartCoroutine(RestartMapBoard(0));
            return;
        }

        UpdateMiddlePieces();

        Color[] colors = new Color[MiddlePieces.Length];
        for (int i = 0; i < MiddlePieces.Length; i++)
        {
            colors[i] = MiddlePieces[i].color;
        }
        FinalPiece.color = GetFinalColor(colors);
        Debug.Log(
            "Difference: "
                + VectorSum(ColorToVector(GetColorDifference(FinalPiece.color, TargetColor)))
        );

        VerifyResult();
    }

    private Color GetFinalColor(Color[] colors)
    {
        Vector3 newColor = new Vector3(0, 0, 0);

        int whitePieces = 0;

        foreach (var color in colors)
        {
            if (IsColorWhite(color))
                whitePieces++;
            else
                newColor += new Vector3(color.r, color.g, color.b);
        }
        newColor /= colors.Length;

        if (whitePieces > 0)
        {
            newColor = ColorToVector(
                ApplyColorSaturation(
                    VectorToColor(newColor),
                    whitePieces * GetColorSaturation(Color.white)
                )
            );
        }

        return new Color(newColor.x, newColor.y, newColor.z, 1f);
    }

    private Color AddColors(Color color1, Color color2, float importance1, float importance2) // use only 3 colors
    {
        Vector3 newColor = new Vector3(0, 0, 0);

        Vector3 v1 = new Vector3(color1.r, color1.g, color1.b);
        Vector3 v2 = new Vector3(color2.r, color2.g, color2.b);

        Vector3 v1n = NormalizeColorVector(v1) * GetColorSaturation(VectorToColor(v1));
        Vector3 v2n = NormalizeColorVector(v2) * GetColorSaturation(VectorToColor(v2));

        
        Vector3 fv;


        v1n = ColorToVector(
            ApplyColorSaturation(
                VectorToColor(v1),
                GetColorSaturation(VectorToColor(v1)) * importance1
                    + GetColorSaturation(VectorToColor(v2) * importance2)
            )
        );
        v2n = ColorToVector(
            ApplyColorSaturation(
                VectorToColor(v2),
                GetColorSaturation(VectorToColor(v2)) * importance1
                    + GetColorSaturation(VectorToColor(v1) * importance2)
            )
        );

        fv = v1n * importance1 + v2n * importance2;

        return new Color(fv.x, fv.y, fv.z, 1f);
    }

    private Vector3 Vector3Abs(Vector3 vector3)
    {
        return new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
    }

    private void UpdateMiddlePieces()
    {
        MiddlePieces[0].color = AddColors(PlayerPieces[0].color, PlayerPieces[2].color, 0.5f, 0.5f);
        MiddlePieces[1].color = AddColors(PlayerPieces[1].color, PlayerPieces[3].color, 0.5f, 0.5f);

        MiddlePieces[2].color = AddColors(
            PlayerPieces[2].color,
            PlayerPieces[3].color,
            0.66f,
            0.33f
        );
        MiddlePieces[2].color = AddColors(
            MiddlePieces[2].color,
            MiddlePieces[0].color,
            0.66f,
            0.33f
        );
        MiddlePieces[3].color = AddColors(
            PlayerPieces[3].color,
            PlayerPieces[2].color,
            0.66f,
            0.33f
        );
        MiddlePieces[3].color = AddColors(
            MiddlePieces[3].color,
            MiddlePieces[1].color,
            0.66f,
            0.33f
        );

        MiddlePieces[4].color = AddColors(
            PlayerPieces[0].color,
            PlayerPieces[1].color,
            0.66f,
            0.33f
        );
        MiddlePieces[4].color = AddColors(
            MiddlePieces[4].color,
            MiddlePieces[0].color,
            0.66f,
            0.33f
        );
        MiddlePieces[5].color = AddColors(
            PlayerPieces[1].color,
            PlayerPieces[0].color,
            0.66f,
            0.33f
        );
        MiddlePieces[5].color = AddColors(
            MiddlePieces[5].color,
            MiddlePieces[1].color,
            0.66f,
            0.33f
        );

        Updated = true;
    }

    private float GetColorSaturation(Color color) => color.r + color.g + color.b;

    //white = 3
    //RGB = 1
    //Black = 0

    private Color VectorToColor(Vector3 vector3) => new Color(vector3.x, vector3.y, vector3.z, 1f);

    private Vector3 ColorToVector(Color color) => new Vector3(color.r, color.g, color.b);

    private bool IsUpdated()
    {
        float cSum = 0;
        for (int i = 0; i < PlayerPieces.Length; i++)
        {
            cSum += PlayerPieces[i].color.r + PlayerPieces[i].color.g + PlayerPieces[i].color.b;
        }

        if (cSum == colorSum)
            return true;
        else
        {
            colorSum = cSum;
            return false;
        }
    }

    private Vector3 NormalizeColorVector(Vector3 vector3)
    {
        float vectorSum = vector3.x + vector3.y + vector3.z;
        Vector3 result = new Vector3(
            vector3.x / vectorSum,
            vector3.y / vectorSum,
            vector3.z / vectorSum
        );
        return result;
    }

    private Color ApplyColorSaturation(Color color, float saturation) //saturation (0-3)
    {
        return VectorToColor(NormalizeColorVector(ColorToVector(color)) * saturation);
    }

    private bool IsColorWhite(Color color) => color == Color.white;

    private float VectorSum(Vector3 vector3) => vector3.x + vector3.y + vector3.z;

    private float CompareColors(Color c1, Color c2)
    {
        Vector3 result = new Vector3(
            Mathf.Abs(c1.r - c1.r),
            Mathf.Abs(c1.g - c2.g),
            Mathf.Abs(c1.b - c2.b)
        );
        return VectorSum(result);
    }

    private int GetCurrentColorLevelIndex(Color color)
    {
        int index = 0;
        foreach (Color c in LevelColors)
        {
            if (c == color)
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    public void VerifyResult()
    {
        float cDifference = CompareColors(FinalPiece.color, TargetColor);
        Debug.Log("Difference: " + cDifference);
        if (cDifference <= MIN_COLOR_DIFFERENCE)
        { // correct color
            if (GetCurrentColorLevelIndex(TargetColor) + 1 < LevelColors.Length) // Next Color
            {
                IndicatorPiece.color = LevelColors[GetCurrentColorLevelIndex(TargetColor) + 1];
                TargetColor = IndicatorPiece.color;
                StartCoroutine(RestartMapBoard(1));
            }
            else // Puzzle Finished
            {
                IndicatorPiece.color = Color.white;
                StartCoroutine(RestartMapBoard(1, false));
                StartCoroutine(StopRunning(2, 100));
            }
        }
        return;
    }

    IEnumerator StartRunning(float time, int divisions)
    {
        Overlay.enabled = true;
        Overlay.color = overlayColor;
        float deltaAlpha = (float)time / divisions;

        for (int i = 0; i < divisions; i++)
        {
            yield return new WaitForSeconds(time / divisions);

            Overlay.color = new Color(
                Overlay.color.r,
                Overlay.color.g,
                Overlay.color.b,
                (divisions - i) * deltaAlpha
            );
        }
        Overlay.enabled = false;
        LevelCompleted = false;
    }

    IEnumerator StopRunning(float time, int divisions)
    {
        yield return new WaitForSeconds(2);
        LevelCompleted = true;
        Overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        float deltaAlpha = (float)time / divisions;
        Overlay.enabled = true;

        for (int i = 0; i < divisions; i++)
        {
            yield return new WaitForSeconds(time / divisions);

            Overlay.color = new Color(
                Overlay.color.r,
                Overlay.color.g,
                Overlay.color.b,
                i * deltaAlpha
            );
        }

        yield return new WaitForSeconds(3);
        //Application.Quit();

        SceneChanger sceneManager = FindObjectOfType<SceneChanger>();
        sceneManager.LoadSceneByName("MainScene");
    }
}
