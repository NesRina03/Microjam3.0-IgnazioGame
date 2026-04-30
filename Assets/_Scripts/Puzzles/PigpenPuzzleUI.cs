using UnityEngine;
using TMPro;
using System.Collections;

public class PigpenPuzzleUI : MonoBehaviour
{
    private const string CORRECT = "lookthroughthelens";
    private const int    ANSWER_LENGTH = 18;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI inputDisplay;
    [SerializeField] TextMeshProUGUI feedbackText;
    [SerializeField] TextMeshProUGUI titleText;

    private string typed  = "";
    private bool   solved = false;
    private bool   isOpen = false;

    void OnEnable()
    {
        typed  = "";
        solved = false;
        isOpen = true;
        if (feedbackText) feedbackText.text = "";
        if (inputDisplay) inputDisplay.color = Color.white;
        RefreshDisplay();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        Time.timeScale   = 0f;
    }

    void OnDisable() { isOpen = false; }

    void Update()
    {
        if (!isOpen || solved) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { ClosePuzzle(); return; }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) { TrySubmit(); return; }
        foreach (char c in Input.inputString)
        {
            if (c == '\b') { if (typed.Length > 0) typed = typed.Substring(0, typed.Length - 1); }
            else if (c != '\n' && c != '\r' && char.IsLetter(c))
                if (typed.Length < ANSWER_LENGTH) typed += char.ToUpper(c);
        }
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (inputDisplay == null) return;
        string display = "";
        for (int i = 0; i < ANSWER_LENGTH; i++)
        {
            if (i < typed.Length) display += "<color=#FFFFFF>" + typed[i] + "</color>";
            else if (i == typed.Length) display += "<color=#00FF66>_</color>";
            else display += "<color=#2A4A2A>_</color>";
            if (i < ANSWER_LENGTH - 1) display += " ";
        }
        inputDisplay.text = display;
    }

    void TrySubmit()
    {
        if (typed.Length == 0) return;
        if (typed.ToLower().Trim() == CORRECT) StartCoroutine(CorrectSequence());
        else StartCoroutine(WrongSequence());
    }

    IEnumerator CorrectSequence()
    {
        solved = true;
        if (feedbackText) { feedbackText.color = new Color(0f,1f,0.4f); feedbackText.text = "DECRYPTED — FIND THE MICROSCOPE"; }
        if (inputDisplay) inputDisplay.color = new Color(0f,1f,0.4f);
        yield return new WaitForSecondsRealtime(2.5f);
        PigpenBoardController.Instance?.OnPuzzleSolved();
        MicroscopeController.Instance?.Unlock();
        ClosePuzzle();
    }

    IEnumerator WrongSequence()
    {
        if (feedbackText) { feedbackText.color = new Color(1f,0.2f,0.1f); feedbackText.text = "INCORRECT — CHECK THE SYMBOLS AGAIN"; }
        yield return StartCoroutine(ShakeDisplay());
        yield return new WaitForSecondsRealtime(1.2f);
        if (feedbackText && !solved) feedbackText.text = "";
    }

    IEnumerator ShakeDisplay()
    {
        if (inputDisplay == null) yield break;
        RectTransform rt = inputDisplay.GetComponent<RectTransform>();
        Vector3 ori = rt.localPosition;
        for (int i = 0; i < 10; i++) { rt.localPosition = ori + new Vector3(Random.Range(-7f,7f),0f,0f); yield return new WaitForSecondsRealtime(0.03f); }
        rt.localPosition = ori;
    }

    public void ClosePuzzle()
    {
        isOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PigpenBoardController.Instance?.ClosePuzzle();
    }
}