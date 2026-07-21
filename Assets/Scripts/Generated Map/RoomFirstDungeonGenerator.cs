using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;

    [Header("Character Settings")]
    [SerializeField]
    private Transform playerTransform; // Kéo thả Character của bạn vào ô này ở Inspector

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        // Xóa map cũ hoàn toàn trước khi sinh map mới (Tránh lỗi đè dữ liệu)
        tilemapVisualizer.Clear();

        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth, minRoomHeight
        );

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        // 1. Vẽ sàn và tường
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        // 2. CẬP NHẬT COLLIDER (Sửa lỗi đi xuyên tường)
        RefreshTilemapColliders();

        // 3. ĐƯA NHÂN VẬT VỀ MAP MỚI (Sửa lỗi spawn sai vị trí)
        if (playerTransform != null && roomCenters.Count > 0)
        {
            // Lấy tâm căn phòng đầu tiên làm điểm xuất phát
            Vector2Int startRoomCenter = roomCenters[0];

            // Đưa nhân vật về tâm phòng (Cộng thêm 0.5f để nhân vật đứng chính giữa ô vuông)
            playerTransform.position = new Vector3(startRoomCenter.x + 0.5f, startRoomCenter.y + 0.5f, 0);
        }
    }

    private void RefreshTilemapColliders()
    {
        // Tìm Component TilemapCollider2D trên ô Tường
        TilemapCollider2D tilemapCollider = tilemapVisualizer.GetComponentInChildren<TilemapCollider2D>();
        if (tilemapCollider != null)
        {
            // Ép Tilemap Collider của Unity 6 tính toán lại toàn bộ ma trận các ô gạch
            tilemapCollider.ProcessTilemapChanges();
        }

        // Tìm CompositeCollider2D để gộp các ô lại thành đường viền lớn
        CompositeCollider2D compositeCollider = tilemapVisualizer.GetComponentInChildren<CompositeCollider2D>();
        if (compositeCollider != null)
        {
            compositeCollider.GenerateGeometry();
        }
    }

    // --- Giữ nguyên các hàm bổ trợ phía dưới của bạn ---
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;

        // Thêm ô bắt đầu (bao gồm cả ô mở rộng)
        AddDoubleWidthTile(corridor, position);

        // Chạy theo trục Y (Đi lên / Đi xuống)
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            AddDoubleWidthTile(corridor, position);
        }

        // Chạy theo trục X (Đi sang phải / Đi sang trái)
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            AddDoubleWidthTile(corridor, position);
        }

        return corridor;
    }

    // Hàm bổ trợ: Bất kể đường đi ở đâu, luôn tự động thêm 1 ô bên cạnh để tạo thành hành lang 2 ô cố định
    private void AddDoubleWidthTile(HashSet<Vector2Int> corridor, Vector2Int basePosition)
    {
        // Ô gốc của hành lang
        corridor.Add(basePosition);

        // Ô mở rộng cố định (ở đây ta chọn mở rộng sang bên phải/phía trên để tạo độ dày 2 ô)
        corridor.Add(basePosition + Vector2Int.right);
        corridor.Add(basePosition + Vector2Int.up);
        corridor.Add(basePosition + new Vector2Int(1, 1)); // Ô chéo để bù góc rẽ chữ L không bị thắt nút
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
}