using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmotionGameManager : MonoBehaviour, IPuzzle
{
    public bool Running { get; private set; } = false;
    public bool PuzzleStarted { get; private set; } = false;
    bool LevelCompleted = false;
    bool piecesActive;
    Dictionary<MeshRenderer, Coroutine> movingPieces = new Dictionary<MeshRenderer, Coroutine>();
    int CurrentColor { get; } = 0;

    public bool IsRunning() => Running;

    [SerializeField]
    Image colorPicker;

    [SerializeField]
    MeshRenderer[] PlayerPieces;

    [SerializeField]
    MeshRenderer[] MiddlePieces;

    [SerializeField]
    MeshRenderer FinalPiece;

    [SerializeField]
    public MeshRenderer[] PickPieces;

    [SerializeField]
    MeshRenderer IndicatorPiece;

    MeshRenderer IndicatorCoverPiece;

    [SerializeField]
    float MIN_COLOR_DIFFERENCE = 0.001f;

    GameObject playerPieceHistory;

    static Color[] LevelColors = new Color[4]
    {
        new Color(0.350f, 0.805f, 0.805f, 1.000f), // cian - felicidade
        new Color(0.793f, 0.374f, 0.793f, 1.000f), // purple - tristeza
        new Color(0.870f, 0.432f, 0.167f, 1.000f), // orange - raiva
        new Color(0.490f, 0.000f, 0.490f, 1.000f), // magenta - medo
    };

    string[] ColorSpeeches = new string[4]
    {
        "Start by creating a <color="
            + ColorUtils.ColorToHex(LevelColors[0])
            + ">happy</color> color",
        "Now, try a <color=" + ColorUtils.ColorToHex(LevelColors[1]) + ">sadder</color> one",
        "How think of a color that reminds you of  <color="
            + ColorUtils.ColorToHex(LevelColors[2])
            + ">anger</color>",
        "How about a <color=" + ColorUtils.ColorToHex(LevelColors[3]) + ">scary</color> one?",
    };
    Dictionary<MeshRenderer, float> PiecesScale = new Dictionary<MeshRenderer, float>();

    void Cache_CreatePiecesScale()
    {
        List<MeshRenderer> allPieces = GetAllPieces(PieceOrderType.none);
        foreach (MeshRenderer render in allPieces)
        {
            PiecesScale.Add(render, render.gameObject.transform.localScale.z);
        }
    }

    void Cache_UpdatePieceScale(MeshRenderer piece, float newSize) => PiecesScale[piece] = newSize;

    Color TargetColor = LevelColors[0];
    float TimeDebugger = 0.5f;
    private float colorSum;
    public Color HoldingColor { get; private set; } = Color.white;

    [SerializeField]
    LayerMask layerMask;
    PlayerInteraction playerInteraction;
    MouseSystem mouseSystem;
    Narrator narrator;

    void Start()
    {
        narrator = FindAnyObjectByType<Narrator>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        mouseSystem = FindAnyObjectByType<MouseSystem>();

        for (int i = 0; i < LevelColors.Length; i++)
        {
            LevelColors[i] = VectorToColor(
                NormalizeColorVector(ColorToVector(LevelColors[i]))
                    * GetColorSaturation(LevelColors[i])
            );
        }
        IndicatorPiece.material.SetColor("_Color", LevelColors[0]);
        TargetColor = LevelColors[0];
        StartCoroutine(RestartMapBoard(0));

        IndicatorCoverPiece =
            IndicatorPiece.transform.parent.GetComponentInChildren<MeshRenderer>();

        Cache_CreatePiecesScale();
        //HoldingPiece.enabled = false;
    }

    void Update()
    {
        if (LevelCompleted)
            return;

        if (Running && !PuzzleStarted) // Start puzzle
        {
            StartCoroutine(StartLevel());
            PuzzleStarted = true;
        }
        else if (!Running)
            return;

        TimeDebugger = Mathf.Clamp(TimeDebugger - Time.deltaTime, 0, 0.5f);

        // --------- Raycast ---------

        float raycastDistance = 10;
        Vector3 mousePos = Input.mousePosition;

        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        Vector3 worldPoint = mouseRay.GetPoint(raycastDistance);

        Vector3 direction = (worldPoint - Camera.main.transform.position).normalized;

        // --------- ------- ---------


        MeshRenderer renderHit = null;

        RaycastHit hit;
        if (
            Physics.Raycast(
                Camera.main.transform.position,
                direction,
                out hit,
                raycastDistance,
                layerMask
            )
        )
        {
            renderHit = hit.collider.gameObject.GetComponent<MeshRenderer>();
            Debug.DrawRay(Camera.main.transform.position, direction * hit.distance, Color.yellow);
        }
        else
        {
            Debug.DrawRay(Camera.main.transform.position, direction * raycastDistance, Color.white);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (renderHit)
            {
                MeshRenderer render = hit.collider.gameObject.GetComponent<MeshRenderer>();

                if (PickPieces.Contains(render))
                {
                    HoldingColor = render.material.GetColor("_Color");
                    mouseSystem.HideAll();
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (HoldingColor != Color.white)
            {
                colorPicker.gameObject.transform.position = worldPoint - direction * 9;
                colorPicker.color = HoldingColor;
            }

            if (renderHit)
            { // piece animation
                if (PlayerPieces.Contains(renderHit)) // is a Player Piece
                {
                    if (renderHit.gameObject != playerPieceHistory)
                    {
                        if (playerPieceHistory != null)
                        {
                            MeshRenderer render = playerPieceHistory.GetComponent<MeshRenderer>();
                            ScalePiece(render, PiecesScale[render], 10, false);
                        }
                        MeshRenderer render2 = renderHit.gameObject.GetComponent<MeshRenderer>();
                        ScalePiece(render2, PiecesScale[render2] - 30, 10, false);
                        playerPieceHistory = renderHit.gameObject;
                    } // is new
                }
            }
            else if (playerPieceHistory != null)
            {
                MeshRenderer render = playerPieceHistory.GetComponent<MeshRenderer>();
                ScalePiece(render, PiecesScale[render], 10);
                playerPieceHistory = null;
            }
        }
        else if (playerPieceHistory != null)
        {
            MeshRenderer render = playerPieceHistory.GetComponent<MeshRenderer>();
            ScalePiece(render, PiecesScale[render], 10);
            playerPieceHistory = null;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseSystem.ShowMouse();
            colorPicker.gameObject.transform.position = -Vector3.one * 1000;
            if (TimeDebugger > 0)
                return;

            if (PlayerPieces.Contains(renderHit))
            {
                renderHit.material.SetColor("_Color", HoldingColor);
                HoldingColor = Color.white;
                UpdateGraph();
                StartCoroutine(AnimateAllPieces(renderHit));
            }
            else
                HoldingColor = Color.white;
        }
    }

    private Color GetColorDifference(Color c1, Color c2) =>
        VectorToColor(Vector3Abs(ColorToVector(c1) - ColorToVector(c2)));

    private IEnumerator RestartMapBoard(float timeMultiplier, bool showIndicator = true)
    {
        Color middleColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        Color centralColor = new Color(0.5f, 0.5f, 0.5f, 1);

        yield return new WaitForSeconds(1 * timeMultiplier);
        foreach (MeshRenderer render in PlayerPieces)
        {
            render.material.SetColor("_Color", Color.white);
        }
        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[0].material.SetColor("_Color", middleColor);
        MiddlePieces[1].material.SetColor("_Color", middleColor);

        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[2].material.SetColor("_Color", middleColor);
        MiddlePieces[5].material.SetColor("_Color", middleColor);

        yield return new WaitForSeconds(0.1f * timeMultiplier);
        MiddlePieces[4].material.SetColor("_Color", middleColor);
        MiddlePieces[3].material.SetColor("_Color", middleColor);

        yield return new WaitForSeconds(0.1f * timeMultiplier);

        Color deltaColor = VectorToColor(
            ColorToVector(
                GetColorDifference(
                    VectorToColor(NormalizeColorVector(new Vector3(70, 70, 70))),
                    Color.white
                )
            ) / 50
        );
        FinalPiece.material.SetColor("_Color", centralColor);

        if (showIndicator)
        {
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.0005f); // 2secs
                if (i >= 50) //down
                {
                    IndicatorCoverPiece.material.SetColor(
                        "_Color",
                        VectorToColor(
                            ColorToVector(
                                IndicatorCoverPiece.material.GetColor("_Color") - deltaColor
                            )
                        )
                    );
                }
                else // up
                {
                    IndicatorCoverPiece.material.SetColor(
                        "_Color",
                        VectorToColor(
                            ColorToVector(
                                IndicatorCoverPiece.material.GetColor("_Color") + deltaColor
                            )
                        )
                    );
                }
            }
        }
    }

    private void UpdateGraph()
    {
        TimeDebugger = 0.5f;
        float playerPiecesSum = 0;
        foreach (MeshRenderer MeshRenderer in PlayerPieces)
        {
            playerPiecesSum += VectorSum(ColorToVector(MeshRenderer.material.GetColor("_Color")));
        }

        if (playerPiecesSum == 3 * 4) // all white
        {
            StartCoroutine(RestartMapBoard(0));
            return;
        }

        UpdateMiddlePieces();

        Color[] colors = new Color[MiddlePieces.Length];
        for (int i = 0; i < MiddlePieces.Length; i++)
        {
            colors[i] = MiddlePieces[i].material.GetColor("_Color");
        }
        FinalPiece.material.SetColor("_Color", GetFinalColor(colors));
        /*
        Debug.Log(
            "Difference: "
                + VectorSum(
                    ColorToVector(
                        GetColorDifference(FinalPiece.material.GetColor("_Color"), TargetColor)
                    )
                )
        );
        */
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
        MiddlePieces[0]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[0].material.GetColor("_Color"),
                    PlayerPieces[2].material.GetColor("_Color"),
                    0.5f,
                    0.5f
                )
            );
        MiddlePieces[1]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[1].material.GetColor("_Color"),
                    PlayerPieces[3].material.GetColor("_Color"),
                    0.5f,
                    0.5f
                )
            );

        MiddlePieces[2]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[2].material.GetColor("_Color"),
                    PlayerPieces[3].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[2]
            .material.SetColor(
                "_Color",
                AddColors(
                    MiddlePieces[2].material.GetColor("_Color"),
                    MiddlePieces[0].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[3]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[3].material.GetColor("_Color"),
                    PlayerPieces[2].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[3]
            .material.SetColor(
                "_Color",
                AddColors(
                    MiddlePieces[3].material.GetColor("_Color"),
                    MiddlePieces[1].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );

        MiddlePieces[4]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[0].material.GetColor("_Color"),
                    PlayerPieces[1].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[4]
            .material.SetColor(
                "_Color",
                AddColors(
                    MiddlePieces[4].material.GetColor("_Color"),
                    MiddlePieces[0].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[5]
            .material.SetColor(
                "_Color",
                AddColors(
                    PlayerPieces[1].material.GetColor("_Color"),
                    PlayerPieces[0].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
        MiddlePieces[5]
            .material.SetColor(
                "_Color",
                AddColors(
                    MiddlePieces[5].material.GetColor("_Color"),
                    MiddlePieces[1].material.GetColor("_Color"),
                    0.66f,
                    0.33f
                )
            );
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
            cSum +=
                PlayerPieces[i].material.GetColor("_Color").r
                + PlayerPieces[i].material.GetColor("_Color").g
                + PlayerPieces[i].material.GetColor("_Color").b;
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
        float cDifference = CompareColors(FinalPiece.material.GetColor("_Color"), TargetColor);

        if (cDifference <= MIN_COLOR_DIFFERENCE)
        { // correct color
            if (GetCurrentColorLevelIndex(TargetColor) + 1 < LevelColors.Length) // Next Color
            {
                IndicatorPiece.material.SetColor(
                    "_Color",
                    LevelColors[GetCurrentColorLevelIndex(TargetColor) + 1]
                );
                TargetColor = IndicatorPiece.material.GetColor("_Color");
                StartCoroutine(RestartMapBoard(1));
            }
            else // Puzzle Finished
            {
                IndicatorPiece.material.SetColor("_Color", Color.white);
                StartCoroutine(RestartMapBoard(1, false));
                StartCoroutine(FinishLevel());
            }
        }
        return;
    }

    IEnumerator StartLevel()
    {
        LevelCompleted = false;
        Debug.Log("Emotion puzzle started");
        string firsttext = "Lets see if you can describe your fellings!";
        narrator.Say(firsttext, Narrator.FraseType.None, 1.5f);
        narrator.Say(ColorSpeeches[CurrentColor], Narrator.FraseType.Wait);

        yield break;
    }

    IEnumerator FinishLevel()
    {
        LevelCompleted = true;

        yield return new WaitForSeconds(1);
        //Application.Quit();

        StopRunning();
    }

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

    public void OnExitPuzzle() => StopRunning();

    public void StartRunning()
    {
        ActivatePieces();
        mouseSystem.ShowMouse();
        Running = true;

        narrator.SkipQueue();
        if (PuzzleStarted)
            narrator.Say(ColorSpeeches[CurrentColor], Narrator.FraseType.Wait, 1.5f);
    }

    public void StopRunning()
    {
        Running = false;
        playerInteraction.StartRunning();
        mouseSystem.HideMouse();
        DeactivatePieces();
        narrator.SkipQueue();
        narrator.ClearText();

        HoldingColor = Color.white;
        colorPicker.gameObject.transform.position = Vector3.one * 10000;
    }

    void ActivatePieces()
    {
        if (piecesActive)
            return;
        piecesActive = true;
        foreach (MeshRenderer render in PlayerPieces)
        {
            ScalePiece(render, 200);
        }

        foreach (MeshRenderer render in MiddlePieces)
        {
            ScalePiece(render, 125);
        }

        foreach (MeshRenderer render in PickPieces)
        {
            ScalePiece(render, 50);
        }
        ScalePiece(FinalPiece.GetComponent<MeshRenderer>(), 100f);
    }

    void DeactivatePieces()
    {
        if (!piecesActive)
            return;
        piecesActive = false;
        foreach (MeshRenderer render in PlayerPieces)
        {
            ScalePiece(render, 3);
        }

        foreach (MeshRenderer render in MiddlePieces)
        {
            ScalePiece(render, 3);
        }

        foreach (MeshRenderer render in PickPieces)
        {
            ScalePiece(render, 3);
        }
        ScalePiece(FinalPiece.GetComponent<MeshRenderer>(), 3);
    }

    IEnumerator ScalePieceCoroutine(MeshRenderer piece, float finalSize, float speed)
    {
        while (Mathf.Abs(piece.gameObject.transform.localScale.z - finalSize) > 0.1f)
        {
            piece.gameObject.transform.localScale =
                piece.gameObject.transform.localScale
                + new Vector3(
                    0,
                    0,
                    (finalSize - piece.gameObject.transform.localScale.z) * speed * Time.deltaTime
                );
            yield return null;
        }
        yield break;
    }

    void ScalePiece(
        MeshRenderer piece,
        float finalSize = 50f,
        float speed = 1f,
        bool updateVar = true
    )
    {
        foreach (KeyValuePair<MeshRenderer, Coroutine> pieceInfo in movingPieces)
        {
            if (pieceInfo.Key == piece)
            {
                try
                {
                    StopCoroutine(pieceInfo.Value);
                }
                catch { }
                movingPieces.Remove(pieceInfo.Key);

                break;
            }
        }

        Coroutine newCoroutine = StartCoroutine(ScalePieceCoroutine(piece, finalSize, speed));
        movingPieces.Add(piece, newCoroutine);

        if (updateVar)
            Cache_UpdatePieceScale(piece, finalSize);

        return;
    }

    enum PieceOrderType
    {
        none,
        basic,
        interaction,
    };

    float[,] interactionPieceOrder = new float[3, 4]
    {
        { 3, 2, 2, 3 },
        { 2, 1, 2, 0 },
        { 3, 2, 2, 3 },
    };

    List<MeshRenderer> GetAllPieces(PieceOrderType orderType, int playerPieceIndex = 0)
    {
        List<MeshRenderer> pieces = new List<MeshRenderer>();

        switch (orderType)
        {
            case PieceOrderType.none:
                foreach (MeshRenderer render in PlayerPieces)
                {
                    pieces.Add(render);
                }
                foreach (MeshRenderer render in MiddlePieces)
                {
                    pieces.Add(render);
                }
                pieces.Add(FinalPiece);

                break;

            case PieceOrderType.basic:
                pieces.Add(PlayerPieces[0]);
                pieces.Add(MiddlePieces[0]);
                pieces.Add(PlayerPieces[3]);

                pieces.Add(MiddlePieces[2]);
                pieces.Add(FinalPiece);
                pieces.Add(MiddlePieces[4]);

                pieces.Add(MiddlePieces[3]);
                pieces.Add(MiddlePieces[1]);
                pieces.Add(MiddlePieces[5]);

                pieces.Add(PlayerPieces[1]);
                pieces.Add(null); // doesnt exist
                pieces.Add(PlayerPieces[2]);
                break;

            case PieceOrderType.interaction:
                switch (playerPieceIndex)
                {
                    case 0:
                        pieces.Add(MiddlePieces[0]);
                        pieces.Add(MiddlePieces[4]);
                        pieces.Add(FinalPiece);
                        pieces.Add(MiddlePieces[2]);
                        pieces.Add(MiddlePieces[5]);
                        pieces.Add(MiddlePieces[3]);
                        pieces.Add(MiddlePieces[1]);

                        break;

                    case 1:

                        pieces.Add(MiddlePieces[1]);
                        pieces.Add(MiddlePieces[5]);
                        pieces.Add(FinalPiece);
                        pieces.Add(MiddlePieces[3]);
                        pieces.Add(MiddlePieces[4]);
                        pieces.Add(MiddlePieces[2]);
                        pieces.Add(MiddlePieces[0]);
                        break;

                    case 2:

                        pieces.Add(MiddlePieces[0]);
                        pieces.Add(MiddlePieces[2]);
                        pieces.Add(FinalPiece);
                        pieces.Add(MiddlePieces[4]);
                        pieces.Add(MiddlePieces[3]);
                        pieces.Add(MiddlePieces[5]);
                        pieces.Add(MiddlePieces[1]);

                        break;

                    case 3:

                        pieces.Add(MiddlePieces[1]);
                        pieces.Add(MiddlePieces[3]);
                        pieces.Add(FinalPiece);
                        pieces.Add(MiddlePieces[5]);
                        pieces.Add(MiddlePieces[2]);
                        pieces.Add(MiddlePieces[4]);
                        pieces.Add(MiddlePieces[0]);

                        break;
                }

                break;

            default:
                Debug.LogError("Invalid PieceOrderType");
                break;
        }

        return pieces;
    }

    IEnumerator AnimateAllPieces(MeshRenderer interactionPiece)
    {
        if (!PlayerPieces.Contains(interactionPiece))
        {
            Debug.LogError("Invalid Player piece");
            yield break;
        }

        int playerPieceIndex = 0;
        foreach (MeshRenderer render in PlayerPieces)
        {
            if (interactionPiece == render)
                break;

            playerPieceIndex++;
        }

        List<MeshRenderer> allPieces = GetAllPieces(PieceOrderType.interaction, playerPieceIndex);

        WaitForSeconds basicDelay = new WaitForSeconds(0.03f);

        foreach (MeshRenderer render in allPieces)
        {
            ScalePiece(render, 30, 20, false);
            yield return basicDelay;
        }

        foreach (MeshRenderer render in allPieces)
        {
            ScalePiece(render, PiecesScale[render], 20, false);
            yield return basicDelay;
        }

        yield break;
    }

    enum DirectionOption
    {
        None,
        Up,
        Down,
        Right,
        Left,
    }

    Vector2 DirectionValue(DirectionOption directionOption)
    {
        switch (directionOption)
        {
            case DirectionOption.Up:
                return Vector2.up;

            case DirectionOption.Down:
                return Vector2.down;

            case DirectionOption.Right:
                return Vector2.right;

            case DirectionOption.Left:
                return Vector2.left;

            default:
                return Vector2.zero;
        }
    }

    MeshRenderer NeighborPiece(
        int index,
        DirectionOption directionOption,
        MeshRenderer[] boardPieces
    )
    {
        Vector2 direction = DirectionValue(directionOption);
        (int row, int column) limits = (3, 4);
        (int row, int column) multiIndex = IndexUtils.Converter.Uni2Multi(index, limits);

        int neighborIndex = IndexUtils.Converter.Multi2Uni(
            (multiIndex.row + (int)direction.y, multiIndex.column + (int)direction.x),
            limits
        );
        if (Mathf.Abs((int)direction.y) > 0 && multiIndex.row + (int)direction.y == 1)
        { // target row is middle one
            neighborIndex -= 1;
        }
        else if (Mathf.Abs((int)direction.y) > 0 && multiIndex.row + (int)direction.y != 1)
        {
            neighborIndex += 1;
        }

        MeshRenderer neighborPiece = boardPieces[neighborIndex];

        if (neighborPiece)
            return neighborPiece;
        else
        {
            Debug.Log("( NeighborPiece() ) | Out of board");
            return null;
        }
    }
}
