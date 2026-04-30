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
        if (PigpenBoardController.Instance != null && PigpenBoardController.Instance.IsOpen)
        {
            HidePrompt();
            return;
        }

        if (MicroscopeController.Instance != null && MicroscopeController.Instance.IsCanvasOpen)
        {
            HidePrompt();
            return;
        }

        if (PuzzleManager.Instance == null)
            return;

        Ray earlyRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] earlyHits = Physics.RaycastAll(earlyRay, interactRange);
        foreach (RaycastHit h in earlyHits)
        {
            PigpenBoardController board = h.collider.GetComponent<PigpenBoardController>();
            if (board != null && !board.IsSolved)
            {
                ShowPrompt("[E] Inspect Board");
                if (Input.GetKeyDown(interactKey))
                    board.OpenPuzzle();
                return;
            }

            MicroscopeController scope = h.collider.GetComponent<MicroscopeController>();
            if (scope != null && !scope.IsCanvasOpen)
            {
                ShowPrompt("[E] Look through microscope");
                if (Input.GetKeyDown(interactKey))
                    scope.OpenCanvas();
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