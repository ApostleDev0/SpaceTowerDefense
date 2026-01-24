using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }
    #endregion

    #region Audio Sources & Settings
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.3f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    #endregion

    #region Audio Clips
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    public AudioClip buttonClickClip;
    public AudioClip buttonHoverClip;
    public AudioClip pauseClip;
    public AudioClip unpauseClip;
    public AudioClip speedSlowClip;
    public AudioClip speedNormalClip;
    public AudioClip speedFastClip;
    public AudioClip panelToggleClip;
    public AudioClip warningClip;

    public AudioClip towerPlacedClip;
    public AudioClip enemyDestroyedClip;
    public AudioClip missionCompleteClip;
    public AudioClip gameOverClip;
    #endregion

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyVolumeSettings();
        }
    }
    private void ApplyVolumeSettings()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    //====PUBLIC
    public void PlaySound(AudioClip clip)
    {
        // check clip
        if(clip == null)
        {
            return;
        }
        sfxSource.PlayOneShot(clip,sfxVolume);
    }
    public void PlayMusic(AudioClip clip)
    {
        // check clip
        if(clip == null)
        {
            return;
        }
        if(musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    //====PUBLIC

    // Gameplay
    public void PlayTowerPlaced() => PlaySound(towerPlacedClip);
    public void PlayEnemyDestroyed() => PlaySound(enemyDestroyedClip);
    public void PlayMissionComplete() => PlaySound(missionCompleteClip);
    public void PlayGameOver() => PlaySound(gameOverClip);

    // UI
    public void PlayButtonClick() => PlaySound(buttonClickClip);
    public void PlayButtonHover() => PlaySound(buttonHoverClip);
    public void PlayPause() => PlaySound(pauseClip);
    public void PlayUnPause() => PlaySound(unpauseClip);
    public void PlaySpeedSlow() => PlaySound(speedSlowClip);
    public void PlaySpeedNormal() => PlaySound(speedNormalClip);
    public void PlaySpeedFast() => PlaySound(speedFastClip);
    public void PlayWarning() => PlaySound(warningClip);
    public void PlayPanelToggle() => PlaySound(panelToggleClip);

}
