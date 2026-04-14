using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

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
        ApplySettings();
    }

   public void ApplySettings()
{
    AudioListener.volume = GetMasterVolume();
    Screen.fullScreen = GetFullscreen();

    AudioManager.Instance?.ApplyVolumes(); // 🔥 ADD THIS
}

    // 🎚 MASTER
public void SetMasterVolume(float value)
{
    PlayerPrefs.SetFloat("MASTER_VOL", value);
    PlayerPrefs.Save();

    ApplySettings(); // 🔥 ADD THIS
}

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MASTER_VOL", 1f);
    }

    // 🎵 MUSIC
 public void SetMusicVolume(float value)
{
    PlayerPrefs.SetFloat("MUSIC_VOL", value);
    PlayerPrefs.Save();

    ApplySettings(); // 🔥 ADD THIS
}

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MUSIC_VOL", 1f);
    }

    // 🔊 SFX
public void SetSFXVolume(float value)
{
    PlayerPrefs.SetFloat("SFX_VOL", value);
    PlayerPrefs.Save();

    ApplySettings(); // 🔥 ADD THIS
}

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFX_VOL", 1f);
    }

    // 🖥 FULLSCREEN
    public void SetFullscreen(bool value)
{
    PlayerPrefs.SetInt("FULLSCREEN", value ? 1 : 0);
    PlayerPrefs.Save();
    Screen.fullScreen = value;
}

    public bool GetFullscreen()
    {
        return PlayerPrefs.GetInt("FULLSCREEN", 1) == 1;
    }
}