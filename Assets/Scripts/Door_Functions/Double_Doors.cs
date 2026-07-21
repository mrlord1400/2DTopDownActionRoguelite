using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoubleDoorSync : MonoBehaviour
{
    private Animator[] childAnimators;

    void Start()
    {
        // Tự động tìm tất cả Animator của các Object con (2 cánh cửa nhỏ)
        childAnimators = GetComponentsInChildren<Animator>();
    }

    // Khi Player bước vào vùng nhận diện (Collider có tích Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu đối tượng chạm vào có Tag là "Player"
        if (collision.CompareTag("Player"))
        {
            SetDoorsOpenState(true);
        }
    }

    // Khi Player bước ra khỏi vùng nhận diện
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Kiểm tra nếu đối tượng rời đi có Tag là "Player"
        if (collision.CompareTag("Player"))
        {
            SetDoorsOpenState(false);
        }
    }

    // Hàm phụ trợ để gửi trạng thái đóng/mở tới tất cả các Animator con
    private void SetDoorsOpenState(bool isOpen)
    {
        foreach (Animator anim in childAnimators)
        {
            if (anim != null)
            {
                anim.SetBool("isOpen", isOpen);
            }
        }
    }
}