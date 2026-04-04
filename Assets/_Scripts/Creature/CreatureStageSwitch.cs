using UnityEngine;
using UnityEngine.SceneManagement;

public class CreatureStageSwitch : MonoBehaviour
{
    [Header("Creature Stages")]
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;

    [Header("Tank")]
    public GameObject normalTank;
    public GameObject brokenTankPart1;
    public GameObject brokenTankPart2;

    [Header("Stage Sounds")]
    public AudioClip stage1Sound;
    public AudioClip stage2Sound;
    public AudioClip stage3Sound;
    public AudioClip gameOverSound;

    [Header("Timing (seconds)")]
    public float timeToStage2 = 15f;
    public float timeToStage3 = 30f;
    public float timeToStage4 = 45f;

    [Header("Game Over")]
    public string gameOverScene = "GameOver";

    private int currentStage = 1;
    private float lockedX;
    private float lockedZ;
    private Quaternion lockedRotation;
    private AudioSource audioSource;

    void Start()
    {
        stage2.SetActive(false);
        stage3.SetActive(false);

        // Hide both broken tank parts at start
        brokenTankPart1.SetActive(false);
        brokenTankPart2.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        PlaySound(stage1Sound);
        SavePosition(stage1);

        Invoke("SwitchToStage2", timeToStage2);
        Invoke("SwitchToStage3", timeToStage3);
        Invoke("GameOver", timeToStage4);
    }

    void PlaySound(AudioClip clip)
    {
        if(clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void SavePosition(GameObject obj)
    {
        lockedX = obj.transform.position.x;
        lockedZ = obj.transform.position.z;
        lockedRotation = obj.transform.rotation;
    }

    void DisableRootMotion(GameObject obj)
    {
        Animator anim = obj.GetComponent<Animator>();
        if(anim != null) anim.applyRootMotion = false;
    }

    void SwitchToStage(GameObject from, GameObject to)
    {
        lockedX = from.transform.position.x;
        lockedZ = from.transform.position.z;
        lockedRotation = from.transform.rotation;

        DisableRootMotion(to);

        from.SetActive(false);

        to.transform.position = new Vector3(lockedX, from.transform.position.y, lockedZ);
        to.transform.rotation = lockedRotation;
        to.SetActive(true);
    }

    void SwitchToStage2()
    {
        Debug.Log("Creature evolving to Stage 2!");
        SwitchToStage(stage1, stage2);
        currentStage = 2;
        PlaySound(stage2Sound);
    }

    void SwitchToStage3()
    {
        Debug.Log("Creature evolving to Stage 3!");
        SwitchToStage(stage2, stage3);
        currentStage = 3;
        PlaySound(stage3Sound);
    }

    void GameOver()
    {
        Debug.Log("Stage 4 - Creature breaks glass - GAME OVER!");

        // Hide stage 3
        stage3.SetActive(false);

        // Hide normal tank, show both broken parts
        normalTank.SetActive(false);
        brokenTankPart1.SetActive(true);
        brokenTankPart2.SetActive(true);

        // Play game over sound
        audioSource.loop = false;
        PlaySound(gameOverSound);

        Invoke("LoadGameOver", gameOverSound != null ? gameOverSound.length : 2f);
    }

    void LoadGameOver()
    {
        SceneManager.LoadScene(gameOverScene);
    }

    void LateUpdate()
    {
        if(currentStage == 2 && stage2.activeSelf)
        {
            stage2.transform.position = new Vector3(lockedX, stage2.transform.position.y, lockedZ);
            stage2.transform.rotation = lockedRotation;
        }
        else if(currentStage == 3 && stage3.activeSelf)
        {
            stage3.transform.position = new Vector3(lockedX, stage3.transform.position.y, lockedZ);
            stage3.transform.rotation = lockedRotation;
        }
    }

    public void OnPuzzleSolved(int puzzleNumber)
    {
        Debug.Log("Puzzle " + puzzleNumber + " solved!");
        if(puzzleNumber == 1) SwitchToStage2();
        else if(puzzleNumber == 2) SwitchToStage3();
        else if(puzzleNumber == 3) GameOver();
    }
}