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
    readonly RaycastHit[] hitBuffer = new RaycastHit[32];

    void Update()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
        {
            HidePrompt();
            return;
        }

        // Block ALL input when Pigpen puzzle is open
        if (PigpenBoardController.Instance != null && PigpenBoardController.Instance.IsOpen)
        {
            HidePrompt();
            return;
        }

        if (PuzzleManager.Instance == null)
            return;

        Ray earlyRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (TryGetNearestComponentHit<PigpenBoardController>(earlyRay, out PigpenBoardController board, out _))
        {
            ShowPrompt("[E] Inspect Board");
            if (Input.GetKeyDown(interactKey))
                board.OpenPuzzle();
            return;
        }

        if (PuzzleManager.Instance.IsPuzzleOpen)
        {
            HidePrompt();
            return;
        }

        if (TryGetNearestComponentHit<PuzzleTerminal>(earlyRay, out PuzzleTerminal terminal, out _))
        {
            if (!terminal.IsSolved)
            {
                // Hide door prompt unless we're in Level2
                if (terminal.FactorName == "door" && GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Level2)
                {
                    currentTerminal = null;
                    HidePrompt();
                    return;
                }

                currentTerminal = terminal;
                ShowPrompt("[E] " + terminal.PromptLabel);
                if (Input.GetKeyDown(interactKey))
                    PuzzleManager.Instance.OpenPuzzle(currentTerminal);
                return;
            }
        }

        currentTerminal = null;
        HidePrompt();
    }

    bool TryGetNearestComponentHit<T>(Ray ray, out T component, out RaycastHit bestHit) where T : Component
    {
        component = null;
        bestHit = default;

        int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, interactRange);
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit.collider == null) continue;

            // check on the hit collider and its parents (some controllers live on parent objects)
            T c = hit.collider.GetComponentInParent<T>();
            if (c == null) continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                component = c;
                bestHit = hit;
            }
        }

        return component != null;
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