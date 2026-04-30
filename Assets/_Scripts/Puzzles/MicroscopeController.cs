using UnityEngine;
using TMPro;

public class MicroscopeController : MonoBehaviour
{
    public static MicroscopeController Instance;

    [Header("The door code shown through microscope")]
    [SerializeField] string doorCode = "4729";

    [Header("Code display — a TextMesh on the microscope")]
    [SerializeField] GameObject codeDisplayObject;
    [SerializeField] TextMeshPro codeText;

    [Header("Interaction")]
    [SerializeField] float interactDistance = 2f;
    [SerializeField] GameObject promptObject;

    private bool isUnlocked = false;
    private Transform player;

    void Awake() => Instance = this;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (codeDisplayObject) codeDisplayObject.SetActive(false);
        if (promptObject)      promptObject.SetActive(false);
        if (codeText)          codeText.text = doorCode;
    }

    void Update()
    {
        if (!isUnlocked) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool  near = dist < interactDistance;

        if (promptObject) promptObject.SetActive(near);

        if (near && Input.GetKeyDown(KeyCode.E))
            LookThroughLens();
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Microscope unlocked!");
    }

    void LookThroughLens()
    {
        if (codeDisplayObject)
            codeDisplayObject.SetActive(true);
    }
}