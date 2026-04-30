using UnityEngine;
using UnityEngine.Audio;

public static class AudioSettings
{
    const string PREF_MUSIC = "opt_music";
    const string PREF_SFX = "opt_sfx";

    public static AudioMixer audioMixer;
    public static float MusicVolume { get; private set; } = 0.8f;
    public static float SFXVolume { get; private set; } = 0.8f;

    public static void Load()
    {
        MusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC, MusicVolume);
        SFXVolume = PlayerPrefs.GetFloat(PREF_SFX, SFXVolume);
        
        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        
        ApplyMixerVolumes();
    }

    public static void SetMusic(float v)
    {
        MusicVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PREF_MUSIC, MusicVolume);
        PlayerPrefs.Save();
        ApplyMixerVolumes();
    }

    public static void SetSFX(float v)
    {
        SFXVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PREF_SFX, SFXVolume);
        PlayerPrefs.Save();
        ApplyMixerVolumes();
    }

    static void ApplyMixerVolumes()
    {
        if (audioMixer == null) return;

        float musicdB = MusicVolume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(MusicVolume, 0.0001f, 1f)) * 20f;
        float sfxdB = SFXVolume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(SFXVolume, 0.0001f, 1f)) * 20f;

        audioMixer.SetFloat("MusicVolume", musicdB);
        audioMixer.SetFloat("SFXVolume", sfxdB);
    }

    public static void ApplyMusicToSource(AudioSource src)
    {
        // mixer handles volume now
        if (src == null) return;
        src.volume = 1f;
    }

    public static void PlaySFX(AudioSource src, AudioClip clip)
    {
        if (src == null || clip == null) return;
        src.PlayOneShot(clip, 1f);
    }
}
