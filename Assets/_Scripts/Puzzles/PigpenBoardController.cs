using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PigpenBoardController : MonoBehaviour
{
    public static PigpenBoardController Instance;

    [Header("The Pigpen puzzle canvas")]
    [SerializeField] GameObject pigpenCanvas;
    [SerializeField] bool enforceCanvasScaler = true;
    [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Interaction")]
    [SerializeField] float interactDistance = 3f;
    [SerializeField] GameObject promptObject;
    [SerializeField] Color promptColor = new Color(0.2f, 1f, 0.35f, 1f);

    TextMeshProUGUI promptLabel;

    public bool IsOpen => isOpen;

    private bool isOpen  = false;
    private bool isSolved = false;
    private Transform player;
    bool prevNear = false;

    void Awake() => Instance = this;

    void Start()
    {
        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.transform;
        else if (Camera.main != null) player = Camera.main.transform;

        if (pigpenCanvas) pigpenCanvas.SetActive(false);

        // Optionally enforce CanvasScaler settings to improve UI scaling across screens
        if (enforceCanvasScaler && pigpenCanvas != null)
        {
            var scaler = pigpenCanvas.GetComponentInChildren<UnityEngine.UI.CanvasScaler>(true);
            if (scaler != null)
            {
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = referenceResolution;
                scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                Debug.Log("PigpenBoardController: enforced CanvasScaler settings for pigpenCanvas");
            }
        }
        if (promptObject)
        {
            promptObject.SetActive(false);
            promptLabel = promptObject.GetComponentInChildren<TextMeshProUGUI>(true);
            if (promptLabel != null)
                promptLabel.color = promptColor;
        }
    }

    void Update()
    {
        if (isSolved || isOpen) return;

        if (player == null)
        {
            // Try to locate player at runtime if missing
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        bool  near = dist < interactDistance;

        if (promptObject)
        {
            promptObject.SetActive(near);
            if (near && promptLabel == null)
                promptLabel = promptObject.GetComponentInChildren<TextMeshProUGUI>(true);
            if (promptLabel != null)
                promptLabel.color = promptColor;
        }

        if (near && !prevNear)
        {
            Debug.Log($"PigpenBoardController: player entered range (dist={dist:F2}) for {gameObject.name}");
        }

        if (near && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("PigpenBoardController: E pressed to open pigpen puzzle");
            OpenPuzzle();
        }

        prevNear = near;
    }

    public void OpenPuzzle()
    {
        isOpen = true;
        Debug.Log("OpenPuzzle called! Canvas is: " + (pigpenCanvas == null ? "NULL" : pigpenCanvas.name));
        if (pigpenCanvas) pigpenCanvas.SetActive(true);
        if (promptObject) promptObject.SetActive(false);
    }

    public void ClosePuzzle()
    {
        isOpen = false;
        if (pigpenCanvas) pigpenCanvas.SetActive(false);
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public void OnPuzzleSolved()
    {
        isSolved = true;
        isOpen   = false;
        if (promptObject) promptObject.SetActive(false);
        Debug.Log("Pigpen puzzle solved!");
    }
}