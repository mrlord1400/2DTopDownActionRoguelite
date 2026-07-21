using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình Hệ thống Spawner")]
    public List<GameObject> enemyPrefabs;  // Kéo nhiều loại quái khác nhau vào danh sách này
    public int numberOfEnemies = 3;  // Tổng số lượng quái muốn sinh ra trong phòng này

    [Header("Danh sách Cửa của phòng này")]
    public List<DoorController> doors; // Kéo thả các cánh cửa vào đây

    [Header("Cài đặt Nhạc Boss (Bỏ trống nếu là phòng quái thường)")]
    public AudioClip bossMusic; // Nhạc đánh Boss
    [Range(0f, 1f)] public float bossMusicVolume = 0.5f; // THANH KÉO CHỈNH ÂM LƯỢNG (0 đến 1)
    public AudioSource bgmSource; // Nguồn phát nhạc nền (Kéo cục Background Music vào đây)
    private AudioClip originalMusic; // Lưu lại nhạc cũ
    private float originalVolume; // Lưu lại mức âm lượng của nhạc nền cũ

    private BoxCollider2D col;
    private bool hasSpawned = false; 
    private bool isRoomCleared = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Nếu đã spawn quái và phòng chưa được clear
        if (hasSpawned && !isRoomCleared)
        {
            // Kiểm tra xem tất cả quái vật đã bị tiêu diệt chưa (GameObject bị Destroy sẽ biến thành null)
            spawnedEnemies.RemoveAll(enemy => enemy == null);

            if (spawnedEnemies.Count == 0)
            {
                isRoomCleared = true;
                UnlockAllDoors();
                
                // Trả lại nhạc cũ khi đã giết xong quái/boss
                if (bossMusic != null && bgmSource != null && originalMusic != null)
                {
                    bgmSource.clip = originalMusic;
                    bgmSource.volume = originalVolume; // Trả lại mức volume gốc
                    bgmSource.Play();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasSpawned)
        {
            LockAllDoors();
            SpawnEnemies();
            hasSpawned = true; 
            
            // Đổi sang nhạc Boss nếu có cài đặt
            if (bossMusic != null && bgmSource != null)
            {
                originalMusic = bgmSource.clip; // Lưu lại bài nhạc đang phát
                originalVolume = bgmSource.volume; // Lưu lại mức volume cũ
                bgmSource.clip = bossMusic;     // Đổi bài mới
                bgmSource.volume = bossMusicVolume; // Áp dụng mức volume mới cho Boss
                bgmSource.Play();               // Bắt đầu phát
            }
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return; // Tránh lỗi nếu bạn quên kéo Prefab vào

        for (int i = 0; i < numberOfEnemies; i++)
        {
            float randomX = Random.Range(col.bounds.min.x, col.bounds.max.x);
            float randomY = Random.Range(col.bounds.min.y, col.bounds.max.y);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            // Bốc thăm ngẫu nhiên 1 loại quái trong danh sách bạn đã cung cấp
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

            GameObject enemy = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
            spawnedEnemies.Add(enemy); // Lưu lại để theo dõi
        }
    }

    void LockAllDoors()
    {
        foreach (DoorController door in doors)
        {
            if (door != null) door.LockDoor();
        }
    }

    void UnlockAllDoors()
    {
        foreach (DoorController door in doors)
        {
            if (door != null) door.UnlockDoor();
        }
    }

}