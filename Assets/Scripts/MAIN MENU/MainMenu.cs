using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject statsPanel;
    public GameObject settingsPanel;
    public GameObject infoPanel; // ✅ ADD THIS

    [Header("Audio")]
    public AudioClip menuMusic; // 🎵 ADD THIS

    public Texture2D cursorTexture;
    public Vector2 hotspot;

    void Start()
    {
        ApplyCursor();
        // 🎵 PLAY MENU MUSIC
    AudioManager.Instance.PlayMusic(menuMusic);
    }

    void ApplyCursor()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("🎯 Cursor Applied: " + cursorTexture.name);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Elementum");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    // ================= OPEN PANELS =================

    public void OpenStats()
    {
        mainPanel.SetActive(false);
        statsPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenInfo() // ✅ ADD THIS
    {
        mainPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    // ================= BACK =================

    public void BackToMain()
    {
        statsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        infoPanel.SetActive(false); // ✅ ADD THIS
        mainPanel.SetActive(true);
    }
}