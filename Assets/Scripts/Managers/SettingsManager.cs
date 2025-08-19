using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Master volume slider")]
    public Slider masterslider;
    [Tooltip("Master volume display text")]
    public TMP_Text masterValueText;

    [Tooltip("Music volume slider")]
    public Slider musicslider;
    [Tooltip("Music volume display text")]
    public TMP_Text musicValueText;

    [Tooltip("SFX volume slider")]
    public Slider sfxslider;
    [Tooltip("SFX volume display text")]
    public TMP_Text sfxValueText;

    // Initialize settings on game start
    void Start()
    {
        // Initialize UI listeners
        masterslider.onValueChanged.AddListener(UpdateMasterValueText);
        musicslider.onValueChanged.AddListener(UpdateMusicValueText);
        sfxslider.onValueChanged.AddListener(UpdateSFXValueText);

        // Load and apply saved settings
        LoadSettings();
    }



    // Load saved settings from PlayerPrefs
    private void LoadSettings()
    {
        // Load volume settings
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // Update sliders
        masterslider.value = masterVol * masterslider.maxValue;
        musicslider.value = musicVol * musicslider.maxValue;
        sfxslider.value = sfxVol * sfxslider.maxValue;

        // Update display texts
        UpdateMasterValueText(masterslider.value);
        UpdateMusicValueText(musicslider.value);
        UpdateSFXValueText(sfxslider.value);

        // Apply settings to MainAudioManager
        ApplyVolumeSettings();
    }

    // Update master volume display and save setting
    private void UpdateMasterValueText(float value)
    {
        masterValueText.text = value.ToString("0");
        float normalizedValue = value / masterslider.maxValue;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", normalizedValue);
        PlayerPrefs.Save();
        
        // Apply to MainAudioManager
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.SetMasterVolume(normalizedValue);
        }
    }


    // Update music volume display and save setting
    private void UpdateMusicValueText(float value)
    {
        musicValueText.text = value.ToString("0");
        float normalizedValue = value / musicslider.maxValue;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", normalizedValue);
        PlayerPrefs.Save();
        
        // Apply to MainAudioManager
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.SetMusicVolume(normalizedValue);
        }
    }

    // Update SFX volume display and save setting
    private void UpdateSFXValueText(float value)
    {
        sfxValueText.text = value.ToString("0");
        float normalizedValue = value / sfxslider.maxValue;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", normalizedValue);
        PlayerPrefs.Save();
        
        // Apply to MainAudioManager
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.SetSFXVolume(normalizedValue);
        }
    }

    // Apply all volume settings to MainAudioManager
    private void ApplyVolumeSettings()
    {
        if (MainAudioManager.Instance == null)
        {
            Debug.LogWarning("MainAudioManager not found!");
            return;
        }

        // Apply all volume settings
        MainAudioManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        MainAudioManager.Instance.SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.5f));
        MainAudioManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 0.5f));
    }

    // Reset all settings to default values
    public void ResetToDefaults()
    {
        // Reset volumes
        masterslider.value = masterslider.maxValue; // 100%
        musicslider.value = masterslider.maxValue * 0.5f; // 50%
        sfxslider.value = masterslider.maxValue * 0.5f; // 50%
    }
}
