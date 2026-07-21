using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransitionSave : MonoBehaviour
{
    [Header("Kéo Panel UI Chuyển Màn vào đây")]
    public GameObject transitionUIPanel;

    [Header("Số thứ tự (Build Index) của Map tiếp theo")]
    public int nextSceneIndex; // Em gõ số của Scene Map 2 vào đây trong Inspector

    // Bật UI khi chạm vào cửa
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            transitionUIPanel.SetActive(true);
        }
    }

    // Tự động tắt UI nếu người chơi bỏ đi xa khỏi cửa
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            transitionUIPanel.SetActive(false);
        }
    }

    // Gắn vào nút "Lưu & Đi tiếp"
    public void SaveAndGoNext()
    {
        // Lưu lại vị trí là Màn Tiếp Theo (để Load game là vô luôn map mới)
        PlayerPrefs.SetInt("SavedScene", nextSceneIndex);
        PlayerPrefs.Save();
        
        // Chuyển sang màn tiếp theo
        SceneManager.LoadScene(nextSceneIndex);
    }

    // Gắn vào nút "Không lưu & Đi tiếp"
    public void SkipSaveAndGoNext()
    {
        SceneManager.LoadScene(nextSceneIndex);
    }

    // Gắn vào nút "Ở lại"
    public void Cancel()
    {
        transitionUIPanel.SetActive(false);
    }
}