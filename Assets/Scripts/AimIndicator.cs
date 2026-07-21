using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public Transform playerTransform;

    [Header("Settings")]
    public float radius = 1f;
    
    void Start()
    {
        
    }

    void LateUpdate()
    {
        // Thêm dòng này để kiểm tra xem biến đã được kéo thả trong Inspector chưa
        if (playerController == null || playerTransform == null)
        {
            Debug.LogWarning("AimIndicator: Bạn chưa gán Player Controller hoặc Player Transform trong Inspector!");
            return; // Thoát khỏi hàm để ngăn lỗi NullReferenceException xảy ra
        }

        // 1. Get the mouse position from your existing input script
        Vector2 mousePos = playerController.GetMouseWorldPosition();

        // 2. Calculate the raw direction vector from player to mouse
        Vector2 rawDirection = mousePos - (Vector2)playerTransform.position;

        // 3. Normalize the vector (forces length to 1) and multiply by radius
        Vector2 offset = rawDirection.normalized * radius;

        // 4. Apply the position (Player Position + Offset)
        transform.position = (Vector2)playerTransform.position + offset;

        // 5. Calculate the angle and apply rotation (so the arrow points outward)
        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}