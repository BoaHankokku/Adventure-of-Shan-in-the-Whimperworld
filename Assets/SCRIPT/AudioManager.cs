using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;  // For background music
    public AudioSource sfxSource;    // For sound effects

    private void Awake()
    {
        // Ensure that there is only one instance of AudioManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Make AudioManager persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Play a new music track
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource.clip != musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    // Stop the current music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Set music volume (slider connects here)
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    // Play a sound effect
    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }

    // Set sound effects volume (can also add a separate slider for SFX)
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
