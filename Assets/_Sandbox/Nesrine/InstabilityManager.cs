using UnityEngine;
using UnityEngine.Events;

public class InstabilityManager : MonoBehaviour
{
    public static InstabilityManager Instance;

    [Header("Stage Base Times (seconds)")]
    public float[] stageBaseTimes = { 900f, 600f, 420f }; // 15, 10, 7 min

    [Header("Extension % per puzzle solved (per stage)")]
    public float[] extensionPercents = { 0.10f, 0.20f, 0.30f };

    [Header("Cap: max multiplier on base time")]
    public float maxExtensionMultiplier = 2f; // max 2x base time

    // Events other scripts listen to
    public UnityEvent<int> OnStageChanged;   // broadcasts new stage number
    public UnityEvent OnCreatureFreed;        // creature breaks tank → lose
    public UnityEvent OnAllPuzzlesSolved;     // all 5 solved → win

    [HideInInspector] public int currentStage = 0; // 0-indexed (0=stage1)
    [HideInInspector] public float timeRemaining;
    [HideInInspector] public int puzzlesSolved = 0;

    private float currentStageMax; // current cap for this stage
    private bool running = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartStage(0);
        running = true;
    }

    void Update()
    {
        if (!running) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
            AdvanceStage();
    }

    void StartStage(int stage)
    {
        currentStage = stage;
        timeRemaining = stageBaseTimes[stage];
        currentStageMax = stageBaseTimes[stage] * maxExtensionMultiplier;
        OnStageChanged?.Invoke(stage + 1); // send 1-indexed to HUD
    }

    void AdvanceStage()
    {
        int nextStage = currentStage + 1;

        if (nextStage >= stageBaseTimes.Length)
        {
            // Reached stage 4 → creature breaks free
            running = false;
            OnCreatureFreed?.Invoke();
            //GameManager.Instance.TriggerGameOver();
            Debug.Log("Game Over triggered");

        }
        else
        {
            StartStage(nextStage);
        }
    }

    // Call this from any puzzle when solved
    public void OnPuzzleSolved()
    {
        puzzlesSolved++;

        // Extend current stage time
        float extension = stageBaseTimes[currentStage] * extensionPercents[currentStage];
        timeRemaining = Mathf.Min(timeRemaining + extension, currentStageMax);

        // Check win condition
        if (puzzlesSolved >= 5)
        {
            running = false;
            OnAllPuzzlesSolved?.Invoke();
            //GameManager.Instance.TriggerGameOver(); // we'll add Win state next
            Debug.Log("Game Over triggered");

        }

        Debug.Log($"Puzzle solved! Stage {currentStage + 1} time extended by {extension}s. Remaining: {timeRemaining}s");
    }

    // Handy for HUD — returns 0 to 1
    public float GetStageProgress()
    {
        return timeRemaining / stageBaseTimes[currentStage];
    }
}