using UnityEngine;
using UnityEngine.UI;


public class VolumeSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        // Mỗi lần panel này được bật lên (mở Pause Menu / mở tab Settings),
        // đồng bộ giá trị Slider với volume hiện tại của AudioManager.
        if (AudioManager.Instance == null) return;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    private void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}