using UnityEngine;
using EasyTransition;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pausePanel;
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

    Time.timeScale = 0f;

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

    public void Resume()
{
    isPaused = false;

    pausePanel.SetActive(false);

    Time.timeScale = 1f;

    // ✅ KEEP CURSOR VISIBLE
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

    public void GoToMenu()
{
    StartCoroutine(GoToMenuRoutine());
}

IEnumerator GoToMenuRoutine()
{
    // 🔥 ensure game is unfrozen
    Time.timeScale = 1f;

    yield return null; // wait 1 frame (VERY IMPORTANT)

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