using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElectricalPuzzle : MonoBehaviour, IPuzzlePanel
{
    [Header("Breaker Sprites")]
    public Sprite spriteON;
    public Sprite spriteOFF;

    [Header("Breaker Buttons (assign the Button component directly)")]
    public Button[] breakerButtons;

    [Header("Breaker Images (assign the Image inside each Button)")]
    public Image[] breakerImages;

    [Header("Displays")]
    public TextMeshProUGUI currentDisplay;
    public TextMeshProUGUI targetDisplay;
    public TextMeshProUGUI timerDisplay;

    [Header("Status Panel")]
    public GameObject      statusPanel;
    public TextMeshProUGUI statusText;
    public Image           statusBg;

    [Header("Confirm Button")]
    public Button confirmButton;

    [Header("Settings")]
    [SerializeField] float  timeLimit  = 90f;
    [SerializeField] string factorName = "temp";

    // ── Colors ────────────────────────────────────────────────────
    static readonly Color colorSolved = new Color(0.00f, 1.00f, 0.53f);
    static readonly Color colorFailed = new Color(1.00f, 0.23f, 0.19f);
    static readonly Color colorWarm   = new Color(1.00f, 0.72f, 0.00f);
    static readonly Color colorTimer  = new Color(0.18f, 0.48f, 0.23f);
    static readonly Color colorUrgent = new Color(1.00f, 0.40f, 0.00f);
    static readonly Color colorDanger = new Color(1.00f, 0.23f, 0.19f);

    // ── State ─────────────────────────────────────────────────────
    int[]  breakerAmps   = new int[6];
    bool[] isON          = new bool[6];
    int    targetAmps;
    bool   isSolved;
    bool   isFailed;
    bool   viewOnly;
    float  timeRemaining;
    bool   initialized;

    Coroutine timerCoroutine;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        statusPanel.SetActive(false);
        initialized = true;
        StartRound();
    }

    void OnEnable()
    {
        if (!initialized) return;
        StartRound();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        timerCoroutine = null;
    }

    // ── IPuzzlePanel ──────────────────────────────────────────────
    public void SetViewOnly(bool isViewOnly)
    {
        viewOnly = isViewOnly;

        foreach (Button btn in breakerButtons)
            if (btn != null) btn.interactable = !isViewOnly;

        if (confirmButton != null)
            confirmButton.interactable = !isViewOnly;

        if (isViewOnly)
        {
            StopAllCoroutines();
            timerCoroutine = null;
            if (timerDisplay != null)
            {
                timerDisplay.text  = "SOLVED";
                timerDisplay.color = colorSolved;
            }
        }
    }

    // ── Round init ────────────────────────────────────────────────
    void StartRound()
    {
        isSolved      = false;
        isFailed      = false;
        viewOnly      = false;
        timeRemaining = timeLimit;

        // Wire confirm button
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirm);
            confirmButton.interactable = true;
        }

        // Wire breaker buttons directly
        for (int i = 0; i < breakerButtons.Length; i++)
        {
            if (breakerButtons[i] == null) continue;
            int idx = i;
            breakerButtons[i].onClick.RemoveAllListeners();
            breakerButtons[i].onClick.AddListener(() => ToggleBreaker(idx));
            breakerButtons[i].interactable = true;
        }

        if (statusPanel != null)
            statusPanel.SetActive(false);

        GenerateNewRound();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());
    }

    // ── Timer ─────────────────────────────────────────────────────
    IEnumerator RunTimer()
    {
        while (timeRemaining > 0f && !isSolved && !isFailed && !viewOnly)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }

        if (!isSolved && !isFailed && !viewOnly)
            StartCoroutine(TimeoutSequence());
    }

    void UpdateTimerDisplay()
    {
        if (timerDisplay == null) return;
        int secs = Mathf.CeilToInt(timeRemaining);
        timerDisplay.text  = "TIME: " + secs + "s";
        timerDisplay.color = timeRemaining > 30f ? colorTimer
                           : timeRemaining > 10f ? colorUrgent
                           : colorDanger;
    }

    // ── RNG Round ─────────────────────────────────────────────────
    void GenerateNewRound()
    {
        isON = new bool[6];

        for (int i = 0; i < breakerImages.Length; i++)
            if (breakerImages[i] != null)
                breakerImages[i].sprite = spriteOFF;

        for (int i = 0; i < 6; i++)
            breakerAmps[i] = Random.Range(2, 10);

        List<int> solution = PickRandomSubset();
        targetAmps = 0;
        foreach (int idx in solution)
            targetAmps += breakerAmps[idx];

        if (targetDisplay != null)
            targetDisplay.text = "TARGET: " + targetAmps + "A";

        UpdateCurrentDisplay();
    }

    List<int> PickRandomSubset()
    {
        List<int> indices = new List<int> { 0,1,2,3,4,5 };
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }
        List<int> chosen = new List<int>();
        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
            chosen.Add(indices[i]);
        return chosen;
    }

    // ── Toggle ────────────────────────────────────────────────────
    void ToggleBreaker(int index)
    {
        if (isSolved || isFailed || viewOnly) return;
        if (index >= breakerImages.Length) return;

        isON[index] = !isON[index];

        if (breakerImages[index] != null)
            breakerImages[index].sprite = isON[index] ? spriteON : spriteOFF;

        StartCoroutine(PunchScale(breakerButtons[index].transform));
        UpdateCurrentDisplay();
    }

    // ── Display ───────────────────────────────────────────────────
    void UpdateCurrentDisplay()
    {
        if (currentDisplay == null) return;
        int total = GetTotal();
        currentDisplay.text  = "CURRENT: " + total + "A";
        currentDisplay.color = total == targetAmps          ? colorSolved
                             : Mathf.Abs(total-targetAmps) <= 3 ? colorWarm
                             : Color.white;
    }

    int GetTotal()
    {
        int sum = 0;
        for (int i = 0; i < 6; i++)
            if (isON[i]) sum += breakerAmps[i];
        return sum;
    }

    // ── Confirm ───────────────────────────────────────────────────
    void OnConfirm()
    {
        if (isSolved || isFailed || viewOnly) return;
        if (GetTotal() == targetAmps)
            StartCoroutine(SolveSequence());
        else
            StartCoroutine(FailSequence());
    }

    // ── Solve ─────────────────────────────────────────────────────
    IEnumerator SolveSequence()
    {
        isSolved = true;
        StopAllCoroutines();

        if (timerDisplay != null)
        {
            timerDisplay.text  = "COMPLETE";
            timerDisplay.color = colorSolved;
        }

        ShowStatus("POWER RESTORED", colorSolved);
        if (currentDisplay != null) currentDisplay.color = colorSolved;

        yield return new WaitForSeconds(2f);
        PuzzleManager.Instance.NotifySolved(factorName);
    }

    // ── Fail ──────────────────────────────────────────────────────
    IEnumerator FailSequence()
    {
        isFailed = true;
        ShowStatus(GetTotal() + "A / " + targetAmps + "A — WRONG", colorFailed);
        yield return StartCoroutine(ShakePanel(statusPanel.transform));
        PuzzleManager.Instance.NotifyFailed(0.15f);
        yield return new WaitForSeconds(1.2f);

        statusPanel.SetActive(false);
        isFailed      = false;
        timeRemaining = timeLimit;
        GenerateNewRound();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());
    }

    // ── Timeout ───────────────────────────────────────────────────
    IEnumerator TimeoutSequence()
    {
        isFailed = true;
        ShowStatus("TIME UP — RESETTING", colorFailed);
        yield return StartCoroutine(ShakePanel(statusPanel.transform));
        PuzzleManager.Instance.NotifyFailed(0.25f);
        yield return new WaitForSeconds(1.5f);

        statusPanel.SetActive(false);
        isFailed      = false;
        timeRemaining = timeLimit;
        GenerateNewRound();

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());
    }

    // ── Status ────────────────────────────────────────────────────
    void ShowStatus(string msg, Color col)
    {
        if (statusPanel == null) return;
        statusPanel.SetActive(true);
        if (statusText != null) { statusText.text = msg; statusText.color = col; }
        if (statusBg   != null) statusBg.color = new Color(col.r, col.g, col.b, 0.15f);
    }

    // ── Juice ─────────────────────────────────────────────────────
    IEnumerator PunchScale(Transform t)
    {
        Vector3 origin = t.localScale;
        t.localScale   = origin * 1.2f;
        float e = 0f;
        while (e < 0.15f)
        {
            t.localScale = Vector3.Lerp(t.localScale, origin, e / 0.15f);
            e += Time.deltaTime;
            yield return null;
        }
        t.localScale = origin;
    }

    IEnumerator ShakePanel(Transform t)
    {
        Vector3 origin = t.localPosition;
        float dur = 0.35f, e = 0f;
        while (e < dur)
        {
            t.localPosition = origin + new Vector3(Random.Range(-8f, 8f), 0, 0);
            e += Time.deltaTime;
            yield return null;
        }
        t.localPosition = origin;
    }
}