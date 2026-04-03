using UnityEngine;

public class interactableObject : MonoBehaviour
{
    public float interactRange = 3f;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    Debug.Log("You clicked: " + hit.collider.gameObject.name);
                    // Add your interaction logic here
                }
            }
        }
    }
}