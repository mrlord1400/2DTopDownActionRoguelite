using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    [Header("Cấu hình loại cửa")]
    [Tooltip("Tích chọn nếu đây là cánh cửa đặt ở sảnh Lobby")]
    public bool isLobbyDoor = false; 

    [Header("Tên Màn Tiếp Theo")]
    [Tooltip("Chỉ điền ô này nếu cửa đặt trong Dungeon (Ví dụ ở Dungeon 1 điền: scene_dungeon(2))")]
    public string nextSceneName; 

    private Animator animator;
    private bool isPlayerInZone = false;
    private Coroutine teleportCoroutine; // Khai báo biến Coroutine để dễ dàng hủy khi Player đi ra ngoài

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy component Animator trên " + gameObject.name);
        }
        else
        {
            // Fix luôn lỗi cửa tự mở toang khi vừa play game
            animator.SetBool("isOpen", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = true;
            
            if (animator != null)
            {
                animator.SetBool("isOpen", true);
            }

            // Hủy tiến trình cũ (nếu có) và bắt đầu tiến trình dịch chuyển mới
            if (teleportCoroutine != null) StopCoroutine(teleportCoroutine);
            teleportCoroutine = StartCoroutine(TeleportRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            if (animator != null)
            {
                animator.SetBool("isOpen", false);
            }

            // Hủy tiến trình dịch chuyển lập tức nếu Player đi lùi ra ngoài
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }
    }

    // Dùng IEnumerator (Coroutine) thay cho hàm Teleport() cũ
    private IEnumerator TeleportRoutine()
    {
        // 1. Chờ 0.5 giây để chạy xong animation mở cửa
        yield return new WaitForSeconds(0.5f);

        // 2. Nếu trong lúc chờ mà Player đi ra ngoài thì dừng lại ngay
        if (!isPlayerInZone) yield break;

        string targetScene = "";

        // 3. Xác định Scene cần tới
        if (isLobbyDoor)
        {
            targetScene = GameManager.Instance.currentDungeonScene;
            Debug.Log("Cửa Lobby kích hoạt! Đang chuyển Player tới: " + targetScene);
        }
        else
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogError("Lỗi: Bạn chưa điền tên 'Next Scene Name' cho " + gameObject.name);
                yield break;
            }
            GameManager.Instance.UpdateNextDungeon(nextSceneName);
            targetScene = nextSceneName;
        }

        // 4. KÍCH HOẠT HIỆU ỨNG FADE VÀ CHUYỂN SCENE
        if (SceneFader.instance != null)
        {
            // Gọi Script SceneFader để mờ màn hình rồi mới load scene
            SceneFader.instance.FadeToScene(targetScene);
        }
        else
        {
            // Đề phòng trường hợp Scene này quên chưa kéo FadeCanvas vào thì vẫn load scene bình thường
            Debug.LogWarning("Không tìm thấy SceneFader trong Scene! Đang load trực tiếp...");
            SceneManager.LoadScene(targetScene);
        }
    }
}