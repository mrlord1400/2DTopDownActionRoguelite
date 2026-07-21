using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Đặt component này trên mỗi Door Anchor (object con của Room, nằm đúng vị trí
/// 1 trong 4 cạnh N/S/E/W). Khi Generator quyết định hướng này có room kề nối sang
/// hay không, ta gọi SetOpen(true/false) để:
///   - true  -> bật đường đi (vô hiệu hoá wallBlocker tile, hoặc bật trigger chuyển room)
///   - false -> bịt lại bằng wallBlocker (1 đoạn Wall tile đặt sẵn ngay tại cửa)
/// </summary>
public class DoorAnchor : MonoBehaviour
{
    [Header("Tham chiếu tuỳ chọn")]
    [Tooltip("GameObject chứa tile/wall dùng để BỊT cửa khi không có room kề (vd: 1 đoạn Wall Tilemap hoặc collider).")]
    public GameObject wallBlocker;

    [Tooltip("GameObject chứa trigger collider để phát hiện player đi qua, dùng để load room kế tiếp (tuỳ chọn).")]
    public GameObject passageTrigger;

    /// <summary>True nếu cửa này đang mở (có room kề nối sang).</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Vị trí world của cửa này, dùng để Generator tính khoảng cách/kiểm tra khớp hướng.</summary>
    public Vector3 WorldPosition => transform.position;

    public void SetOpen(bool open)
    {
        IsOpen = open;

        if (wallBlocker != null)
            wallBlocker.SetActive(!open); // mở cửa thì tắt block tường, đóng thì bật tường bịt lại

        if (passageTrigger != null)
            passageTrigger.SetActive(open);
    }
}