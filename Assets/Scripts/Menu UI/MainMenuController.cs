using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Bắt buộc phải có để dùng UI chữ mới của Unity

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject selectModePanel;
    public GameObject nameInputPanel;
    
    [Header("Input")]
    public TMP_InputField nameInputField;

    // 1. Hàm gọi khi bấm nút Play ở Main Menu ngoài cùng
    public void OpenSelectMode()
    {
        selectModePanel.SetActive(true);
    }

    public void CloseSelectMode()
    {
        selectModePanel.SetActive(false);
    }

    // 2. Hàm gọi khi bấm New Game
    public void OpenNameInput()
    {
        selectModePanel.SetActive(false);
        nameInputPanel.SetActive(true);
    }

    // 3. Hàm gọi khi nhập tên xong và bấm Start
    public void StartNewGame()
    {
        string playerName = nameInputField.text;
        
        // Kiểm tra xem người chơi có nhập tên chưa (không được để trống)
        if(!string.IsNullOrEmpty(playerName))
        {
            // Lưu tên người chơi
            PlayerPrefs.SetString("PlayerName", playerName);
            
            // Lưu mốc Scene bắt đầu (Giả sử Scene Level 1 có Index là 1 trong Build Settings)
            PlayerPrefs.SetInt("SavedScene", 1); 
            PlayerPrefs.Save();
            
            // Load vào màn 1
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogWarning("Vui lòng nhập tên trước khi chơi!");
        }
    }

    // 4. Hàm gọi khi bấm Load Game
    public void LoadGame()
    {
        // Kiểm tra xem có dữ liệu save cũ không
        if(PlayerPrefs.HasKey("SavedScene"))
        {
            // Lấy tên Scene đã lưu và Load
            int sceneToLoad = PlayerPrefs.GetInt("SavedScene");
            
            // Có thể in ra tên người chơi để test
            string savedName = PlayerPrefs.GetString("PlayerName");
            Debug.Log("Chào mừng " + savedName + " quay trở lại!");
            
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Chưa có file Save nào!");
            // UI Designer: Em có thể hiện một dòng Text thông báo lỗi ra màn hình ở đây
        }
    }

    public void BackToSelectMode()
    {
        // Tắt bảng nhập tên đi
        nameInputPanel.SetActive(false);
        
        // Bật lại bảng chọn chế độ lên
        selectModePanel.SetActive(true);
    }
}