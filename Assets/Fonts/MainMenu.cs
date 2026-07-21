using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsMenuPanel;
    public void PlayGame()
    {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);  
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Options()
    {
        // This turns on the options menu panel
        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(true);
        }
    }
}
