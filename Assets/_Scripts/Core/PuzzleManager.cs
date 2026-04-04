using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] MonoBehaviour fpsController;
    [SerializeField] Transform playerCameraTransform;

    [Header("Puzzle Panels")]
    [SerializeField] GameObject lightPuzzlePanel;
    [SerializeField] GameObject tempPuzzlePanel;
    [SerializeField] GameObject pressurePuzzlePanel;
    [SerializeField] GameObject oxygenPuzzlePanel;
    [SerializeField] GameObject radiationPuzzlePanel;

    [Header("Overlay")]
    [SerializeField] GameObject backgroundDim;

    // Environmental factors
    [HideInInspector] public float flight     = 1f;
    [HideInInspector] public float ftemp      = 1f;
    [HideInInspector] public float fpressure  = 1f;
    [HideInInspector] public float foxygen    = 1f;
    [HideInInspector] public float fradiation = 1f;

    public bool IsPuzzleOpen { get; private set; }

    GameObject currentPanel;
    PuzzleTerminal activeTerminal;

    void Awake()
{
    Debug.Log("PuzzleManager Awake — Instance is: " + (Instance == null ? "null" : "already set"));
    
    if (Instance == null)
    {
        Instance = this;
        Debug.Log("PuzzleManager Instance SET successfully");
    }
    else
    {
        Debug.Log("Duplicate PuzzleManager found — destroying this one");
        Destroy(gameObject);
    }
}

    public void OpenPuzzle(PuzzleTerminal terminal)
{
    if (IsPuzzleOpen) return;

    activeTerminal = terminal;
    IsPuzzleOpen = true;

    fpsController.enabled = false;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    if (backgroundDim != null)
        backgroundDim.SetActive(true);

    currentPanel = GetPanel(terminal.FactorName);

    // ADD THIS
    Debug.Log("Opening puzzle for factor: " + terminal.FactorName 
              + " | Panel found: " + (currentPanel != null));

    if (currentPanel != null)
        currentPanel.SetActive(true);
}

    public void ClosePuzzle()
    {
        if (!IsPuzzleOpen) return;

        // Hide panel and overlay
        if (currentPanel != null)
            currentPanel.SetActive(false);

        if (backgroundDim != null)
            backgroundDim.SetActive(false);

        // Unfreeze player
        fpsController.enabled = true;

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        IsPuzzleOpen = false;
        currentPanel = null;

        if (activeTerminal != null)
            activeTerminal.OnClosed();

        activeTerminal = null;

        fpsController.enabled = true;        
        GameManager.Instance.RestorePlayState();
    }

   public void NotifySolved(string factorName)
{
    Debug.Log("NotifySolved called with: " + factorName);
    Debug.Log("HUDController.Instance is null: " + (HUDController.Instance == null));
    
    SetFactor(factorName, 1f);
    InstabilityManager.Instance.OnPuzzleSolved();

    var hud = HUDController.Instance;
    if (hud != null)
    {
        Debug.Log("Calling Mark on HUD for: " + factorName);
        if (factorName == "light")      hud.MarkLightSolved();
        if (factorName == "oxygen")     hud.MarkOxygenSolved();
        if (factorName == "temp")       hud.MarkTemperatureSolved();
        if (factorName == "pressure")   hud.MarkPressureSolved();
        if (factorName == "radiation")  hud.MarkRadiationSolved();
    }

    if (activeTerminal != null) activeTerminal.MarkSolved();
    ClosePuzzle();
}
    public void NotifyFailed(float penalty = 0.15f)
    {
        if (activeTerminal != null)
            ApplyPenalty(activeTerminal.FactorName, penalty);
    }

    GameObject GetPanel(string factorName)
    {
        switch (factorName)
        {
            case "light":     return lightPuzzlePanel;
            case "temp":      return tempPuzzlePanel;
            case "pressure":  return pressurePuzzlePanel;
            case "oxygen":    return oxygenPuzzlePanel;
            case "radiation": return radiationPuzzlePanel;
            default:          return null;
        }
    }

    public void SetFactor(string name, float value)
    {
        value = Mathf.Clamp01(value);
        switch (name)
        {
            case "light":     flight     = value; break;
            case "temp":      ftemp      = value; break;
            case "pressure":  fpressure  = value; break;
            case "oxygen":    foxygen    = value; break;
            case "radiation": fradiation = value; break;
        }
    }

    public void ApplyPenalty(string name, float amount)
    {
        switch (name)
        {
            case "light":     flight     = Mathf.Max(0, flight    - amount); break;
            case "temp":      ftemp      = Mathf.Max(0, ftemp      - amount); break;
            case "pressure":  fpressure  = Mathf.Max(0, fpressure  - amount); break;
            case "oxygen":    foxygen    = Mathf.Max(0, foxygen    - amount); break;
            case "radiation": fradiation = Mathf.Max(0, fradiation - amount); break;
        }
    }
public void ForceReset()
{
    if (currentPanel != null) currentPanel.SetActive(false);
    if (backgroundDim != null) backgroundDim.SetActive(false);
    currentPanel   = null;
    activeTerminal = null;
    IsPuzzleOpen   = false;
    fpsController.enabled = true;
}
    public float GetInstability() =>
        1f - (flight + ftemp + fpressure + foxygen + fradiation) / 5f;
}