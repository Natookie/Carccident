using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip backgroundMusic;

    [Header("SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip trafficLightChangeSFX;
    public AudioClip carPassSFX;
    public AudioClip crashSFX;
    public AudioClip gameOverSFX;
    public AudioClip upgradeSFX;
    public AudioClip notEnoughMoneySFX;

    [Header("Volume")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (backgroundMusic == null || bgmSource == null) return;

        bgmSource.clip = backgroundMusic;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlayTrafficLightChange()
    {
        PlaySFX(trafficLightChangeSFX);
    }

    public void PlayCarPass()
    {
        PlaySFX(carPassSFX);
    }

    public void PlayCrash()
    {
        PlaySFX(crashSFX);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSFX);
    }

    public void PlayUpgrade()
    {
        PlaySFX(upgradeSFX);
    }

    public void PlayNotEnoughMoney()
    {
        PlaySFX(notEnoughMoneySFX);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }
}