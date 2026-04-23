using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Mixer : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MUSIC_KEY = "music"; 
    private const string SFX_KEY = "sfx";    
    private const float DEFAULT_VOLUME = 1f; 

    private void Awake()
    {
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_KEY, DEFAULT_VOLUME);
        float savedSfx = PlayerPrefs.GetFloat(SFX_KEY, DEFAULT_VOLUME);

        if (musicSlider != null)
            musicSlider.value = savedMusic * musicSlider.maxValue;
        if (sfxSlider != null)
            sfxSlider.value = savedSfx * sfxSlider.maxValue;

        ApplyVolumeToMixer("Music", savedMusic);
        ApplyVolumeToMixer("SFX", savedSfx);
    }

    private void OnEnable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    private void OnMusicSliderChanged(float sliderValue)
    {
        float normalized = NormalizeSliderValue(sliderValue, musicSlider);
        PlayerPrefs.SetFloat(MUSIC_KEY, normalized);
        ApplyVolumeToMixer("Music", normalized);
    }

    private void OnSFXSliderChanged(float sliderValue)
    {
        float normalized = NormalizeSliderValue(sliderValue, sfxSlider);
        PlayerPrefs.SetFloat(SFX_KEY, normalized);
        ApplyVolumeToMixer("SFX", normalized);
    }

    private float NormalizeSliderValue(float sliderValue, Slider slider)
    {
        if (slider == null)
            return Mathf.Clamp01(sliderValue);

        if (slider.maxValue > 1f)
            return Mathf.Clamp01(sliderValue / slider.maxValue);

        return Mathf.Clamp01(sliderValue);
    }

    private void ApplyVolumeToMixer(string exposedParam, float normalizedVolume)
    {
        if (audioMixer == null) return;

        float v = Mathf.Clamp(normalizedVolume, 0.0001f, 1f);
        float dB = Mathf.Log10(v) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
    }
}
