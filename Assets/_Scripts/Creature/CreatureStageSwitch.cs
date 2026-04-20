using UnityEngine;
using System.Collections;
using UnityEngine.Video;


public class CreatureStageSwitch : MonoBehaviour
{

    [Header("Cutscene")]
    public GameObject cutsceneCanvas;
    public VideoPlayer cutsceneVideo;

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

    private int currentStage = 1;
    private float lockedX;
    private float lockedZ;
    private Quaternion lockedRotation;
    private AudioSource audioSource;

    void Start()
    {
        stage2.SetActive(false);
        stage3.SetActive(false);
        brokenTankPart1.SetActive(false);
        brokenTankPart2.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        PlaySound(stage1Sound);
        SavePosition(stage1);

        // Driven by InstabilityManager events — no independent Invoke timers
        if (InstabilityManager.Instance != null)
        {
            InstabilityManager.Instance.OnStageChanged.AddListener(OnInstabilityStageChanged);
            InstabilityManager.Instance.OnCreatureFreed.AddListener(GameOver);
        }
    }

    void OnDestroy()
    {
        if (InstabilityManager.Instance != null)
        {
            InstabilityManager.Instance.OnStageChanged.RemoveListener(OnInstabilityStageChanged);
            InstabilityManager.Instance.OnCreatureFreed.RemoveListener(GameOver);
        }
    }

    void OnInstabilityStageChanged(int stage)
    {
        if (stage == 2) SwitchToStage2();
        else if (stage == 3) SwitchToStage3();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
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
        if (anim != null) anim.applyRootMotion = false;
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
    SwitchToStage(stage1, stage2);
    currentStage = 2;
    PlaySound(stage2Sound);
}

void SwitchToStage3()
{
    SwitchToStage(stage2, stage3);
    currentStage = 3;
    PlaySound(stage3Sound);
}

    void GameOver()
    {
        stage3.SetActive(false);
        normalTank.SetActive(false);
        brokenTankPart1.SetActive(true);
        brokenTankPart2.SetActive(true);

        audioSource.loop = false;
        PlaySound(gameOverSound);

        float delay = gameOverSound != null ? gameOverSound.length : 2f;
        StartCoroutine(GameOverDelay(delay));
    }
IEnumerator GameOverDelay(float delay)
{
    yield return new WaitForSecondsRealtime(delay);
    StartCoroutine(PlayCutsceneThenLose());
}

IEnumerator PlayCutsceneThenLose()
    {
        // Show cutscene canvas
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(true);

        if (cutsceneVideo != null)
        {
            cutsceneVideo.Play();
            // Wait until video is done
            yield return new WaitUntil(() => !cutsceneVideo.isPlaying);
        }
        else
        {
            yield return new WaitForSecondsRealtime(3f); // fallback
        }

        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        ShowLoseUI();
    }

    void ShowLoseUI()
    {
        GameManager.Instance.TriggerGameOver();
    }
    void LateUpdate()
    {
        if (currentStage == 2 && stage2.activeSelf)
        {
            stage2.transform.position = new Vector3(lockedX, stage2.transform.position.y, lockedZ);
            stage2.transform.rotation = lockedRotation;
        }
        else if (currentStage == 3 && stage3.activeSelf)
        {
            stage3.transform.position = new Vector3(lockedX, stage3.transform.position.y, lockedZ);
            stage3.transform.rotation = lockedRotation;
        }
    }
}