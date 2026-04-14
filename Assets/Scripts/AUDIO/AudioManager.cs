using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    Coroutine musicRoutine;

    float currentBaseVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
{
    ApplyVolumes();
    RefreshMusicVolume(); // 🔥 ADD THIS
}

    // 🎚 APPLY VOLUMES
    public void ApplyVolumes()
    {
        if (SettingsManager.Instance == null) return;

        float master = SettingsManager.Instance.GetMasterVolume();
        float music = SettingsManager.Instance.GetMusicVolume();
        float sfx = SettingsManager.Instance.GetSFXVolume();

        musicSource.volume = master * music * currentBaseVolume;
        sfxSource.volume = master * sfx;
    }

    // 🎵 PLAY MUSIC WITH FADE
    public void PlayMusicWithFade(AudioClip newClip, float fadeOutTime, float delay, float fadeInTime, float baseVolume = 1f)
    {
        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        currentBaseVolume = baseVolume;

        musicRoutine = StartCoroutine(MusicTransition(newClip, fadeOutTime, delay, fadeInTime));
    }

    // 🎵 SIMPLE PLAY
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        ApplyVolumes();
    }

    IEnumerator MusicTransition(AudioClip newClip, float fadeOutTime, float delay, float fadeInTime)
    {
        float startVolume = musicSource.volume;
        float t = 0f;

        // 🔻 FADE OUT
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        musicSource.volume = 0f;

        // 🔁 SWITCH CLIP
        musicSource.clip = newClip;
        musicSource.Play();
        musicSource.Pause();

        // ⏳ DELAY
        float waitTimer = 0f;
        while (waitTimer < delay)
        {
            waitTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        musicSource.UnPause();

        float targetVolume = GetFinalMusicVolume();

        t = 0f;

        // 🔺 FADE IN
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeInTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // 🔊 SFX
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume);
    }

    // ⏸ PAUSE
    public void PauseAllAudio()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();

        sfxSource.Pause();
    }

    // ▶ RESUME
    public void ResumeAllAudio()
    {
        musicSource.UnPause();
        RefreshMusicVolume();
        sfxSource.UnPause();
    }

    // 🎚 FINAL VOLUME CALC
    float GetFinalMusicVolume()
    {
        if (SettingsManager.Instance == null) return 1f;

        return
            SettingsManager.Instance.GetMasterVolume() *
            SettingsManager.Instance.GetMusicVolume() *
            currentBaseVolume;
    }

    // 🔄 REFRESH (CALLED BY SETTINGS)
    public void RefreshMusicVolume()
    {
        if (musicSource == null) return;

        float v = GetFinalMusicVolume();
        Debug.Log("Music Volume Set To: " + v);

        musicSource.volume = v;
    }

    #if UNITY_EDITOR
void OnValidate()
{
    if (!Application.isPlaying) return;

    RefreshMusicVolume();
}
#endif


}