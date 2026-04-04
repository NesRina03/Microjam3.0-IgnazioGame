using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LightPuzzle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI attemptsText;
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] TextMeshProUGUI movesText;
    [SerializeField] Transform inputsContainer;
    [SerializeField] Transform gatesContainer;
    [SerializeField] Transform outputContainer;

    [Header("Prefabs")]
    [SerializeField] GameObject inputTogglePrefab;
    [SerializeField] GameObject gateCardPrefab;
    [SerializeField] GameObject outputIndicatorPrefab;

    [Header("Gate Sprites")]
    [SerializeField] Sprite spriteAND;
    [SerializeField] Sprite spriteOR;
    [SerializeField] Sprite spriteNOT;
    [SerializeField] Sprite spriteXOR;
    [SerializeField] Sprite spriteNAND;
    [SerializeField] Sprite spriteNOR;
    [SerializeField] Sprite spriteXNOR;

    [Header("Bulb Sprites")]
    [SerializeField] Sprite bulbOff;
    [SerializeField] Sprite bulbOn;

    [Header("Wire Drawing")]
    [SerializeField] GameObject wirePrefab;
    [SerializeField] Transform wireContainer;

    List<RectTransform>    spawnedWires    = new List<RectTransform>();
    List<RectTransform>    gateRects       = new List<RectTransform>();
    List<TextMeshProUGUI>  gateValueLabels = new List<TextMeshProUGUI>();

    // ── Gate definition ───────────────────────────────────────────────
    struct Gate
    {
        public string type;
        public int inA, inB;
        public int gateA, gateB;
    }

    struct PuzzleLayout
    {
        public string   label;
        public string[] inputs;
        public Gate[]   gates;
        public int      outputGate;
    }

    PuzzleLayout[] puzzles;

    // ── Runtime state ─────────────────────────────────────────────────
    int    currentPuzzle = 0;
    bool[] inputValues;
    bool[] gateResults;
    int    togglesUsed   = 0;
    int    minMovesNeeded = 0;
    bool   hintUsed      = false;
    bool   solved        = false;
    bool   failed        = false;

    List<Toggle>          inputToggles = new List<Toggle>();
    List<TextMeshProUGUI> gateLabels   = new List<TextMeshProUGUI>();
    List<Image>           gateCards    = new List<Image>();
    Image                 outputImage;

    // ── Gate colors ───────────────────────────────────────────────────
    Color colAND  = new Color(0f,    0.10f, 0.04f, 1f);
    Color colOR   = new Color(0f,    0.06f, 0.10f, 1f);
    Color colNOT  = new Color(0.10f, 0.02f, 0.02f, 1f);
    Color colXOR  = new Color(0.10f, 0.05f, 0f,    1f);
    Color colNAND = new Color(0.06f, 0f,    0.10f, 1f);
    Color colNOR  = new Color(0f,    0.10f, 0.10f, 1f);
    Color colXNOR = new Color(0.10f, 0.10f, 0f,    1f);
    Color colInactive = new Color(0.04f, 0.08f, 0.05f, 1f);

    // ─────────────────────────────────────────────────────────────────
    void Awake() => BuildPuzzles();

    void OnEnable()
    {
        solved       = false;
        failed       = false;
        hintUsed     = false;
        togglesUsed  = 0;
        currentPuzzle = Random.Range(0, puzzles.Length);
        SpawnUI();
    }

    // ── Build puzzle layouts ──────────────────────────────────────────
    void BuildPuzzles()
    {
        puzzles = new PuzzleLayout[]
        {
            new PuzzleLayout
            {
                label = "LVL 1", inputs = new[]{"A","B","C"},
                gates = new Gate[]
                {
                    new Gate{type="AND", inA=0,inB=1, gateA=-1,gateB=-1},
                    new Gate{type="NOT", inA=2,inB=-1,gateA=-1,gateB=-1},
                    new Gate{type="OR",  inA=-1,inB=-1,gateA=0,gateB=1},
                    new Gate{type="AND", inA=0,inB=-1,gateA=-1,gateB=2},
                },
                outputGate = 3
            },
            new PuzzleLayout
            {
                label = "LVL 2", inputs = new[]{"A","B","C","D"},
                gates = new Gate[]
                {
                    new Gate{type="XOR", inA=0,inB=1, gateA=-1,gateB=-1},
                    new Gate{type="NOT", inA=2,inB=-1,gateA=-1,gateB=-1},
                    new Gate{type="OR",  inA=-1,inB=3,gateA=1, gateB=-1},
                    new Gate{type="AND", inA=-1,inB=-1,gateA=0,gateB=2},
                },
                outputGate = 3
            },
            new PuzzleLayout
            {
                label = "LVL 3", inputs = new[]{"A","B","C","D"},
                gates = new Gate[]
                {
                    new Gate{type="NAND",inA=0,inB=1, gateA=-1,gateB=-1},
                    new Gate{type="XNOR",inA=2,inB=3, gateA=-1,gateB=-1},
                    new Gate{type="NOT", inA=0,inB=-1,gateA=-1,gateB=-1},
                    new Gate{type="AND", inA=-1,inB=-1,gateA=0,gateB=1},
                    new Gate{type="OR",  inA=-1,inB=-1,gateA=3,gateB=2},
                },
                outputGate = 4
            },
            new PuzzleLayout
            {
                label = "LVL 4", inputs = new[]{"A","B","C","D"},
                gates = new Gate[]
                {
                    new Gate{type="NOR", inA=0,inB=1, gateA=-1,gateB=-1},
                    new Gate{type="XOR", inA=1,inB=2, gateA=-1,gateB=-1},
                    new Gate{type="NAND",inA=3,inB=0, gateA=-1,gateB=-1},
                    new Gate{type="AND", inA=-1,inB=-1,gateA=1,gateB=2},
                    new Gate{type="OR",  inA=-1,inB=-1,gateA=0,gateB=3},
                },
                outputGate = 4
            },
            new PuzzleLayout
            {
                label = "LVL 5", inputs = new[]{"A","B","C","D","E"},
                gates = new Gate[]
                {
                    new Gate{type="NAND",inA=0,inB=1, gateA=-1,gateB=-1},
                    new Gate{type="XOR", inA=0,inB=2, gateA=-1,gateB=-1},
                    new Gate{type="AND", inA=1,inB=3, gateA=-1,gateB=-1},
                    new Gate{type="NOR", inA=3,inB=4, gateA=-1,gateB=-1},
                    new Gate{type="OR",  inA=-1,inB=-1,gateA=0,gateB=1},
                    new Gate{type="XNOR",inA=-1,inB=-1,gateA=2,gateB=3},
                    new Gate{type="NAND",inA=-1,inB=2, gateA=4,gateB=-1},
                    new Gate{type="AND", inA=-1,inB=4, gateA=5,gateB=-1},
                    new Gate{type="XOR", inA=-1,inB=-1,gateA=6,gateB=7},
                },
                outputGate = 8
            },
        };
    }

    // ── Calculate minimum moves needed via BFS ────────────────────────
    int CalculateMinMoves(PuzzleLayout p)
    {
        int n = p.inputs.Length;

        // Start state: all inputs false (represented as int bitmask)
        int startState = 0;

        // If start state already gives output 1, 0 moves needed
        if (EvaluateState(p, startState)) return 0;

        // BFS over all possible input states
        Queue<(int state, int moves)> queue = new Queue<(int, int)>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue((startState, 0));
        visited.Add(startState);

        while (queue.Count > 0)
        {
            var (state, moves) = queue.Dequeue();

            // Try flipping each input one at a time
            for (int i = 0; i < n; i++)
            {
                int newState = state ^ (1 << i); // flip bit i

                if (visited.Contains(newState)) continue;
                visited.Add(newState);

                if (EvaluateState(p, newState))
                    return moves + 1; // found minimum

                queue.Enqueue((newState, moves + 1));
            }
        }

        return n; // fallback — worst case all inputs
    }

    // ── Evaluate a bitmask state against the circuit ──────────────────
    bool EvaluateState(PuzzleLayout p, int stateMask)
    {
        bool[] inp = new bool[p.inputs.Length];
        for (int i = 0; i < p.inputs.Length; i++)
            inp[i] = ((stateMask >> i) & 1) == 1;

        bool[] res = EvaluateAll(p, inp);
        return res[p.outputGate];
    }

    // ── Spawn UI ──────────────────────────────────────────────────────
    void SpawnUI()
    {
        foreach (Transform t in inputsContainer) Destroy(t.gameObject);
        foreach (Transform t in gatesContainer)  Destroy(t.gameObject);
        foreach (Transform t in outputContainer) Destroy(t.gameObject);

        inputToggles.Clear();
        gateLabels.Clear();
        gateCards.Clear();
        gateRects.Clear();
        gateValueLabels.Clear();
        spawnedWires.Clear();

        PuzzleLayout p = puzzles[currentPuzzle];
        inputValues = new bool[p.inputs.Length];
        gateResults = new bool[p.gates.Length];

        // Calculate minimum moves before spawning
        minMovesNeeded = CalculateMinMoves(p);

        // If no solution possible (output already 1 or impossible) reroll
        if (minMovesNeeded == 0)
        {
            currentPuzzle = (currentPuzzle + 1) % puzzles.Length;
            p = puzzles[currentPuzzle];
            minMovesNeeded = CalculateMinMoves(p);
        }

        // ── Spawn inputs ──────────────────────────────────────────
        for (int i = 0; i < p.inputs.Length; i++)
        {
            GameObject go  = Instantiate(inputTogglePrefab, inputsContainer);
            Toggle     tog = go.GetComponentInChildren<Toggle>();

            TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = p.inputs[i];

            int captured = i;
            tog.isOn = false;
            tog.onValueChanged.AddListener((val) =>
            {
                if (solved || failed) return;

                inputValues[captured] = val;
                if (texts.Length > 1)
                    texts[1].text = val ? "1" : "0";

                togglesUsed++;
                UpdateMovesDisplay();
                Evaluate();
            });

            inputToggles.Add(tog);
        }

        // ── Spawn gates ───────────────────────────────────────────
        int[] gateColumn = new int[p.gates.Length];
        for (int i = 0; i < p.gates.Length; i++)
        {
            int col = 0;
            if (p.gates[i].gateA != -1)
                col = Mathf.Max(col, gateColumn[p.gates[i].gateA] + 1);
            if (p.gates[i].gateB != -1)
                col = Mathf.Max(col, gateColumn[p.gates[i].gateB] + 1);
            gateColumn[i] = col;
        }

        int totalCols = 0;
        foreach (int c in gateColumn) totalCols = Mathf.Max(totalCols, c + 1);

        int[] gatesInCol   = new int[totalCols];
        int[] gateRowIndex = new int[p.gates.Length];
        for (int i = 0; i < p.gates.Length; i++)
        {
            gateRowIndex[i] = gatesInCol[gateColumn[i]];
            gatesInCol[gateColumn[i]]++;
        }

        float colWidth  = 220f;
        float rowHeight = 140f;
        float startX    = -(totalCols - 1) * colWidth / 2f;

        for (int i = 0; i < p.gates.Length; i++)
        {
            GameObject    go  = Instantiate(gateCardPrefab, gatesContainer);
            RectTransform rt  = go.GetComponent<RectTransform>();
            Image         img = go.GetComponent<Image>();

            int   col    = gateColumn[i];
            int   row    = gateRowIndex[i];
            int   rows   = gatesInCol[col];
            float startY = (rows - 1) * rowHeight / 2f;

            rt.anchoredPosition = new Vector2(
                startX + col * colWidth,
                startY - row * rowHeight
            );

            Sprite gateSprite = GetGateSprite(p.gates[i].type);
            if (gateSprite != null)
            {
                img.sprite         = gateSprite;
                img.color          = Color.white;
                img.type           = Image.Type.Simple;
                img.preserveAspect = true;
                img.SetNativeSize();

                float targetHeight = 100f;
                float aspect = (float)gateSprite.texture.width
                             / gateSprite.texture.height;
                rt.sizeDelta = new Vector2(targetHeight * aspect, targetHeight);
            }
            else
            {
                img.sprite = null;
                img.color  = GetGateColor(p.gates[i].type);
                rt.sizeDelta = p.gates[i].type == "NOT"
                    ? new Vector2(90f,  100f)
                    : new Vector2(140f, 100f);
            }

            TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].gameObject.SetActive(false);
            if (texts.Length > 1) gateValueLabels.Add(texts[1]);

            if (img != null) gateCards.Add(img);
            gateRects.Add(rt);
        }

        // ── Spawn output ──────────────────────────────────────────
        if (outputIndicatorPrefab != null)
        {
            GameObject go = Instantiate(outputIndicatorPrefab, outputContainer);
            outputImage   = go.GetComponent<Image>();
            if (bulbOff != null)
            {
                outputImage.sprite         = bulbOff;
                outputImage.color          = Color.white;
                outputImage.preserveAspect = true;
                outputImage.SetNativeSize();
            }
        }

        // ── Status ────────────────────────────────────────────────
        if (statusText != null)
        {
            statusText.text  = p.label + " — OUTPUT: 0";
            statusText.color = new Color(0.18f, 0.48f, 0.23f);
        }

        UpdateMovesDisplay();
        StartCoroutine(DrawWiresNextFrame());
    }

    // ── Moves display ─────────────────────────────────────────────────
    void UpdateMovesDisplay()
    {
        if (movesText == null) return;
        int remaining = minMovesNeeded - togglesUsed;

        if (remaining > 0)
        {
            movesText.text  = "MOVES LEFT: " + remaining + " / " + minMovesNeeded;
            movesText.color = remaining == 1
                ? new Color(1f, 0.4f, 0f)    // orange — last move warning
                : new Color(0.18f, 0.48f, 0.23f);
        }
        else if (remaining == 0 && !solved)
        {
            movesText.text  = "LAST MOVE USED";
            movesText.color = new Color(1f, 0.2f, 0.1f);
        }
    }

    // ── Evaluate ──────────────────────────────────────────────────────
    void Evaluate()
    {
        PuzzleLayout p = puzzles[currentPuzzle];

        for (int i = 0; i < p.gates.Length; i++)
        {
            Gate g = p.gates[i];
            bool a = g.gateA == -1 ? inputValues[g.inA] : gateResults[g.gateA];
            bool b = false;
            if (g.type != "NOT")
                b = g.gateB == -1 ? inputValues[g.inB] : gateResults[g.gateB];
            gateResults[i] = EvalGate(g.type, a, b);
        }

        RefreshUI();
        CheckSolved();

        // Check fail — used all moves without solving
        if (!solved && togglesUsed >= minMovesNeeded && !gateResults[p.outputGate])
            StartCoroutine(FailSequence());
    }

    bool EvalGate(string type, bool a, bool b)
    {
        switch (type)
        {
            case "AND":  return a && b;
            case "OR":   return a || b;
            case "NOT":  return !a;
            case "XOR":  return a ^ b;
            case "NAND": return !(a && b);
            case "NOR":  return !(a || b);
            case "XNOR": return !(a ^ b);
            default:     return false;
        }
    }

    // ── Refresh UI ────────────────────────────────────────────────────
    void RefreshUI()
    {
        PuzzleLayout p = puzzles[currentPuzzle];

        for (int i = 0; i < gateCards.Count && i < p.gates.Length; i++)
        {
            bool active = gateResults[i];
            if (gateCards[i].sprite != null)
                gateCards[i].color = active ? Color.white
                    : new Color(0.4f, 0.4f, 0.4f, 1f);
            else
                gateCards[i].color = active
                    ? new Color(0f, 0.18f, 0.10f, 1f)
                    : GetGateColor(p.gates[i].type);

            if (i < gateValueLabels.Count && gateValueLabels[i] != null)
                gateValueLabels[i].text = "out: " + (active ? "1" : "0");
        }

        if (outputImage != null)
        {
            bool finalOut = gateResults[p.outputGate];
            outputImage.sprite         = finalOut ? bulbOn : bulbOff;
            outputImage.color          = Color.white;
            outputImage.preserveAspect = true;
            outputImage.SetNativeSize();
        }

        DrawWires();

        if (statusText != null)
        {
            bool out1 = gateResults[p.outputGate];
            statusText.text  = puzzles[currentPuzzle].label
                             + " — OUTPUT: " + (out1 ? "1" : "0");
            statusText.color = out1
                ? new Color(0f, 1f, 0.53f)
                : new Color(0.18f, 0.48f, 0.23f);
        }
    }

    // ── Win check ─────────────────────────────────────────────────────
    void CheckSolved()
    {
        if (solved || failed) return;
        if (togglesUsed == 0)  return;

        PuzzleLayout p = puzzles[currentPuzzle];
        if (!gateResults[p.outputGate]) return;

        // Only win if used exactly the minimum moves
        if (togglesUsed != minMovesNeeded)
        {
            // Solved but with wrong number — treat as fail
            StartCoroutine(FailSequence());
            return;
        }

        solved = true;

        if (statusText != null)
        {
            statusText.text  = "SOLVED IN " + togglesUsed + " MOVES — PERFECT";
            statusText.color = new Color(0f, 1f, 0.53f);
        }

        if (movesText != null)
        {
            movesText.text  = "MINIMUM MOVES ACHIEVED";
            movesText.color = new Color(0f, 1f, 0.53f);
        }

        Invoke(nameof(StartSolvedSequence), 0.1f);
    }

    void StartSolvedSequence() => StartCoroutine(SolvedSequence());

    System.Collections.IEnumerator SolvedSequence()
    {
        foreach (RectTransform w in spawnedWires)
        {
            if (w == null) continue;
            Image img = w.GetComponent<Image>();
            if (img != null) img.color = new Color(0f, 1f, 0.53f, 1f);
        }

        yield return new WaitForSeconds(2.5f);
        PuzzleManager.Instance.NotifySolved("light");
    }

    // ── Fail sequence ─────────────────────────────────────────────────
    System.Collections.IEnumerator FailSequence()
    {
        if (failed) yield break;
        failed = true;

        // Flash wires red
        foreach (RectTransform w in spawnedWires)
        {
            if (w == null) continue;
            Image img = w.GetComponent<Image>();
            if (img != null) img.color = new Color(1f, 0.15f, 0.1f, 1f);
        }

        if (statusText != null)
        {
            statusText.text  = "CIRCUIT FAILED — RESETTING";
            statusText.color = new Color(1f, 0.15f, 0.1f);
        }

        if (movesText != null)
        {
            movesText.text  = "TOO MANY MOVES — PENALTY";
            movesText.color = new Color(1f, 0.15f, 0.1f);
        }

        // Apply instability penalty
        PuzzleManager.Instance.NotifyFailed(0.2f);

        yield return new WaitForSeconds(2f);

        // Load a new random circuit
        solved       = false;
        failed       = false;
        hintUsed     = false;
        togglesUsed  = 0;
        currentPuzzle = Random.Range(0, puzzles.Length);
        SpawnUI();
    }

    // ── Hint ──────────────────────────────────────────────────────────
    public void UseHint()
    {
        if (hintUsed)
        {
            if (hintText != null) hintText.text = "HINT ALREADY USED";
            return;
        }

        PuzzleLayout p     = puzzles[currentPuzzle];
        int          count = p.inputs.Length;
        int          rows  = (int)Mathf.Pow(2, count);

        for (int r = 0; r < rows; r++)
        {
            bool[] test = new bool[count];
            for (int i = 0; i < count; i++)
                test[i] = ((r >> (count - 1 - i)) & 1) == 1;

            // Only show hint if it uses exactly minimum moves from start
            int flips = 0;
            for (int i = 0; i < count; i++)
                if (test[i]) flips++;

            if (flips != minMovesNeeded) continue;

            bool[] res = EvaluateAll(p, test);
            if (res[p.outputGate])
            {
                string hint = "";
                for (int i = 0; i < count; i++)
                    if (test[i]) hint += p.inputs[i] + " ";

                if (hintText != null)
                    hintText.text = "TOGGLE: " + hint.Trim();

                hintUsed = true;
                PuzzleManager.Instance.NotifyFailed(0.05f);
                return;
            }
        }
    }

    bool[] EvaluateAll(PuzzleLayout p, bool[] inputs)
    {
        bool[] res = new bool[p.gates.Length];
        for (int i = 0; i < p.gates.Length; i++)
        {
            Gate g = p.gates[i];
            bool a = g.gateA == -1 ? inputs[g.inA] : res[g.gateA];
            bool b = false;
            if (g.type != "NOT")
                b = g.gateB == -1 ? inputs[g.inB] : res[g.gateB];
            res[i] = EvalGate(g.type, a, b);
        }
        return res;
    }

    // ── Reset ─────────────────────────────────────────────────────────
    public void ResetPuzzle()
    {
        if (solved) return;
        foreach (Toggle t in inputToggles) t.isOn = false;
    }

    // ── Wire drawing ──────────────────────────────────────────────────
    System.Collections.IEnumerator DrawWiresNextFrame()
    {
        yield return null;
        DrawWires();
    }

    void DrawWires()
    {
        if (wirePrefab == null) return;

        foreach (RectTransform w in spawnedWires)
            if (w != null) Destroy(w.gameObject);
        spawnedWires.Clear();

        PuzzleLayout p = puzzles[currentPuzzle];

        for (int i = 0; i < p.gates.Length; i++)
        {
            Gate g = p.gates[i];

            if (g.gateA != -1)
                DrawWire(gateRects[g.gateA], gateRects[i], gateResults[g.gateA]);
            else if (g.inA >= 0 && g.inA < inputToggles.Count)
                DrawWireFromInput(g.inA, gateRects[i], inputValues[g.inA]);

            if (g.type != "NOT")
            {
                if (g.gateB != -1)
                    DrawWire(gateRects[g.gateB], gateRects[i], gateResults[g.gateB]);
                else if (g.inB >= 0 && g.inB < inputToggles.Count)
                    DrawWireFromInput(g.inB, gateRects[i], inputValues[g.inB]);
            }
        }
    }

    void DrawWire(RectTransform from, RectTransform to, bool active)
    {
        Vector2 start = from.anchoredPosition + new Vector2(from.sizeDelta.x * 0.5f, 0f);
        Vector2 end   = to.anchoredPosition   - new Vector2(to.sizeDelta.x   * 0.5f, 0f);
        DrawBezierWire(start, end, active);
    }

    void DrawWireFromInput(int inputIndex, RectTransform to, bool active)
    {
        if (inputIndex >= inputToggles.Count) return;
        RectTransform fromRT = inputToggles[inputIndex]
                               .GetComponentInParent<RectTransform>();
        if (fromRT == null) return;

        Vector3 world    = fromRT.TransformPoint(
                           new Vector3(fromRT.sizeDelta.x * 0.5f, 0f, 0f));
        Vector2 localPos = ((RectTransform)gatesContainer)
                           .InverseTransformPoint(world);
        Vector2 end      = to.anchoredPosition
                         - new Vector2(to.sizeDelta.x * 0.5f, 0f);

        DrawBezierWire(localPos, end, active);
    }

    void DrawBezierWire(Vector2 start, Vector2 end, bool active)
    {
        Transform parent = wireContainer != null ? wireContainer : gatesContainer;

        Color wireColor = active
            ? new Color(0f,    1f,    0.53f, 1f  )
            : new Color(0.15f, 0.35f, 0.18f, 0.9f);

        List<Vector2> points = BezierWire.GetPoints(start, end, 16);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 s   = points[i];
            Vector2 e   = points[i + 1];
            Vector2 dir = e - s;
            float len   = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject    wire = Instantiate(wirePrefab, parent);
            RectTransform rt   = wire.GetComponent<RectTransform>();
            Image         img  = wire.GetComponent<Image>();

            rt.anchoredPosition = s + dir * 0.5f;
            rt.sizeDelta        = new Vector2(len + 1f, active ? 4f : 2f);
            rt.localRotation    = Quaternion.Euler(0f, 0f, angle);

            if (img != null) img.color = wireColor;
            spawnedWires.Add(rt);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────
    Sprite GetGateSprite(string type)
    {
        switch (type)
        {
            case "AND":  return spriteAND;
            case "OR":   return spriteOR;
            case "NOT":  return spriteNOT;
            case "XOR":  return spriteXOR;
            case "NAND": return spriteNAND;
            case "NOR":  return spriteNOR;
            case "XNOR": return spriteXNOR;
            default:     return null;
        }
    }

    Color GetGateColor(string type)
    {
        switch (type)
        {
            case "AND":  return colAND;
            case "OR":   return colOR;
            case "NOT":  return colNOT;
            case "XOR":  return colXOR;
            case "NAND": return colNAND;
            case "NOR":  return colNOR;
            case "XNOR": return colXNOR;
            default:     return colInactive;
        }
    }

    void UpdateAttempts()
    {
        if (attemptsText != null)
            attemptsText.text = "TOGGLES: " + togglesUsed;
    }
}

// ── Bezier helper ─────────────────────────────────────────────────────
public static class BezierWire
{
    public static List<Vector2> GetPoints(Vector2 start, Vector2 end,
                                          int segments = 12)
    {
        var     points = new List<Vector2>();
        float   dx     = Mathf.Abs(end.x - start.x);
        Vector2 cp1    = new Vector2(start.x + dx * 0.5f, start.y);
        Vector2 cp2    = new Vector2(end.x   - dx * 0.5f, end.y);

        for (int i = 0; i <= segments; i++)
        {
            float   t   = i / (float)segments;
            float   u   = 1 - t;
            Vector2 pt  = u*u*u * start
                        + 3*u*u*t * cp1
                        + 3*u*t*t * cp2
                        + t*t*t   * end;
            points.Add(pt);
        }
        return points;
    }
}