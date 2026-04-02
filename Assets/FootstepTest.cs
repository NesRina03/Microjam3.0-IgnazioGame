using UnityEngine;

public class FootstepTest : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip[] footstepSounds;
    public float walkInterval = 0.5f;
    public float sprintInterval = 0.3f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private AudioSource audioSource;
    private float footstepTimer = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        bool isSprinting = Input.GetKey(sprintKey);

        if(isMoving && footstepSounds.Length > 0)
        {
            float interval = isSprinting ? sprintInterval : walkInterval;
            footstepTimer -= Time.deltaTime;
            if(footstepTimer <= 0)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                audioSource.PlayOneShot(footstepSounds[randomIndex]);
                footstepTimer = interval;
            }
        }
    }
}