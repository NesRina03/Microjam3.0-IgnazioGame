using UnityEngine;
using TMPro;

public class InteractionRaycaster : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask terminalLayer;
    [SerializeField] KeyCode interactKey = KeyCode.E;

    [Header("HUD")]
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] GameObject crosshair;

    PuzzleTerminal currentTerminal;

    void Update()
    {
        // Block ALL input when Pigpen puzzle is open
        if (PigpenBoardController.Instance != null && PigpenBoardController.Instance.IsOpen)
        {
            HidePrompt();
            return;
        }

        if (PuzzleManager.Instance == null)
            return;

        // Check for board interaction
        Ray earlyRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] earlyHits = Physics.RaycastAll(earlyRay, interactRange);
        foreach (RaycastHit h in earlyHits)
        {
            PigpenBoardController board = h.collider.GetComponent<PigpenBoardController>();
            if (board != null)
            {
                ShowPrompt("[E] Inspect Board");
                if (Input.GetKeyDown(interactKey))
                    board.OpenPuzzle();
                return;
            }
        }

        if (PuzzleManager.Instance.IsPuzzleOpen)
        {
            HidePrompt();
            return;
        }

        ShootRay();

        if (currentTerminal != null && Input.GetKeyDown(interactKey))
            PuzzleManager.Instance.OpenPuzzle(currentTerminal);
    }

    void ShootRay()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, interactRange);

        if (hit)
        {
            PuzzleTerminal terminal = hitInfo.collider.GetComponent<PuzzleTerminal>();
            if (terminal != null && !terminal.IsSolved)
            {
                currentTerminal = terminal;
                ShowPrompt("[E] " + terminal.PromptLabel);
                return;
            }
        }

        currentTerminal = null;
        HidePrompt();
    }

    void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            promptText.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}