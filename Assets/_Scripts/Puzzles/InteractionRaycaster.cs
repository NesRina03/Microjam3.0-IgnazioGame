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
    Debug.Log("A");

    if (PuzzleManager.Instance == null)
    {
        Debug.Log("B - PM is null");
        return;
    }

    Debug.Log("C - PM exists");

    if (PuzzleManager.Instance.IsPuzzleOpen)
    {
        Debug.Log("D - puzzle is open");
        HidePrompt();
        return;
    }

    Debug.Log("E - about to shoot ray");

    ShootRay();

    if (currentTerminal != null && Input.GetKeyDown(interactKey))
        PuzzleManager.Instance.OpenPuzzle(currentTerminal);
}

void ShootRay()
{
    if (playerCamera == null)
    {
        Debug.Log("CAMERA IS NULL");
        return;
    }

    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);

    // TEMPORARY — no layer filter, hits everything
    bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, interactRange);

    Debug.Log("Hit: " + hit);

    if (hit)
    {
        Debug.Log("Hit object: " + hitInfo.collider.gameObject.name);
        Debug.Log("Object layer: " + hitInfo.collider.gameObject.layer);

        PuzzleTerminal terminal = hitInfo.collider.GetComponent<PuzzleTerminal>();
        Debug.Log("Has PuzzleTerminal: " + (terminal != null));

        if (terminal != null && !terminal.IsSolved)
        {
            // Skip door terminal if not in Level 2
            if (terminal.FactorName == "door" && GameManager.Instance != null)
            {
                if (GameManager.Instance.currentState != GameManager.GameState.Level2)
                {
                    currentTerminal = null;
                    HidePrompt();
                    return;
                }
            }

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