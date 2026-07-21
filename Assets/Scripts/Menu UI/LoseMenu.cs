using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenu : MonoBehaviour
{
    // Keep track of whether the game is currently in a "lost" state
    public static bool isDead = false;

    // Drag your Lose Menu UI Panel GameObject here in the Inspector
    public GameObject loseMenuUI;

    // Reference to the player's stats script (drag it in the Inspector)
    public PlayerStats playerStats;

    [Header("Death Sequence")]
    [Tooltip("Thời gian chờ (giây) trước khi hiện Lose Menu")]
    [SerializeField] private float deathMenuDelay = 2f;

    void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath += HandleDeath;
        }
    }

    void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath -= HandleDeath;
        }
    }

    void HandleDeath()
    {
        if (isDead) return; // Prevent this from firing more than once
        isDead = true;

        StartCoroutine(ShowLoseMenuAfterDelay());
    }

    private IEnumerator ShowLoseMenuAfterDelay()
    {
        // Time.timeScale is still 1f here, so the death animation plays normally
        yield return new WaitForSeconds(deathMenuDelay);

        loseMenuUI.SetActive(true);   // Show the lose menu
        Time.timeScale = 0f;          // Freeze game time now, after the animation has finished
    }

    public void Restart()
    {
        Time.timeScale = 1f;          // CRITICAL: Unfreeze time before reloading the scene!
        isDead = false;
        SceneManager.LoadScene("Scene_Lobby");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;          // CRITICAL: Unfreeze time before switching scenes!
        isDead = false;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}