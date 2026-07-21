using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    // Cơ chế Singleton thông minh: Tự động tìm hoặc tạo mới GameManager nếu chưa có trong Scene
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tìm kiếm GameManager sẵn có trong Scene
                _instance = FindFirstObjectByType<GameManager>();

                // Nếu không tìm thấy, tự động tạo ra một GameObject mới chứa GameManager
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager_AutoCreated");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Tiến Trình Game")]
    // Lưu tên scene dungeon hiện tại mà người chơi đang ở (Mặc định bắt đầu từ màn 1)
    public string currentDungeonScene = "Scene_Dungeon (1)"; 

    private void Awake()
    {
        // Đảm bảo chỉ tồn tại một GameManager duy nhất xuyên suốt trò chơi
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Cập nhật màn chơi tiếp theo vào bộ nhớ khi clear xong một Dungeon
    /// </summary>
    public void UpdateNextDungeon(string nextSceneName)
    {
        currentDungeonScene = nextSceneName;
        Debug.Log("Đã lưu tiến trình màn chơi mới: " + currentDungeonScene);
    }

    /// <summary>
    /// Gọi hàm này khi Player bị chết để đưa về Lobby
    /// </summary>
    public void PlayerDied()
    {
        Debug.Log("Player đã chết! Đang đưa về sảnh Lobby. Giữ nguyên tiến trình: " + currentDungeonScene);
        SceneManager.LoadScene("Scene_Lobby"); 
    }

    /// <summary>
    /// Khởi động lại tiến trình game về màn 1 (Dùng khi phá đảo hoặc bấm New Game)
    /// </summary>
    public void ResetGame()
    {
        currentDungeonScene = "Scene_Dungeon (1)";
    }
}