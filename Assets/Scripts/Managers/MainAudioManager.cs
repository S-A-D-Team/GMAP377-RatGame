using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//  Audio containers
// Holds information about a music track
[System.Serializable]
public class MusicBundle
{
    public string name;                    // Readable name (lookup key)
    public AudioClip clip;                 // AudioClip to play
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;

    [HideInInspector] public AudioSource source;   // Filled at runtime
}

// Holds information about a sound effect
[System.Serializable]
public class SoundEffectBundle
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public class MainAudioManager : MonoBehaviour
{
    public static MainAudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;  // Optional mixer for volume control

    [Header("Music Bundle")]
    public List<MusicBundle> musicTracks = new List<MusicBundle>();
    private Dictionary<string, MusicBundle> musicDictionary = new Dictionary<string, MusicBundle>();
    private AudioSource musicSource;

    [Header("Sound Effects Bundle")]
    public List<SoundEffectBundle> soundEffects = new List<SoundEffectBundle>();
    private Dictionary<string, SoundEffectBundle> sfxDictionary = new Dictionary<string, SoundEffectBundle>();

    // Pooling of SFX sources so we don't instantiate new AudioSources every frame
    private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();

    [Header("Volume Control")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Keys for PlayerPrefs
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY  = "MusicVolume";
    private const string SFX_VOLUME_KEY    = "SFXVolume";

    //  Initialization
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadVolumeSettings();
        ApplyVolumeSettings();
    }

    private void InitializeAudioManager()
    {
        CreateAudioSources();        // create AudioSources for music/SFX
        PopulateDictionaries();      // build lookups from lists
    }

    //  Create AudioSource objects
    private void CreateAudioSources()
    {
        // Music source - one single dedicated AudioSource
        GameObject musicObject = new GameObject("MusicSource");
        musicObject.transform.SetParent(transform);
        musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        // Prefill SFX pool with reusable AudioSource objects
        for (int i = 0; i < 10; i++)
        {
            GameObject sfxObject = new GameObject("SFXSource_" + i);
            sfxObject.transform.SetParent(transform);
            AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSourcePool.Enqueue(sfxSource);
        }
    }

    //  Build lookup dictionaries for music & SFX
    private void PopulateDictionaries()
    {
        musicDictionary.Clear();
        foreach (MusicBundle track in musicTracks)
        {
            if (!string.IsNullOrEmpty(track.name))
                musicDictionary[track.name.ToLower()] = track;
        }

        sfxDictionary.Clear();
        foreach (SoundEffectBundle sfx in soundEffects)
        {
            if (!string.IsNullOrEmpty(sfx.name))
                sfxDictionary[sfx.name.ToLower()] = sfx;
        }
    }

    //  MUSIC METHODS
    public void PlayMusic(string trackName)
    {
        if (musicDictionary.TryGetValue(trackName.ToLower(), out MusicBundle track))
        {
            if (track.clip != null)
            {
                musicSource.clip   = track.clip;
                musicSource.volume = track.volume * musicVolume;
                musicSource.loop   = track.loop;
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)  musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)  musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
            musicSource.UnPause();
    }

    //  SFX METHODS
    public void PlaySFX(string sfxName) => PlaySFX(sfxName, 1f);

    public void PlaySFX(string sfxName, float volumeMultiplier)
    {
        if (sfxDictionary.TryGetValue(sfxName.ToLower(), out SoundEffectBundle sfx))
        {
            if (sfx.clip != null)
            {
                AudioSource source = GetAvailableSFXSource();
                source.clip   = sfx.clip;
                source.volume = sfx.volume * sfxVolume * volumeMultiplier;
                source.pitch  = sfx.pitch;
                source.loop   = sfx.loop;
                source.Play();

                // If this clip isn't looping, return the AudioSource back to the pool after it's done
                if (!sfx.loop)
                    StartCoroutine(ReturnSourceToPool(source, sfx.clip.length));
            }
        }
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in activeSfxSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
                ReturnSourceToPoolImmediate(source);
            }
        }
        activeSfxSources.Clear();
    }

    private AudioSource GetAvailableSFXSource()
    {
        // Re-use from pool if possible
        AudioSource source;
        if (sfxSourcePool.Count > 0)
            source = sfxSourcePool.Dequeue();
        else
        {
            // If we ran out, create a new temp one
            GameObject sfxObject = new GameObject("SFXSource_Temp");
            sfxObject.transform.SetParent(transform);
            source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        activeSfxSources.Add(source);
        return source;
    }

    // Sends it back to the pool after a delay
    private IEnumerator ReturnSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnSourceToPoolImmediate(source);
    }

    // Immediately return a source to the pool
    private void ReturnSourceToPoolImmediate(AudioSource source)
    {
        if (source != null)
        {
            activeSfxSources.Remove(source);
            sfxSourcePool.Enqueue(source);
        }
    }

    //  VOLUME CONTROL
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        // Update music
        if (musicSource.isPlaying)
        {
            var current = GetCurrentMusicTrack();
            if (current != null)
                musicSource.volume = current.volume * musicVolume * masterVolume;
        }

        // Update active SFX
        foreach (var source in activeSfxSources)
        {
            if (source != null)
                source.volume = source.volume * sfxVolume * masterVolume;
        }

        // Update AudioMixer (optional)
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);

        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource.isPlaying)
        {
            var current = GetCurrentMusicTrack();
            if (current != null)
                musicSource.volume = current.volume * musicVolume * masterVolume;
        }

        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        // Update all active SFX sources immediately
        foreach (var source in activeSfxSources)
        {
            if (source != null)
                source.volume = source.volume * sfxVolume * masterVolume;  // <-- include masterVolume
        }

        SaveVolumeSettings();
    }

    public float GetMusicVolume()  => musicVolume;
    public float GetSFXVolume()    => sfxVolume;
    public float GetMasterVolume() => masterVolume;

    // Returns the currently playing music track (if any)
    private MusicBundle GetCurrentMusicTrack()
    {
        if (musicSource.clip != null)
        {
            foreach (var track in musicTracks)
            {
                if (track.clip == musicSource.clip)
                    return track;
            }
        }
        return null;
    }

    //  LOAD / SAVE VOLUME TO PLAYERPREFS
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        musicVolume  = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume    = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY,  musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY,    sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolumeSettings()
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            audioMixer.SetFloat("MusicVolume",  Mathf.Log10(musicVolume)  * 20);
            audioMixer.SetFloat("SFXVolume",    Mathf.Log10(sfxVolume)    * 20);
        }
    }

    //  Utility
    public bool HasMusicTrack(string trackName)   => musicDictionary.ContainsKey(trackName.ToLower());
    public bool HasSoundEffect(string sfxName)    => sfxDictionary.ContainsKey(sfxName.ToLower());

    public void AddMusicTrack(MusicBundle track)
    {
        if (!string.IsNullOrEmpty(track.name))
        {
            musicTracks.Add(track);
            musicDictionary[track.name.ToLower()] = track;
        }
    }

    public void AddSoundEffect(SoundEffectBundle sfx)
    {
        if (!string.IsNullOrEmpty(sfx.name))
        {
            soundEffects.Add(sfx);
            sfxDictionary[sfx.name.ToLower()] = sfx;
        }
    }
}
