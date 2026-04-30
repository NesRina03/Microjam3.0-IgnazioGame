using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip mainMusic;
    public AudioClip level2Music;

    [Header("Sources")]
    [SerializeField] AudioSource mainSource;

    [Header("Playback")]
    [SerializeField] float crossfadeTime = 1.0f;

    AudioSource secondarySource;
    AudioSource activeSrc;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainSource == null)
            mainSource = GetComponent<AudioSource>();

        // create one helper source for crossfading if needed
        if (secondarySource == null)
        {
            secondarySource = gameObject.AddComponent<AudioSource>();
            secondarySource.name = "MusicManager Secondary Source";
        }

        if (mainSource != null)
        {
            mainSource.playOnAwake = false;
            mainSource.loop = true;
        }

        secondarySource.playOnAwake = false;
        secondarySource.loop = true;

        // route to Music group if available
        if (AudioSettings.audioMixer != null)
        {
            var groups = AudioSettings.audioMixer.FindMatchingGroups("Music");
            if (groups != null && groups.Length > 0)
            {
                if (mainSource != null) mainSource.outputAudioMixerGroup = groups[0];
                secondarySource.outputAudioMixerGroup = groups[0];
            }
        }

        activeSrc = mainSource != null ? mainSource : secondarySource;

        // If mainMusic wasn't assigned in inspector, try to auto-find an existing
        // background AudioSource in the scene and use its clip.
        if (mainMusic == null)
        {
            AudioSource[] all = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AudioSource best = null;
            foreach (var a in all)
            {
                if (a == mainSource || a == secondarySource) continue;
                if (a.clip == null) continue;

                // prefer sources routed to Music mixer group
                if (a.outputAudioMixerGroup != null && a.outputAudioMixerGroup.name.ToLower().Contains("music"))
                {
                    best = a;
                    break;
                }

                // prefer objects named like background/music
                if (a.gameObject.name.ToLower().Contains("background") || a.gameObject.name.ToLower().Contains("music"))
                {
                    best = a;
                    break;
                }

                if (best == null) best = a;
            }

            if (best != null)
            {
                mainMusic = best.clip;
                Debug.Log($"[MusicManager] Auto-assigned mainMusic from AudioSource on '{best.gameObject.name}'");
                // disable the original source to avoid double playback
                try { best.playOnAwake = false; best.Stop(); best.enabled = false; } catch { }
            }
        }
    }

    void Start()
    {
        Debug.Log($"[MusicManager] Start: mainMusic={(mainMusic!=null?mainMusic.name:"null")}, level2Music={(level2Music!=null?level2Music.name:"null")}");
        Debug.Log($"[MusicManager] AudioSettings.MusicVolume={AudioSettings.MusicVolume}");
        if (AudioSettings.audioMixer == null) Debug.LogWarning("[MusicManager] AudioSettings.audioMixer is null");
    }

    public void PlayMainMusic(float fade = 1f)
    {
        if (mainMusic == null) return;
        if (fade <= 0f) fade = crossfadeTime;
        PlayClip(mainMusic, fade);
    }

    public void PlayLevel2Music(float fade = 1f)
    {
        if (level2Music == null) return;
        if (fade <= 0f) fade = crossfadeTime;
        PlayClip(level2Music, fade);
    }

    void PlayClip(AudioClip clip, float fade)
    {
        AudioSource incoming = (activeSrc == mainSource) ? secondarySource : mainSource;
        if (incoming == null)
        {
            Debug.LogError("[MusicManager] No available AudioSource to play music.");
            return;
        }
        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();
        Debug.Log($"[MusicManager] PlayClip: playing '{clip.name}' on {incoming.name}. isPlaying after Play(): {incoming.isPlaying}");
        Debug.Log($"[MusicManager] Current AudioSettings.MusicVolume={AudioSettings.MusicVolume}");
        if (AudioSettings.MusicVolume <= 0.0001f) Debug.LogWarning("[MusicManager] MusicVolume appears to be zero or very low — music may be muted in mixer.");
        StopAllCoroutines();
        StartCoroutine(Crossfade(activeSrc, incoming, Mathf.Max(0.001f, fade)));
        activeSrc = incoming;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float duration)
    {
        float t = 0f;
        float fromStart = from != null ? from.volume : 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            to.volume = Mathf.Lerp(0f, 1f, p);
            if (from != null)
                from.volume = Mathf.Lerp(fromStart, 0f, p);
            yield return null;
        }

        to.volume = 1f;
        if (from != null)
        {
            from.volume = 0f;
            from.Stop();
        }
    }

    public void StopMusic(float fade = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(activeSrc, fade));
    }

    IEnumerator FadeOutAndStop(AudioSource s, float duration)
    {
        float start = s.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            s.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        s.Stop();
        s.volume = 1f;
    }
}
