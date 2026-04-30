using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoorPuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject doorPanel;
    [SerializeField] TextMeshProUGUI[] codeCells = new TextMeshProUGUI[4];
    [SerializeField] Button[] numberButtons;
    [SerializeField] Button deleteButton;
    [SerializeField] Button submitButton;
    [SerializeField] TextMeshProUGUI feedbackText;

    [Header("Settings")]
    [SerializeField] string correctCode = "2604";
    [SerializeField] int maxCodeLength = 4;

    private string currentInput = "";

    void Start()
    {
        if (doorPanel == null) doorPanel = gameObject;
        Debug.Log($"[DoorPuzzle] Start: doorPanel = {doorPanel.name}, gameObject.activeSelf = {gameObject.activeSelf}");

        // Wire number buttons
        if (numberButtons != null && numberButtons.Length > 0)
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int digit = i;
                if (numberButtons[i] != null)
                    numberButtons[i].onClick.AddListener(() => OnNumberPressed(digit.ToString()));
            }
        }

        // Wire delete button
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeletePressed);

        // Wire submit button
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitPressed);

        UpdateDisplay();
    }

    void Update()
    {
        // Handle keyboard input for door code
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c) && currentInput.Length < maxCodeLength)
            {
                OnNumberPressed(c.ToString());
            }
            else if (c == '\b') // backspace
            {
                OnDeletePressed();
            }
            else if (c == '\n' || c == '\r') // enter
            {
                OnSubmitPressed();
            }
        }
    }

    void OnNumberPressed(string digit)
    {
        Debug.Log($"[DoorPuzzle] OnNumberPressed: {digit}");
        if (currentInput.Length < maxCodeLength)
        {
            currentInput += digit;
            UpdateDisplay();
        }
    }

    void OnDeletePressed()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
    }

    void OnSubmitPressed()
    {
        Debug.Log($"[DoorPuzzle] OnSubmitPressed: currentInput = '{currentInput}', correctCode = '{correctCode}'");
        if (currentInput == correctCode)
        {
            UnlockDoor();
        }
        else
        {
            ShowFeedback("Incorrect code. Try again.", Color.red, 2f);
            currentInput = "";
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        for (int i = 0; i < 4; i++)
        {
            if (codeCells[i] != null)
                codeCells[i].text = (i < currentInput.Length) ? currentInput[i].ToString() : "";
        }
    }

    void ShowFeedback(string message, Color color, float duration)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            StartCoroutine(ClearFeedbackAfterDelay(duration));
        }
    }

    System.Collections.IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
            feedbackText.text = "";
    }

    void UnlockDoor()
    {
        ShowFeedback("Door unlocked!", Color.green, 1f);
        
        // Disable input
        SetInputEnabled(false);
        
        // Wait a moment then trigger win
        StartCoroutine(TriggerWinAfterDelay(1f));
    }

    void SetInputEnabled(bool enabled)
    {
        if (numberButtons != null)
        {
            foreach (Button btn in numberButtons)
            {
                if (btn != null) btn.interactable = enabled;
            }
        }

        if (deleteButton != null) deleteButton.interactable = enabled;
        if (submitButton != null) submitButton.interactable = enabled;
    }

    System.Collections.IEnumerator TriggerWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.TriggerWin();
    }
}
