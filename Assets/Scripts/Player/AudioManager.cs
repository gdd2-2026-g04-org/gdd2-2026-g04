using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")] [SerializeField] private AudioMixer audioMixer;
    [Header("Music")] [SerializeField] private AudioSource musicSource;
    [Header("UI")] [SerializeField] private AudioSource uiSource;
    
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (!music || !musicSource) return;
        if (musicSource.clip == music && musicSource.isPlaying) return;
        
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource) musicSource.Stop();
    }

    public void PlayUISound(AudioClip sound)
    {
        if (sound && uiSource) uiSource.PlayOneShot(sound);
    }

    public static void PlaySoundAtSource(AudioClip sound, AudioSource source)
    {
        source?.PlayOneShot(sound);
    }

    public void SetMasterVolume(float vol)
    {
        SetMixerVolume("MasterVolume", vol);
    }

    public void SetMusicVolume(float vol)
    {
        SetMixerVolume("MusicVolume", vol);
    }

    public void SetSFXVolume(float vol)
    {
        SetMixerVolume("SFXVolume", vol);
    }

    private void SetMixerVolume(string parameter, float linearVol)
    {
        if (!audioMixer) return;

        linearVol = Mathf.Clamp(linearVol, 0.0001f, 1f);

        var dB = Mathf.Log10(linearVol) * 20f;

        audioMixer.SetFloat(parameter, dB);
    }
}
