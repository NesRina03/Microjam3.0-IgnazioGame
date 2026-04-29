using UnityEngine;
using UnityEngine.Events;

public class InstabilityManager : MonoBehaviour
{
    public static InstabilityManager Instance;

    [Header("Stage Timer")]
    [SerializeField] float stageDurationSeconds = 60f;

    [Header("Reward % per puzzle solved (per stage)")]
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
        currentStage  = 0;
        running       = true;
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
        timeRemaining = stageDurationSeconds;
        currentStageMax = stageDurationSeconds * maxExtensionMultiplier;
        OnStageChanged?.Invoke(stage + 1);
    }

    void AdvanceStage()
    {
        int nextStage = currentStage + 1;
        if (nextStage >= extensionPercents.Length)
        {
            running = false;
            OnCreatureFreed?.Invoke(); // creature handles delay then calls TriggerGameOver
        }
        else StartStage(nextStage);
    }

    public void OnPuzzleSolved()
    {
        puzzlesSolved++;
        AddTime(stageDurationSeconds * extensionPercents[currentStage]);

        if (puzzlesSolved >= 3)
        {
            running = false;
            OnAllPuzzlesSolved?.Invoke();
            GameManager.Instance.TriggerWin();
        }
    }

    public void AddTime(float amountSeconds)
    {
        if (!running) return;

        timeRemaining = Mathf.Min(timeRemaining + amountSeconds, currentStageMax);
    }

    public bool SpendTime(float amountSeconds)
    {
        if (!running) return false;

        if (timeRemaining < amountSeconds) return false;

        timeRemaining -= amountSeconds;
        return true;
    }

    public float GetStageProgress()
    {
        return Mathf.Clamp01(timeRemaining / stageDurationSeconds);
    }
}