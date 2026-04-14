using UnityEngine;
using EasyTransition;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject pauseMenu;      // buttons (Resume, Settings, Quit)
    public GameObject settingsPanel;  // same settings UI as main menu

    public TransitionSettings transition;

    bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
{
    isPaused = true;

    pausePanel.SetActive(true);
    pauseMenu.SetActive(true);
    settingsPanel.SetActive(false);

    Time.timeScale = 0f;

    // 🔊 PAUSE AUDIO
    AudioManager.Instance?.PauseAllAudio();

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

   public void Resume()
{
    isPaused = false;

    pausePanel.SetActive(false);

    Time.timeScale = 1f;

    // 🔊 RESUME AUDIO
    AudioManager.Instance?.ResumeAllAudio();
    PlayerController.Instance?.ResetFootsteps();


    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

    // 🔥 NEW → OPEN SETTINGS
    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // 🔥 NEW → BACK TO PAUSE MENU
    public void BackToPause()
    {
        settingsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void GoToMenu()
    {
        StartCoroutine(GoToMenuRoutine());
    }

    IEnumerator GoToMenuRoutine()
    {
        Time.timeScale = 1f;
        yield return null;

        var tm = EasyTransition.TransitionManager.Instance();

        if (tm != null && transition != null)
        {
            tm.Transition("MainMenu", transition, 0f);
        }
        else
        {
            Debug.LogError("❌ Transition failed → fallback");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    
}