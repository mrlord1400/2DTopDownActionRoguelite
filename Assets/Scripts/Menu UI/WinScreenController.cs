using UnityEngine;
using UnityEngine.SceneManagement; 

public class WinScreenController : MonoBehaviour
{
    // Hàm này gọi khi bấm nút Restart
    public void RestartNewGame()
    {
        // Mở khóa thời gian trước khi chuyển Scene
        Time.timeScale = 1f; 
        
        
        SceneManager.LoadScene("Scene_Lobby"); 
    }

    // Hàm này gọi khi bấm nút Menu
    public void GoToMenu()
    {
        // Mở khóa thời gian trước khi chuyển Scene
        Time.timeScale = 1f; 
        
        
        SceneManager.LoadScene("Menu");
    }
}