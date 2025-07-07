using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class SettingsManager : MonoBehaviour
{
    public Slider musicslider;
    public Slider sfxslider;
    public TMP_Text musicValueText; 
    public TMP_Text sfxValueText;

    public GameObject BGM;   

    private AudioSource bgmAudioSource;

    //public AudioSource sfxAudioSource; 

    void Start()
    {
        // Get AudioSource from BGM
        bgmAudioSource = BGM.GetComponent<AudioSource>();

        musicslider.value = bgmAudioSource.volume * musicslider.maxValue;
        //sfxslider.value = sfxAudioSource.volume * musicslider.maxValue;

        musicslider.onValueChanged.AddListener(UpdateMusicValueText);
        //sfxslider.onValueChanged.AddListener(UpdateSFXValueText);

        UpdateMusicValueText(musicslider.value);
        //UpdateSFXValueText(sfxslider.value);
    }

    void UpdateMusicValueText(float value)
    {
        musicValueText.text = value.ToString("0");
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = value / musicslider.maxValue;
        }
    }

    /*
    void UpdateSFXValueText(float value)
    {
        sfxValueText.text = value.ToString("0");
        // Placeholder: Set SFX volume when ready
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = value / sfxslider.maxValue;
        }
    }
    */
}
