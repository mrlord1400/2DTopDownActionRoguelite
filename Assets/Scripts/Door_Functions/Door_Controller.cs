using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator animator;
    public bool isLocked = false; // Biến kiểm tra cửa có đang bị khóa hay không
    private Collider2D blockingCollider; // Collider dùng để chặn người chơi đi xuyên qua

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Tự động tìm Collider không phải là Trigger (dùng để chặn vật lý)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (!col.isTrigger)
            {
                blockingCollider = col;
                break;
            }
        }
    }

    // Khi Player bước VÀO vùng Collider xanh -> Mở cửa (Nếu không bị khóa)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player đã chạm vào vùng Cảm Biến Mở Cửa! isLocked đang là: " + isLocked);
            if (!isLocked)
            {
                animator.SetBool("isOpen", true);
                if (blockingCollider != null) blockingCollider.enabled = false; // Tắt bức tường vật lý để Player đi qua
            }
        }
    }

    // Khi Player bước QUA và RA KHỎI vùng Collider xanh -> Đóng cửa ngay
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("isOpen", false);
            if (blockingCollider != null) blockingCollider.enabled = true; // Bật lại bức tường vật lý
        }
    }

    // Hàm dùng để Khóa và Đóng cửa
    public void LockDoor()
    {
        isLocked = true;
        animator.SetBool("isOpen", false); // Bắt buộc đóng cửa
        
        if (blockingCollider != null) 
        {
            blockingCollider.enabled = true; // Bật bức tường tàng hình chặn lại
        }
    }

    // Hàm dùng để Mở khóa cửa
    public void UnlockDoor()
    {
        isLocked = false;
        animator.SetBool("isOpen", true); // Tự động mở cửa báo hiệu đã clear phòng
        
        if (blockingCollider != null) 
        {
            blockingCollider.enabled = false; // Tắt bức tường đi để cho qua
        }
    }
}