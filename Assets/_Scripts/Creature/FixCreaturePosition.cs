using UnityEngine;

public class FixCreaturePosition : MonoBehaviour
{
    private float startX;
    private float startZ;
    private float yOffset;
    private bool initialized = false;

    void Start()
    {
        startX = transform.position.x;   // 440.68
        startZ = transform.position.z;   // -14.37
    }

    void LateUpdate()
    {
        // Capture l'offset au premier frame de l'animation
        if (!initialized)
        {
            yOffset = transform.position.y - (-7.64f);
            initialized = true;
        }

        transform.position = new Vector3(
            startX,
            transform.position.y - yOffset,  // corrige le Y
            startZ
        );
    }
}
