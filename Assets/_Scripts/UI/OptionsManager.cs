using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI musicLabel;
    public TextMeshProUGUI sfxLabel;
    public Button resetButton;
    public Button backButton;
    public GameObject optionsPanel; // the panel to close when back is pressed

    // default values
    const float DEFAULT_MUSIC = 0.8f;
    const float DEFAULT_SFX = 0.8f;

    void Awake()
    {
        AudioSettings.Load();
    }

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f; musicSlider.maxValue = 1f; musicSlider.wholeNumbers = false;
            musicSlider.SetValueWithoutNotify(AudioSettings.MusicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdateLabel(musicLabel, AudioSettings.MusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; sfxSlider.wholeNumbers = false;
            sfxSlider.SetValueWithoutNotify(AudioSettings.SFXVolume);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            UpdateLabel(sfxLabel, AudioSettings.SFXVolume);
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
        
        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);
        
        if (optionsPanel == null)
            optionsPanel = gameObject;

        // Register panel on GameManager so options lifecycle is centrally managed.
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterOptionsPanel(optionsPanel);
    }

    void UpdateLabel(TextMeshProUGUI label, float v)
    {
        if (label == null) return;
        label.text = Mathf.RoundToInt(v * 100f) + "%";
    }

    void OnMusicChanged(float v)
    {
        AudioSettings.SetMusic(v);
        UpdateLabel(musicLabel, v);
    }

    void OnSFXChanged(float v)
    {
        AudioSettings.SetSFX(v);
        UpdateLabel(sfxLabel, v);
    }

    public void ResetToDefaults()
    {
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(DEFAULT_MUSIC);
            OnMusicChanged(DEFAULT_MUSIC);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(DEFAULT_SFX);
            OnSFXChanged(DEFAULT_SFX);
        }
    }
    
    public void ClosePanel()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.CloseOptions();
    }
}
