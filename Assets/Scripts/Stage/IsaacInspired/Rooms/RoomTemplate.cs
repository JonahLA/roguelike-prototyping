using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Roguelike/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [Header("Room Information")]
    public string roomName = "New Room";
    public RoomType roomType = RoomType.Normal;
    public Sprite previewImage;  // optional: for editor visualization

    [Header("Layout")]
    public Vector2Int gridSize = new(10, 10);
    public GameObject roomPrefab;

    [Header("Door Configuration")]
    public List<DoorConfig> possibleDoors = new();

    [Header("Content")]
    public List<EnemySpawnPoint> enemySpawnPoints = new();
    // public List<ObstaclePoint> obstaclePoints = new();
    // public Transform rewardSpawnPoint;

    [Header("Difficulty Settings")]
    public int difficultyRating = 1;
    public int minStageToAppear = 1;

    public bool HasDoor(Direction direction)
    {
        foreach (var doorConfig in possibleDoors)
        {
            if (doorConfig.direction == direction)
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class DoorConfig
{
    public Direction direction;
    public Vector2Int position;
    public bool isOptional = false;
}

[System.Serializable]
public class EnemySpawnPoint
{
    public Vector2 localPosition; // Position relative to the room's origin
    public GameObject enemyPrefab;  // Prefab of the enemy to spawn
    public int count = 1;           // Number of enemies of this type to spawn at this point
    public float spawnRadius = 0f; // If count > 1, spawn within this radius around localPosition
    // public EnemyTier difficultyTier; // Placeholder for future difficulty scaling
}

// [System.Serializable]
// public class ObstaclePoint
// {
//     // TODO: this
// }
