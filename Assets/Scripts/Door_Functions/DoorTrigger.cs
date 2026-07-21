using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Kéo UI WinScreen_Panel vào đây")]
    public GameObject winScreenUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người chạm vào cửa có phải là Player không
        if (collision.CompareTag("Player"))
        {
            // Bật màn hình Win lên
            winScreenUI.SetActive(true);
            
            // Dừng thời gian trong game lại (nếu cần)
            Time.timeScale = 0f; 
        }
    }
}