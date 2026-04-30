using UnityEngine;

public class PigpenBoardController : MonoBehaviour
{
    public static PigpenBoardController Instance;

    [Header("The Pigpen puzzle canvas")]
    [SerializeField] GameObject pigpenCanvas;

    [Header("Interaction")]
    [SerializeField] float interactDistance = 3f;
    [SerializeField] GameObject promptObject;

    [Header("Optional — shown after solve")]
    [SerializeField] GameObject solvedIndicator;

    public bool IsOpen   => isOpen;
    public bool IsSolved => isSolved;

    private bool isOpen   = false;
    private bool isSolved = false;
    private Transform player;

    void Awake() => Instance = this;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (pigpenCanvas)    pigpenCanvas.SetActive(false);
        if (promptObject)    promptObject.SetActive(false);
        if (solvedIndicator) solvedIndicator.SetActive(false);
    }

    void Update()
    {
        if (isSolved || isOpen) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool  near = dist < interactDistance;

        if (promptObject) promptObject.SetActive(near);

        if (near && Input.GetKeyDown(KeyCode.E))
            OpenPuzzle();
    }

    public void OpenPuzzle()
    {
        if (isSolved) return;

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
        if (promptObject)    promptObject.SetActive(false);
        if (solvedIndicator) solvedIndicator.SetActive(true);
        Debug.Log("Pigpen puzzle solved — board locked.");
    }
}