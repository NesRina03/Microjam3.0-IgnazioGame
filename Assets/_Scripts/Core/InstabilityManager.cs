using UnityEngine;
using UnityEngine.Events;

public class InstabilityManager : MonoBehaviour
{
    public static InstabilityManager Instance;

    [Header("Stage Base Times (seconds)")]
    public float[] stageBaseTimes = { 900f, 600f, 420f };

    [Header("Extension % per puzzle solved (per stage)")]
    public float[] extensionPercents = { 0.10f, 0.20f, 0.30f };

    [Header("Cap: max multiplier on base time")]
    public float maxExtensionMultiplier = 2f;

    public UnityEvent<int> OnStageChanged;
    public UnityEvent OnCreatureFreed;
    public UnityEvent OnAllPuzzlesSolved;

    [HideInInspector] public int currentStage = 0;
    [HideInInspector] public float timeRemaining;
    [HideInInspector] public int puzzlesSolved = 0;

    private float currentStageMax;
    private bool running = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        puzzlesSolved = 0;
        currentStage = 0;
        running = true;
        StartStage(0);
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
        OnStageChanged?.Invoke(stage + 1);
    }

    void AdvanceStage()
    {
        int nextStage = currentStage + 1;
        if (nextStage >= stageBaseTimes.Length)
        {
            running = false;
            OnCreatureFreed?.Invoke();
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            StartStage(nextStage);
        }
    }

    public void OnPuzzleSolved()
    {
        puzzlesSolved++;
        float extension = stageBaseTimes[currentStage] * extensionPercents[currentStage];
        timeRemaining = Mathf.Min(timeRemaining + extension, currentStageMax);

        if (puzzlesSolved >= 3)
        {
            running = false;
            OnAllPuzzlesSolved?.Invoke();
            GameManager.Instance.TriggerWin();
        }
    }

    public float GetStageProgress()
    {
        return timeRemaining / stageBaseTimes[currentStage];
    }
}