using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Keep track of whether the game is currently paused
    public static bool isPaused = false;

    // Drag your Pause Menu UI Panel GameObject here in the Inspector
    public GameObject pauseMenuUI;

    void Update()
    {
        // Checks if the player pressed the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the menu
        Time.timeScale = 1f;          // Unfreeze game time
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);  // Show the menu
        Time.timeScale = 0f;          // Freeze game time (stops physics, animations, etc.)
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;          // CRITICAL: Unfreeze time before switching scenes!
        isPaused = false;
        SceneManager.LoadScene("Menu"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}