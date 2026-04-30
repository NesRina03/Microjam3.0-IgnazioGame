using UnityEngine;

public class MicroscopeController : MonoBehaviour
{
    public static MicroscopeController Instance;

    [Header("Interaction")]
    [SerializeField] float interactDistance = 2f;
    [SerializeField] GameObject promptObject;

    [Header("Microscope Canvas")]
    [SerializeField] GameObject microscopeCanvas;

    public bool IsCanvasOpen => canvasOpen;

    private bool isUnlocked = false;
    private bool canvasOpen = false;
    private Transform player;

    void Awake() => Instance = this;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (promptObject)     promptObject.SetActive(false);
        if (microscopeCanvas) microscopeCanvas.SetActive(false);
    }

    void Update()
    {
        if (!isUnlocked) return;

        if (canvasOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCanvas();
            return;
        }

        if (canvasOpen) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool  near = dist < interactDistance;

        if (promptObject) promptObject.SetActive(near);

        if (near && Input.GetKeyDown(KeyCode.E))
            OpenCanvas();
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Microscope unlocked!");
    }

    public void OpenCanvas()
    {
        canvasOpen = true;
        if (microscopeCanvas) microscopeCanvas.SetActive(true);
        if (promptObject)     promptObject.SetActive(false);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void CloseCanvas()
    {
        canvasOpen = false;
        if (microscopeCanvas) microscopeCanvas.SetActive(false);
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}