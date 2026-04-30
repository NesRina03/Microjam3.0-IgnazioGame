using UnityEngine;
using TMPro;
using System.Collections;

public class puzzle44  : MonoBehaviour
{
    // ── Answer (spaces/caps ignored when checking) ─────────────────────
    private const string CORRECT = "lookthroughthelens";
    private const int    ANSWER_LENGTH = 18; // must match CORRECT length

    // ── UI References ──────────────────────────────────────────────────
    [Header("UI — drag from Hierarchy")]
    [SerializeField] TextMeshProUGUI inputDisplay;   // the InputDisplay TMP
    [SerializeField] TextMeshProUGUI feedbackText;   // FeedbackText TMP
    [SerializeField] TextMeshProUGUI titleText;      // TitleText TMP (optional)

    // ── State ──────────────────────────────────────────────────────────
    private string  typed        = "";   // what player has typed so far
    private bool    solved       = false;
    private bool    isOpen       = false;

    // ── Called when Canvas activates ───────────────────────────────────
    void OnEnable()
    {
        typed        = "";
        solved       = false;
        isOpen       = true;

        if (feedbackText) feedbackText.text = "";
        RefreshDisplay();

        // Lock cursor — player uses keyboard
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Pause time so creature stops
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        isOpen = false;
    }

    // ── Every frame: listen to keyboard ───────────────────────────────
    void Update()
    {
        if (!isOpen || solved) return;

        // Escape = close panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
            return;
        }

        // Enter = submit
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TrySubmit();
            return;
        }

        // Read typed characters from Unity's input buffer
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // backspace
            {
                if (typed.Length > 0)
                    typed = typed.Substring(0, typed.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                // handled above
            }
            else if (char.IsLetter(c))
            {
                // Only accept up to answer length
                if (typed.Length < ANSWER_LENGTH)
                    typed += char.ToUpper(c);
            }
        }

        RefreshDisplay();
    }

    // ── Display: shows typed letters + remaining underscores ──────────
    void RefreshDisplay()
    {
        if (inputDisplay == null) return;

        string display = "";

        for (int i = 0; i < ANSWER_LENGTH; i++)
        {
            if (i < typed.Length)
            {
                // Typed letter — white
                display += "<color=#FFFFFF>" + typed[i] + "</color>";
            }
            else if (i == typed.Length)
            {
                // Blinking cursor position — green
                display += "<color=#00FF66>_</color>";
            }
            else
            {
                // Empty slot — dim underscore
                display += "<color=#2A4A2A>_</color>";
            }

            // Space between every character for readability
            if (i < ANSWER_LENGTH - 1)
                display += " ";
        }

        inputDisplay.text = display;
    }

    // ── Submit ─────────────────────────────────────────────────────────
    void TrySubmit()
    {
        if (typed.Length == 0) return;

        // Normalize: remove spaces, lowercase
        string normalized = typed.ToLower().Trim();

        if (normalized == CORRECT)
        {
            StartCoroutine(CorrectSequence());
        }
        else
        {
            StartCoroutine(WrongSequence());
        }
    }

    // ── Correct ────────────────────────────────────────────────────────
    IEnumerator CorrectSequence()
    {
        solved = true;

        if (feedbackText)
        {
            feedbackText.color = new Color(0f, 1f, 0.4f);
            feedbackText.text  = "DECRYPTED — FIND THE MICROSCOPE";
        }

        // Turn all underscores green
        if (inputDisplay)
            inputDisplay.color = new Color(0f, 1f, 0.4f);

        yield return new WaitForSecondsRealtime(2.5f);

        // Tell board it's solved
        PigpenBoardController.Instance?.OnPuzzleSolved();

        // Unlock microscope
        MicroscopeController.Instance?.Unlock();

        ClosePuzzle();
    }

    // ── Wrong ──────────────────────────────────────────────────────────
    IEnumerator WrongSequence()
    {
        if (feedbackText)
        {
            feedbackText.color = new Color(1f, 0.2f, 0.1f);
            feedbackText.text  = "INCORRECT — CHECK THE SYMBOLS AGAIN";
        }

        yield return StartCoroutine(ShakeDisplay());

        yield return new WaitForSecondsRealtime(1.2f);

        if (feedbackText && !solved)
            feedbackText.text = "";
    }

    IEnumerator ShakeDisplay()
    {
        if (inputDisplay == null) yield break;
        RectTransform rt  = inputDisplay.GetComponent<RectTransform>();
        Vector3       ori = rt.localPosition;

        for (int i = 0; i < 10; i++)
        {
            rt.localPosition = ori + new Vector3(Random.Range(-7f, 7f), 0f, 0f);
            yield return new WaitForSecondsRealtime(0.03f);
        }
        rt.localPosition = ori;
    }

    // ── Close ──────────────────────────────────────────────────────────
    void ClosePuzzle()
    {
        isOpen         = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        PigpenBoardController.Instance?.ClosePuzzle();
    }
}