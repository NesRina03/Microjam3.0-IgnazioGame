using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LightPuzzle : MonoBehaviour, IPuzzlePanel
{
    [Header("Factor")]
    [SerializeField] string factorName = "light"; // set in Inspector to whichever factor Wordle is assigned
    [Header("UI References")]

    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI movesText;
    [SerializeField] TextMeshProUGUI hintText;
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

    // ── Runtime references ────────────────────────────────────────────
    List<RectTransform>   spawnedWires    = new List<RectTransform>();
    List<RectTransform>   gateRects       = new List<RectTransform>();
    List<Image>           gateCards       = new List<Image>();
    List<Toggle>          inputToggles    = new List<Toggle>();
    Image                 outputImage;

    // ── Gate definition ───────────────────────────────────────────────
    struct Gate
    {
        public string type;
        public int inA, inB;   // input indices (-1 = use gate output)
        public int gateA, gateB; // gate indices (-1 = use input)
    }

    struct Circuit
    {
        public string[] inputs;
        public Gate[]   gates;
        public int      outputGate;
    }

    // ── Runtime state ─────────────────────────────────────────────────
    Circuit currentCircuit;
    bool[]  inputValues;
    bool[]  gateResults;
    int     togglesUsed   = 0;
    int     minMoves      = 0;
    bool    solved        = false;
    bool    failed        = false;
    bool    hintUsed      = false;
    bool    viewOnly      = false;

    // ── Gate colors ───────────────────────────────────────────────────
    readonly Color colInactive = new Color(0.04f, 0.08f, 0.05f, 1f);

    // ─────────────────────────────────────────────────────────────────
    void Awake() { }

    void OnEnable()
    {
        solved      = false;
        failed      = false;
        hintUsed    = false;
        viewOnly    = false;
        togglesUsed = 0;
        GenerateAndSpawn();
    }

    // ── IPuzzlePanel ──────────────────────────────────────────────────
    public void SetViewOnly(bool isViewOnly)
    {
        viewOnly = isViewOnly;
        foreach (Toggle t in inputToggles)
            t.interactable = !isViewOnly;

        if (statusText != null && isViewOnly)
        {
            statusText.text  = "SOLVED — VIEW MODE";
            statusText.color = new Color(0f, 1f, 0.53f);
        }

        if (movesText != null && isViewOnly)
            movesText.text = "";
    }

    // ── RNG Circuit Generation ────────────────────────────────────────
    Circuit GenerateCircuit()
    {
        string[] gateTypes = { "AND","OR","NOT","XOR","NAND","NOR","XNOR" };

        // Pick 4 or 5 inputs randomly
        int inputCount = Random.Range(4, 6);
        string[] inputs = new string[inputCount];
        string[] letters = { "A","B","C","D","E","F" };
        for (int i = 0; i < inputCount; i++)
            inputs[i] = letters[i];

        // Build gates in layers
        // Layer 0: gates that take raw inputs
        // Layer 1+: gates that can take previous gate outputs
        List<Gate> gates = new List<Gate>();
        int layer0Count = Random.Range(2, 4);

        // Track which inputs are used so all get connected
        List<int> unusedInputs = new List<int>();
        for (int i = 0; i < inputCount; i++) unusedInputs.Add(i);

        // Layer 0 — connect raw inputs
        for (int g = 0; g < layer0Count; g++)
        {
            string type = gateTypes[Random.Range(0, gateTypes.Length)];
            bool isNOT  = type == "NOT";

            int inA = unusedInputs.Count > 0
                ? PopRandom(unusedInputs)
                : Random.Range(0, inputCount);

            int inB = -1;
            if (!isNOT)
                inB = unusedInputs.Count > 0
                    ? PopRandom(unusedInputs)
                    : Random.Range(0, inputCount);

            gates.Add(new Gate
            {
                type  = type,
                inA   = inA,
                inB   = inB,
                gateA = -1,
                gateB = -1
            });
        }

        // Make sure remaining unused inputs feed into existing gates
        // by overwriting inB of a random layer0 gate
        while (unusedInputs.Count > 0)
        {
            int inp    = PopRandom(unusedInputs);
            int target = Random.Range(0, layer0Count);
            Gate g     = gates[target];
            if (g.type != "NOT")
            {
                g.inB      = inp;
                gates[target] = g;
            }
        }

        // Layer 1 — mix gate outputs
        int layer1Count = Random.Range(1, 3);
        for (int g = 0; g < layer1Count; g++)
        {
            string type = gateTypes[Random.Range(0, gateTypes.Length)];
            bool isNOT  = type == "NOT";

            int gA = Random.Range(0, layer0Count);
            int gB = isNOT ? -1 : Random.Range(0, layer0Count);

            // Avoid same gate feeding both inputs
            if (!isNOT && gB == gA)
                gB = (gA + 1) % layer0Count;

            gates.Add(new Gate
            {
                type  = type,
                inA   = -1,
                inB   = -1,
                gateA = gA,
                gateB = isNOT ? -1 : layer0Count + g > gA ? gA : gB
            });
        }

        // Final gate — takes last two gates as input
        int total = gates.Count;
        string finalType = gateTypes[Random.Range(0, gateTypes.Length)];
        while (finalType == "NOT") // final gate must take 2 inputs
            finalType = gateTypes[Random.Range(0, gateTypes.Length)];

        gates.Add(new Gate
        {
            type  = finalType,
            inA   = -1,
            inB   = -1,
            gateA = total - 2 < 0 ? 0 : total - 2,
            gateB = total - 1 < 0 ? 0 : total - 1
        });

        return new Circuit
        {
            inputs     = inputs,
            gates      = gates.ToArray(),
            outputGate = gates.Count - 1
        };
    }

    int PopRandom(List<int> list)
    {
        int idx = Random.Range(0, list.Count);
        int val = list[idx];
        list.RemoveAt(idx);
        return val;
    }

    // Try generating until we get a circuit that needs
    // at least 1 move and at most inputCount-1 moves
   void GenerateAndSpawn()
{
    int attempts = 0;
    bool valid = false;

    do
    {
        currentCircuit = GenerateCircuit();

        // Check actual initial output with all inputs false
        inputValues = new bool[currentCircuit.inputs.Length];
        gateResults = new bool[currentCircuit.gates.Length];
        EvaluateSilent();

        bool initialOutput = gateResults[currentCircuit.outputGate];

        // Reject if already solved at start
        if (initialOutput)
        {
            attempts++;
            continue;
        }

        minMoves = CalculateMinMoves(currentCircuit);

        // Accept only if minMoves is 1, 2, or 3 — hard but fair
        valid = minMoves >= 1 && minMoves <= 3;
        attempts++;
    }
    while (!valid && attempts < 100);

    SpawnUI();
}

    // ── Min moves via BFS ─────────────────────────────────────────────
    int CalculateMinMoves(Circuit c)
    {
        int n          = c.inputs.Length;
        int startState = 0;

        if (EvaluateState(c, startState)) return 0;

        var queue   = new Queue<(int state, int moves)>();
        var visited = new HashSet<int>();

        queue.Enqueue((startState, 0));
        visited.Add(startState);

        while (queue.Count > 0)
        {
            var (state, moves) = queue.Dequeue();
            for (int i = 0; i < n; i++)
            {
                int next = state ^ (1 << i);
                if (visited.Contains(next)) continue;
                visited.Add(next);
                if (EvaluateState(c, next)) return moves + 1;
                queue.Enqueue((next, moves + 1));
            }
        }
        return n;
    }

    bool EvaluateState(Circuit c, int mask)
{
    bool[] inp = new bool[c.inputs.Length];
    for (int i = 0; i < c.inputs.Length; i++)
        inp[i] = ((mask >> i) & 1) == 1;

    bool[] res = EvaluateAll(c, inp);
    return res[c.outputGate];
}

    // ── Spawn UI ──────────────────────────────────────────────────────
    void SpawnUI()
    {
        // Clear old objects
        foreach (Transform t in inputsContainer) Destroy(t.gameObject);
        foreach (Transform t in gatesContainer)  Destroy(t.gameObject);
        foreach (Transform t in outputContainer) Destroy(t.gameObject);
        if (wireContainer != null)
            foreach (Transform t in wireContainer) Destroy(t.gameObject);

        inputToggles.Clear();
        gateCards.Clear();
        gateRects.Clear();
        spawnedWires.Clear();

        Circuit c    = currentCircuit;
        inputValues  = new bool[c.inputs.Length];
        gateResults  = new bool[c.gates.Length];

        // ── Evaluate initial state (all inputs false) ─────────────
        // Fix 2: evaluate immediately so NOT gates show correct output
        EvaluateSilent();

        // ── Spawn inputs ──────────────────────────────────────────
        for (int i = 0; i < c.inputs.Length; i++)
        {
            GameObject go  = Instantiate(inputTogglePrefab, inputsContainer);
            Toggle     tog = go.GetComponentInChildren<Toggle>();

            TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = c.inputs[i];

            int captured = i;
            tog.isOn = false;
            tog.onValueChanged.AddListener((val) =>
            {
                if (solved || failed || viewOnly) return;
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
        int[] gateColumn = new int[c.gates.Length];
        for (int i = 0; i < c.gates.Length; i++)
        {
            int col = 0;
            if (c.gates[i].gateA != -1)
                col = Mathf.Max(col, gateColumn[c.gates[i].gateA] + 1);
            if (c.gates[i].gateB != -1)
                col = Mathf.Max(col, gateColumn[c.gates[i].gateB] + 1);
            gateColumn[i] = col;
        }

        int totalCols = 0;
        foreach (int col in gateColumn)
            totalCols = Mathf.Max(totalCols, col + 1);

        int[] gatesInCol   = new int[totalCols];
        int[] gateRowIndex = new int[c.gates.Length];
        for (int i = 0; i < c.gates.Length; i++)
        {
            gateRowIndex[i] = gatesInCol[gateColumn[i]];
            gatesInCol[gateColumn[i]]++;
        }

        float colWidth  = 220f;
        float rowHeight = 140f;
        float startX    = -(totalCols - 1) * colWidth / 2f;

        for (int i = 0; i < c.gates.Length; i++)
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

            Sprite sp = GetGateSprite(c.gates[i].type);
            if (sp != null)
            {
                img.sprite         = sp;
                img.color          = Color.white;
                img.type           = Image.Type.Simple;
                img.preserveAspect = true;
                img.SetNativeSize();

                float h      = 100f;
                float aspect = (float)sp.texture.width / sp.texture.height;
                rt.sizeDelta = new Vector2(h * aspect, h);
            }
            else
            {
                img.sprite   = null;
                img.color    = GetGateColor(c.gates[i].type);
                rt.sizeDelta = c.gates[i].type == "NOT"
                    ? new Vector2(90f, 100f)
                    : new Vector2(140f, 100f);
            }

            // Hide text labels — sprite shows gate type
            foreach (TextMeshProUGUI txt in go.GetComponentsInChildren<TextMeshProUGUI>())
                txt.gameObject.SetActive(false);

            if (img != null) gateCards.Add(img);
            gateRects.Add(rt);
        }

        // ── Spawn output bulb ─────────────────────────────────────
        // Fix 5: always spawn with explicit size so it's visible
        if (outputIndicatorPrefab != null)
        {
            GameObject    go = Instantiate(outputIndicatorPrefab, outputContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            outputImage      = go.GetComponent<Image>();

            // Force size so it's always visible
            rt.sizeDelta = new Vector2(100f, 120f);

            if (outputImage != null)
            {
                Sprite initial         = bulbOff != null ? bulbOff : null;
                outputImage.sprite     = initial;
                outputImage.color      = Color.white;
                outputImage.preserveAspect = true;
                if (initial != null) outputImage.SetNativeSize();
            }
        }

        // ── Status ────────────────────────────────────────────────
        if (statusText != null)
        {
            statusText.text  = "SET OUTPUT TO 1";
            statusText.color = new Color(0.18f, 0.48f, 0.23f);
        }

        UpdateMovesDisplay();

        // Fix 4: draw wires after 2 frames to ensure layout is settled
        StartCoroutine(DrawWiresAfterFrames(3));

        // Fix 2: refresh visuals immediately with initial gate states
        RefreshGateVisuals();
        RefreshBulb();
    }

    // ── Silent evaluate (no UI refresh) ──────────────────────────────
    void EvaluateSilent()
    {
        Circuit c = currentCircuit;
        for (int i = 0; i < c.gates.Length; i++)
        {
            Gate g = c.gates[i];
            bool a = g.gateA == -1 ? inputValues[g.inA] : gateResults[g.gateA];
            bool b = false;
            if (g.type != "NOT")
                b = g.gateB == -1 ? inputValues[g.inB] : gateResults[g.gateB];
            gateResults[i] = EvalGate(g.type, a, b);
        }
    }

    // ── Full evaluate + UI refresh ────────────────────────────────────
    void Evaluate()
    {
        EvaluateSilent();
        RefreshGateVisuals();
        RefreshBulb();
        DrawWires();
        UpdateStatus();
        CheckSolved();

        Circuit c = currentCircuit;
        if (!solved && !failed
            && togglesUsed >= minMoves
            && !gateResults[c.outputGate])
        {
            StartCoroutine(FailSequence());
        }
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

    // ── Refresh gate card colors ──────────────────────────────────────
    void RefreshGateVisuals()
    {
        Circuit c = currentCircuit;
        for (int i = 0; i < gateCards.Count && i < c.gates.Length; i++)
        {
            bool active = gateResults[i];
            if (gateCards[i].sprite != null)
                // Active = full white (bright), inactive = dimmed
                gateCards[i].color = active
                    ? Color.white
                    : new Color(0.35f, 0.35f, 0.35f, 1f);
            else
                gateCards[i].color = active
                    ? new Color(0f, 0.4f, 0.25f, 1f)
                    : GetGateColor(c.gates[i].type);
        }
    }

    // ── Refresh output bulb ───────────────────────────────────────────
    void RefreshBulb()
    {
        if (outputImage == null) return;
        Circuit c       = currentCircuit;
        bool    finalOut = gateResults[c.outputGate];

        // Fix 5: swap sprite, always white color, force size
        Sprite target          = finalOut
            ? (bulbOn  != null ? bulbOn  : null)
            : (bulbOff != null ? bulbOff : null);

        outputImage.sprite         = target;
        outputImage.color          = Color.white;
        outputImage.preserveAspect = true;

        if (target != null)
            outputImage.SetNativeSize();
        else
        {
            // No sprite assigned — show colored rectangle as fallback
            outputImage.color = finalOut
                ? new Color(1f, 0.9f, 0f, 1f)
                : new Color(0.1f, 0.2f, 0.1f, 1f);
        }
    }

    // ── Update status text ────────────────────────────────────────────
    void UpdateStatus()
    {
        if (statusText == null) return;
        Circuit c    = currentCircuit;
        bool    out1 = gateResults[c.outputGate];
        statusText.text  = "OUTPUT: " + (out1 ? "1" : "0");
        statusText.color = out1
            ? new Color(0f, 1f, 0.53f)
            : new Color(0.18f, 0.48f, 0.23f);
    }

    // ── Moves display ─────────────────────────────────────────────────
    void UpdateMovesDisplay()
    {
        if (movesText == null) return;
        int rem = minMoves - togglesUsed;

        if (rem > 0)
        {
            movesText.text  = "MOVES: " + togglesUsed + " / " + minMoves;
            movesText.color = rem == 1
                ? new Color(1f, 0.4f, 0f)
                : new Color(0.18f, 0.48f, 0.23f);
        }
        else if (!solved)
        {
            movesText.text  = "MOVES EXHAUSTED";
            movesText.color = new Color(1f, 0.2f, 0.1f);
        }
    }

    // ── Win check ─────────────────────────────────────────────────────
    void CheckSolved()
    {
        if (solved || failed || togglesUsed == 0) return;
        Circuit c = currentCircuit;
        if (!gateResults[c.outputGate]) return;

        if (togglesUsed != minMoves)
        {
            StartCoroutine(FailSequence());
            return;
        }

        solved = true;

        if (statusText != null)
        {
            statusText.text  = "SOLVED — PERFECT";
            statusText.color = new Color(0f, 1f, 0.53f);
        }

        if (movesText != null)
        {
            movesText.text  = "MINIMUM MOVES";
            movesText.color = new Color(0f, 1f, 0.53f);
        }

        Invoke(nameof(StartSolvedSequence), 0.1f);
    }

    void StartSolvedSequence() => StartCoroutine(SolvedSequence());

    System.Collections.IEnumerator SolvedSequence()
    {
        // Flash all wires bright green
        foreach (RectTransform w in spawnedWires)
        {
            if (w == null) continue;
            Image img = w.GetComponent<Image>();
            if (img != null) img.color = new Color(0f, 1f, 0.53f, 1f);
        }

        yield return new WaitForSeconds(2.5f);
        PuzzleManager.Instance.NotifySolved(factorName);    }

    // ── Fail sequence ─────────────────────────────────────────────────
    System.Collections.IEnumerator FailSequence()
    {
        if (failed) yield break;
        failed = true;

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

        PuzzleManager.Instance.NotifyFailed(0.2f);

        yield return new WaitForSeconds(2f);

        solved      = false;
        failed      = false;
        hintUsed    = false;
        togglesUsed = 0;
        GenerateAndSpawn();
    }

    // ── Hint ──────────────────────────────────────────────────────────
    public void UseHint()
    {
        if (hintUsed)
        {
            if (hintText != null) hintText.text = "HINT ALREADY USED";
            return;
        }

        Circuit c     = currentCircuit;
        int     count = c.inputs.Length;
        int     rows  = 1 << count;

        for (int r = 0; r < rows; r++)
        {
            bool[] test  = new bool[count];
            int    flips = 0;
            for (int i = 0; i < count; i++)
            {
                test[i] = ((r >> i) & 1) == 1;
                if (test[i]) flips++;
            }

            if (flips != minMoves) continue;

            bool[] res = EvaluateAll(c, test);
            if (!res[c.outputGate]) continue;

            string hint = "TOGGLE: ";
            for (int i = 0; i < count; i++)
                if (test[i]) hint += c.inputs[i] + " ";

            if (hintText != null) hintText.text = hint.Trim();
            hintUsed = true;
            PuzzleManager.Instance.NotifyFailed(0.05f);
            return;
        }
    }

    bool[] EvaluateAll(Circuit c, bool[] inputs)
    {
        bool[] res = new bool[c.gates.Length];
        for (int i = 0; i < c.gates.Length; i++)
        {
            Gate g = c.gates[i];
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
        if (solved || viewOnly) return;
        foreach (Toggle t in inputToggles) t.isOn = false;
    }

    // ── Wire drawing ──────────────────────────────────────────────────
    System.Collections.IEnumerator DrawWiresAfterFrames(int frames)
{
    for (int i = 0; i < frames; i++)
        yield return null;

    // Force all layout groups to recalculate before drawing wires
    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
        inputsContainer as RectTransform);
    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
        gatesContainer as RectTransform);

    DrawWires();
}
    void DrawWires()
    {
        // Fix 4: destroy old wires safely
        for (int i = spawnedWires.Count - 1; i >= 0; i--)
        {
            if (spawnedWires[i] != null)
                Destroy(spawnedWires[i].gameObject);
        }
        spawnedWires.Clear();

        if (wirePrefab == null) return;
        if (gateRects.Count == 0) return;

        Circuit c = currentCircuit;

        for (int i = 0; i < c.gates.Length; i++)
        {
            Gate g = c.gates[i];

            if (g.gateA != -1 && g.gateA < gateRects.Count)
                DrawWire(gateRects[g.gateA], gateRects[i], gateResults[g.gateA]);
            else if (g.inA >= 0 && g.inA < inputToggles.Count)
                DrawWireFromInput(g.inA, gateRects[i], inputValues[g.inA]);

            if (g.type != "NOT")
            {
                if (g.gateB != -1 && g.gateB < gateRects.Count)
                    DrawWire(gateRects[g.gateB], gateRects[i], gateResults[g.gateB]);
                else if (g.inB >= 0 && g.inB < inputToggles.Count)
                    DrawWireFromInput(g.inB, gateRects[i], inputValues[g.inB]);
            }
        }
    }

    void DrawWire(RectTransform from, RectTransform to, bool active)
    {
        if (from == null || to == null) return;
        Vector2 start = from.anchoredPosition
                      + new Vector2(from.sizeDelta.x * 0.5f, 0f);
        Vector2 end   = to.anchoredPosition
                      - new Vector2(to.sizeDelta.x   * 0.5f, 0f);
        DrawBezierWire(start, end, active);
    }

    void DrawWireFromInput(int idx, RectTransform to, bool active)
{
    if (idx >= inputToggles.Count || to == null) return;

    // Get the toggle's root RectTransform
    RectTransform fromRT = inputToggles[idx]
                           .GetComponentInParent<RectTransform>();
    if (fromRT == null) return;

    // Use right edge of toggle as wire start point
    Vector2 fromSize   = fromRT.sizeDelta;
    Vector3 localRight = new Vector3(fromSize.x * 0.5f, 0f, 0f);

    // Convert from inputsContainer space → world → gatesContainer space
    Vector3 worldPoint = fromRT.TransformPoint(localRight);
    RectTransform gatesRT = gatesContainer as RectTransform;

    Vector2 localInGates;
    if (gatesRT != null)
        localInGates = gatesRT.InverseTransformPoint(worldPoint);
    else
        localInGates = gatesContainer.InverseTransformPoint(worldPoint);

    Vector2 end = to.anchoredPosition - new Vector2(to.sizeDelta.x * 0.5f, 0f);

    // Only draw if positions are valid (not zero-zero which means layout not ready)
    if (localInGates == Vector2.zero && to.anchoredPosition == Vector2.zero) return;

    DrawBezierWire(localInGates, end, active);
}

    void DrawBezierWire(Vector2 start, Vector2 end, bool active)
    {
        Transform parent = wireContainer != null ? wireContainer : gatesContainer;
        if (parent == null) return;

        Color color = active
            ? new Color(0f,    1f,    0.53f, 1f)
            : new Color(0.15f, 0.35f, 0.18f, 0.9f);

        List<Vector2> pts = BezierWire.GetPoints(start, end, 16);

        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector2 s   = pts[i];
            Vector2 e   = pts[i + 1];
            Vector2 dir = e - s;
            float   len = dir.magnitude;

            if (len < 0.01f) continue; // skip zero-length segments

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject    wire = Instantiate(wirePrefab, parent);
            RectTransform rt   = wire.GetComponent<RectTransform>();
            Image         img  = wire.GetComponent<Image>();

            rt.anchoredPosition = s + dir * 0.5f;
            rt.sizeDelta        = new Vector2(len + 1f, active ? 4f : 2f);
            rt.localRotation    = Quaternion.Euler(0f, 0f, angle);

            if (img != null) img.color = color;
            spawnedWires.Add(rt);
        }
    }

    // ── Sprite / color helpers ────────────────────────────────────────
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
            case "AND":  return new Color(0f,    0.10f, 0.04f, 1f);
            case "OR":   return new Color(0f,    0.06f, 0.10f, 1f);
            case "NOT":  return new Color(0.10f, 0.02f, 0.02f, 1f);
            case "XOR":  return new Color(0.10f, 0.05f, 0f,    1f);
            case "NAND": return new Color(0.06f, 0f,    0.10f, 1f);
            case "NOR":  return new Color(0f,    0.10f, 0.10f, 1f);
            case "XNOR": return new Color(0.10f, 0.10f, 0f,    1f);
            default:     return colInactive;
        }
    }
}

// ── Bezier helper ─────────────────────────────────────────────────────
public static class BezierWire
{
    public static List<Vector2> GetPoints(Vector2 start, Vector2 end,
                                          int segments = 12)
    {
        var     pts = new List<Vector2>();
        float   dx  = Mathf.Abs(end.x - start.x);
        Vector2 cp1 = new Vector2(start.x + dx * 0.5f, start.y);
        Vector2 cp2 = new Vector2(end.x   - dx * 0.5f, end.y);

        for (int i = 0; i <= segments; i++)
        {
            float   t  = i / (float)segments;
            float   u  = 1f - t;
            Vector2 pt = u*u*u * start
                       + 3*u*u*t * cp1
                       + 3*u*t*t * cp2
                       + t*t*t   * end;
            pts.Add(pt);
        }
        return pts;
    }
}