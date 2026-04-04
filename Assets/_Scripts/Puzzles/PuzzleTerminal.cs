using UnityEngine;

public class PuzzleTerminal : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string factorName;
    [SerializeField] string promptLabel = "Inspect Panel";

    [Header("Screen Glow")]
    [SerializeField] Renderer screenRenderer;
    [SerializeField] Color activeColor   = new Color(0f, 1f, 0.5f);
    [SerializeField] Color solvedColor   = new Color(0f, 0.8f, 0.2f);
    [SerializeField] Color inactiveColor = new Color(0.1f, 0.2f, 0.1f);

    public string FactorName  => factorName;
    public string PromptLabel => promptLabel;
    public bool   IsSolved    { get; private set; }

    void Start()
    {
        SetScreenColor(inactiveColor);
    }

    public void OnClosed()
    {
        SetScreenColor(IsSolved ? solvedColor : inactiveColor);
    }

    public void MarkSolved()
    {
        IsSolved = true;
        SetScreenColor(solvedColor);
    }

    void SetScreenColor(Color c)
    {
        if (screenRenderer == null) return;
        screenRenderer.material.SetColor("_EmissionColor", c * 2f);
        screenRenderer.material.EnableKeyword("_EMISSION");
    }
}