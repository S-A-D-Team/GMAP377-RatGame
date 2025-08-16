using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SettingsManager : MonoBehaviour
{
    public Slider masterslider; 
    public TMP_Text masterValueText; 

    public Slider musicslider;
    public TMP_Text musicValueText; 

    public Slider sfxslider;
    public TMP_Text sfxValueText;

    public GameObject BGM;   
    private AudioSource bgmAudioSource;
    private AudioSource sfxAudioSource; 

    [Header("Screen Resolution Dropdown")]
    public TMP_Dropdown resolutionDropdown;

    private readonly List<Vector2Int> supportedResolutions = new List<Vector2Int>
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(800, 600)
    };

    private float masterVolume = 1f;

    void Start()
    {
        // Get AudioSource from BGM
        if (BGM != null)
        {
            bgmAudioSource = BGM.GetComponent<AudioSource>();
        }

        // Load saved volumes
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        masterslider.onValueChanged.AddListener(UpdateMasterValueText);
        musicslider.onValueChanged.AddListener(UpdateMusicValueText);
        sfxslider.onValueChanged.AddListener(UpdateSFXValueText);

        masterslider.value = masterVolume * masterslider.maxValue;
        
        if (bgmAudioSource != null)
        {
            musicslider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f) * musicslider.maxValue;
        }
        else
        {
            musicslider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f) * musicslider.maxValue;
        }
        
        sfxslider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f) * sfxslider.maxValue;

        UpdateMasterValueText(masterslider.value);
        UpdateMusicValueText(musicslider.value);
        UpdateSFXValueText(sfxslider.value);

        // Apply all volumes on start
        ApplyAllVolumes();

        PopulateResolutionDropdown();
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var res in supportedResolutions)
        {
            options.Add($"{res.x} x {res.y}");
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        resolutionDropdown.value = 0;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnResolutionDropdownChanged(int index)
    {
        if (index < 0 || index >= supportedResolutions.Count) return;
        var res = supportedResolutions[index];
        Screen.SetResolution(res.x, res.y, Screen.fullScreen);
    }

    private void UpdateMasterValueText(float value)
    {
        masterValueText.text = value.ToString("0");
        masterVolume = value / masterslider.maxValue;
        
        // Save master volume to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
        
        // Apply master volume to all audio sources
        ApplyAllVolumes();
    }

    void UpdateMusicValueText(float value)
    {
        musicValueText.text = value.ToString("0");
        
        // Save music volume to PlayerPrefs
        float normalizedValue = value / musicslider.maxValue;
        PlayerPrefs.SetFloat("MusicVolume", normalizedValue);
        PlayerPrefs.Save();
        
        // Apply music volume with master volume
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = normalizedValue * masterVolume;
        }
    }

    void UpdateSFXValueText(float value)
    {
        sfxValueText.text = value.ToString("0");
        
        // Save SFX volume to PlayerPrefs
        float normalizedValue = value / sfxslider.maxValue;
        PlayerPrefs.SetFloat("SFXVolume", normalizedValue);
        PlayerPrefs.Save();
        
        // Apply SFX volume with master volume
        ApplyAllVolumes();
    }

    private void ApplyAllVolumes()
    {
        if (bgmAudioSource != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            bgmAudioSource.volume = musicVolume * masterVolume;
        }
        
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != bgmAudioSource)
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                audioSource.volume = sfxVolume * masterVolume;
            }
        }
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

}
