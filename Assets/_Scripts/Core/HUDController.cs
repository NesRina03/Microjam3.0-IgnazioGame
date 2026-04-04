using System.Collections;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Stage & Timer")]
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI timerText;

    [Header("Instability Indicators")]
    public TextMeshProUGUI lightValueText;
    public TextMeshProUGUI oxygenValueText;
    public TextMeshProUGUI temperatureValueText;
    public TextMeshProUGUI pressureValueText;
    public TextMeshProUGUI radiationValueText;

    [Header("Colors")]
    public Color unstableColor = Color.red;
    public Color stableColor   = Color.green;

    [Header("Flicker interval (seconds)")]
    public float flickerInterval = 0.15f;

    // ── per-indicator value ranges (min, max) ──
    // feel free to tweak these to taste
    private static readonly (float min, float max)[] Ranges = {
        (35f, 75f),   // light
        (55f, 95f),   // oxygen
        (18f, 52f),   // temperature
        (28f, 68f),   // pressure
        (10f, 55f),   // radiation
    };

    // index order must match Ranges above
    private TextMeshProUGUI[] _texts;
    private float[]           _currentDisplayValues;
    private bool[]            _solved;

    // ── singleton so PuzzleManager can reach it without FindObjectOfType ──
    public static HUDController Instance { get; private set; }

void Awake()
{
    if (Instance == null) Instance = this;
    else if (Instance != this) Destroy(gameObject); // ← only destroy TRUE duplicates
}

public static void Register(HUDController hud)
{
    Instance = hud;
}
    void OnEnable()
    {
        _texts = new[]
        {
            lightValueText, oxygenValueText, temperatureValueText,
            pressureValueText, radiationValueText
        };

        if (_currentDisplayValues == null)
        {
            _currentDisplayValues = new float[5];
            _solved               = new bool[5];
            for (int i = 0; i < 5; i++)
                _currentDisplayValues[i] = Random.Range(Ranges[i].min, Ranges[i].max);
        }

        StartCoroutine(FlickerRoutine());
    }

    void OnDisable() => StopAllCoroutines();

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            for (int i = 0; i < 5; i++)
            {
                if (!_solved[i])
                    _currentDisplayValues[i] = Random.Range(Ranges[i].min, Ranges[i].max);
            }
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    void Update()
    {
        if (InstabilityManager.Instance == null) return;

        // stage
        stageText.text = "STAGE " + (InstabilityManager.Instance.currentStage + 1);

        // timer
        float t       = InstabilityManager.Instance.timeRemaining;
        int minutes   = Mathf.FloorToInt(t / 60f);
        int seconds   = Mathf.FloorToInt(t % 60f);
        timerText.text  = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = t < 60f ? Color.red : Color.white;

        // indicators — just display whatever the coroutine last wrote
        for (int i = 0; i < 5; i++)
        {
            if (_solved[i])
            {
                _texts[i].text  = "100";
                _texts[i].color = stableColor;
            }
            else
            {
                _texts[i].text  = Mathf.RoundToInt(_currentDisplayValues[i]).ToString();
                _texts[i].color = unstableColor;
            }
        }
    }

    // ── called by PuzzleManager.NotifySolved ──
    public void MarkLightSolved()       => MarkSolved(0);
    public void MarkOxygenSolved()      => MarkSolved(1);
    public void MarkTemperatureSolved() => MarkSolved(2);
    public void MarkPressureSolved()    => MarkSolved(3);
    public void MarkRadiationSolved()   => MarkSolved(4);

    void MarkSolved(int index)
    {
        if (_solved == null) return;   // safety guard
        _solved[index] = true;
    }
}