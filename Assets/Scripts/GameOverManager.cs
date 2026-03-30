using UnityEngine;
using EasyTransition;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    public GameObject deathPanel;
    public TransitionSettings transition;
    public float deathPanelDelay = 0.8f;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        Time.timeScale = 0.2f; // slow motion
        StartCoroutine(ShowDeathPanelDelayed());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator ShowDeathPanelDelayed()
    {
        yield return new WaitForSecondsRealtime(deathPanelDelay);
        deathPanel.SetActive(true);
    }

   public void Respawn()
{
    StartCoroutine(RespawnRoutine());
}

IEnumerator RespawnRoutine()
{
    Time.timeScale = 1f;

    yield return null;

    var tm = EasyTransition.TransitionManager.Instance();

    if (tm != null && transition != null)
    {
        tm.Transition(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            transition,
            0f
        );
    }
    else
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(MenuRoutine());
    }

    IEnumerator MenuRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        var tm = TransitionManager.Instance();

        if (tm != null && transition != null)
        {
            tm.Transition("MainMenu", transition, 0f);
        }
        else
        {
            Debug.LogError("❌ Transition failed → fallback load");
            SceneManager.LoadScene("MainMenu");
        }
    }
}