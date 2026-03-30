using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Elementum"); // your scene name
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // works in editor
    }
}