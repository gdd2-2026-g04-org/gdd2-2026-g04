using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        if (!AudioManager.Instance)
        {
            Debug.LogWarning("(SettingsUI): AudioManager is missing.");
            return;
        }
        
        if (masterVolumeSlider) masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetMasterVolume());
        if (musicVolumeSlider) musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
        if (sfxVolumeSlider) sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
    }
}
