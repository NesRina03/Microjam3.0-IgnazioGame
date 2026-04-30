using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WordleScript : MonoBehaviour
{
    [Header("Factor")]
[SerializeField] string factorName = "oxygen"; // set in Inspector to whichever factor Wordle is assigned

    [Header("Hint")]
    [SerializeField] Button hintButton;
    [SerializeField] TextMeshProUGUI hintStatusText;
    [SerializeField] float hintCostSeconds = 15f;
    [SerializeField] int hintRevealCount = 2;


    [Header("Grid — 6 rows x 5 cols")]
    private TextMeshProUGUI[,] grid = new TextMeshProUGUI[6, 5];
    public GameObject[] rowObjects; // drag 6 row objects in Inspector

    [Header("Keyboard buttons")]
    public Button[] letterButtons; // 26 buttons A-Z

    [Header("Colors")]
    public Color correctColor   = new Color(0.18f, 0.62f, 0.35f); // green
    public Color presentColor   = new Color(0.79f, 0.69f, 0.11f); // yellow
    public Color absentColor    = new Color(0.25f, 0.25f, 0.28f); // grey
    public Color defaultColor   = new Color(0.13f, 0.15f, 0.18f); // dark

    private string[] wordBank = {
        "FLASK", "VENOM", "TOXIC", "SHOCK", "PROBE",
        "LASER", "VIRAL", "DECAY", "CRUEL", "CELLS",
        "NERVE", "AMBER", "GLASS", "STEAM", "STEEL",
        "CRACK", "SURGE", "BLEED", "GHOST", "PANIC"
    };

    private string targetWord;
    private string currentGuess = "";
    private int currentRow = 0;
    private bool gameOver = false;
    private bool hintUsed = false;
    private bool[,] hintedCells = new bool[6, 5];
    private Color hintButtonEnabledColor;
    private bool hintButtonColorCached = false;
    private Coroutine hintStatusCoroutine = null;

    void Start()
    {
        BuildGrid();
        CacheHintButtonColor();
        ResetGame();
    }

    void OnEnable()
    {
        // Reset every time the panel is shown (handles re-open after failure)
        if (grid[0, 0] != null) // grid already built
            ResetGame();
    }

    public void ResetGame()
    {
        targetWord   = wordBank[Random.Range(0, wordBank.Length)];
        currentGuess = "";
        currentRow   = 0;
        gameOver     = false;
        hintUsed     = false;
        hintedCells  = new bool[6, 5];

        SetHintButtonState(true);
        SetHintStatus("");

        // Clear all cells
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 5; c++)
                if (grid[r, c] != null) grid[r, c].text = "";

        // Reset row background colors
        for (int r = 0; r < 6; r++)
        {
            foreach (var img in rowObjects[r].GetComponentsInChildren<Image>())
                img.color = defaultColor;
        }

        // Reset keyboard button colors
        if (letterButtons != null)
            foreach (Button btn in letterButtons)
                if (btn != null) btn.GetComponent<Image>().color = defaultColor;
    }

    void Update()
    {
        if (gameOver) return;

        // Physical keyboard input
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // backspace
                DeleteLetter();
            else if (c == '\n' || c == '\r') // enter
                SubmitGuess();
            else if (char.IsLetter(c) && currentGuess.Length < 5)
                AddLetter(char.ToUpper(c).ToString());
        }
    }

    void BuildGrid()
    {
        for (int row = 0; row < 6; row++)
        {
            TextMeshProUGUI[] cells = rowObjects[row].GetComponentsInChildren<TextMeshProUGUI>();
            for (int col = 0; col < 5; col++)
                grid[row, col] = cells[col];
        }
    }

    // Called by on-screen letter buttons
    public void OnLetterButtonPressed(string letter)
    {
        if (gameOver || !HasEditableSlot()) return;
        AddLetter(letter);
    }

    // Called by on-screen backspace button
    public void OnBackspacePressed()
    {
        DeleteLetter();
    }

    // Called by on-screen enter button
    public void OnEnterPressed()
    {
        SubmitGuess();
    }

    public void OnHintButtonPressed()
    {
        if (gameOver || hintUsed) return;

        int revealed = 0;
        int safety = 0;
        while (revealed < hintRevealCount && safety < 20)
        {
            int column = Random.Range(0, 5);
            safety++;

            if (grid[currentRow, column] == null) continue;
            if (hintedCells[currentRow, column]) continue;
            if (grid[currentRow, column].text != "") continue;

            grid[currentRow, column].text = targetWord[column].ToString();
            hintedCells[currentRow, column] = true;
            revealed++;
        }

        if (revealed == 0)
            return;

        if (InstabilityManager.Instance == null || !InstabilityManager.Instance.SpendTime(hintCostSeconds))
        {
            for (int column = 0; column < 5; column++)
            {
                if (!hintedCells[currentRow, column]) continue;
                if (grid[currentRow, column] == null) continue;

                grid[currentRow, column].text = "";
                hintedCells[currentRow, column] = false;
            }

            RefreshCurrentGuess();
            ShowHintStatusTemporary("NOT ENOUGH TIME", 3f);
            return;
        }

        RefreshCurrentGuess();
        hintUsed = true;
        ShowHintStatusTemporary($"HINT USED: -{(int)hintCostSeconds}s", 3f);

        SetHintButtonState(false);
    }

    void AddLetter(string letter)
    {
        int column = GetNextEditableColumn();
        if (column < 0) return;

        grid[currentRow, column].text = letter;
        RefreshCurrentGuess();
    }

    void DeleteLetter()
    {
        int column = GetLastEditableFilledColumn();
        if (column < 0) return;

        grid[currentRow, column].text = "";
        RefreshCurrentGuess();
    }

    void SubmitGuess()
    {
        RefreshCurrentGuess();

        if (!IsCurrentRowComplete()) return; // not enough letters

        StartCoroutine(RevealRow(currentRow, currentGuess));

        if (currentGuess == targetWord)
        {
            gameOver = true;
            Invoke(nameof(PuzzleSolved), 1.5f); // small delay then solve
            return;
        }

        currentRow++;
        currentGuess = "";

        if (currentRow >= 6)
        {
            gameOver = true;
            Debug.Log($"Out of attempts. Word was {targetWord}");
            // optionally show the answer on screen
        }
    }

    void RefreshCurrentGuess()
    {
        currentGuess = "";

        for (int column = 0; column < 5; column++)
        {
            if (grid[currentRow, column] == null) continue;
            currentGuess += grid[currentRow, column].text;
        }
    }

    int GetNextEditableColumn()
    {
        for (int column = 0; column < 5; column++)
        {
            if (grid[currentRow, column] == null) continue;
            if (hintedCells[currentRow, column]) continue;
            if (grid[currentRow, column].text == "") return column;
        }

        return -1;
    }

    bool HasEditableSlot()
    {
        return GetNextEditableColumn() >= 0;
    }

    bool IsCurrentRowComplete()
    {
        for (int column = 0; column < 5; column++)
        {
            if (grid[currentRow, column] == null) return false;
            if (grid[currentRow, column].text == "") return false;
        }

        return true;
    }

    void CacheHintButtonColor()
    {
        if (hintButton == null || hintButton.targetGraphic == null || hintButtonColorCached) return;

        hintButtonEnabledColor = hintButton.targetGraphic.color;
        hintButtonColorCached = true;
    }

    void SetHintButtonState(bool enabled)
    {
        if (hintButton == null) return;

        CacheHintButtonColor();
        hintButton.interactable = enabled;

        if (hintButton.targetGraphic != null && hintButtonColorCached)
            hintButton.targetGraphic.color = enabled ? hintButtonEnabledColor : new Color(0.45f, 0.45f, 0.45f, hintButtonEnabledColor.a);

        foreach (TextMeshProUGUI label in hintButton.GetComponentsInChildren<TextMeshProUGUI>(true))
            label.color = enabled ? Color.white : new Color(0.55f, 0.55f, 0.55f, label.color.a);
    }

    void SetHintStatus(string message)
    {
        if (hintStatusText == null) return;
        hintStatusText.text = message;

        if (string.IsNullOrEmpty(message))
            return;

        hintStatusText.color = message == "NOT ENOUGH TIME"
            ? new Color(1f, 0.55f, 0.25f)
            : new Color(0.95f, 0.82f, 0.35f);
    }

    void ShowHintStatusTemporary(string message, float seconds)
    {
        if (hintStatusText == null) return;

        // stop any existing clear coroutine
        if (hintStatusCoroutine != null)
        {
            StopCoroutine(hintStatusCoroutine);
            hintStatusCoroutine = null;
        }

        SetHintStatus(message);
        hintStatusCoroutine = StartCoroutine(ClearHintStatusAfterSeconds(seconds));
    }

    IEnumerator ClearHintStatusAfterSeconds(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        SetHintStatus("");
        hintStatusCoroutine = null;
    }

    int GetLastEditableFilledColumn()
    {
        for (int column = 4; column >= 0; column--)
        {
            if (grid[currentRow, column] == null) continue;
            if (hintedCells[currentRow, column]) continue;
            if (grid[currentRow, column].text != "") return column;
        }

        return -1;
    }

    IEnumerator RevealRow(int row, string guess)
    {
        // Build result array first
        string target = targetWord;
        string[] result = new string[5];
        bool[] targetUsed = new bool[5];

        // First pass — correct letters
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == target[i])
            {
                result[i] = "correct";
                targetUsed[i] = true;
            }
        }

        // Second pass — present/absent
        for (int i = 0; i < 5; i++)
        {
            if (result[i] == "correct") continue;

            bool found = false;
            for (int j = 0; j < 5; j++)
            {
                if (!targetUsed[j] && guess[i] == target[j])
                {
                    result[i] = "present";
                    targetUsed[j] = true;
                    found = true;
                    break;
                }
            }
            if (!found) result[i] = "absent";
        }

        // Reveal one cell at a time with a small delay
        for (int i = 0; i < 5; i++)
        {
            Image cell = rowObjects[row].GetComponentsInChildren<Image>()[i];
            cell.color = result[i] == "correct" ? correctColor
                       : result[i] == "present" ? presentColor
                       : absentColor;

            // Also update keyboard button color
            UpdateKeyboardKey(guess[i], result[i]);

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    void UpdateKeyboardKey(char letter, string result)
    {
        foreach (Button btn in letterButtons)
        {
            if (btn.GetComponentInChildren<TextMeshProUGUI>().text == letter.ToString())
            {
                Image img = btn.GetComponent<Image>();
                // Don't downgrade green to yellow
                if (img.color == correctColor) return;
                img.color = result == "correct" ? correctColor
                          : result == "present" ? presentColor
                          : absentColor;
            }
        }
    }

    void PuzzleSolved()
    {
        PuzzleManager.Instance.NotifySolved(factorName);
    }
}