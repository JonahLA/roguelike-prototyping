using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the template for a room, including its layout, content, and properties.
/// Used as a ScriptableObject to create various room configurations.
/// </summary>
[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Roguelike/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [Header("Room Information")]
    /// <summary>
    /// The display name of the room.
    /// </summary>
    [Tooltip("The display name of the room.")]
    public string roomName = "New Room";

    /// <summary>
    /// The type of this room (e.g., Normal, Boss, Shop).
    /// </summary>
    [Tooltip("The type of this room (e.g., Normal, Boss, Shop).")]
    public RoomType roomType = RoomType.Normal;

    /// <summary>
    /// Optional image for visualizing the room layout in the editor or tools.
    /// </summary>
    [Tooltip("Optional image for visualizing the room layout in the editor or tools.")]
    public Sprite previewImage;  // optional: for editor visualization

    [Header("Layout")]
    /// <summary>
    /// The dimensions of the room's grid.
    /// </summary>
    [Tooltip("The dimensions of the room's grid.")]
    public Vector2Int gridSize = new(10, 10);

    /// <summary>
    /// The prefab that represents the visual and structural layout of this room.
    /// </summary>
    [Tooltip("The prefab that represents the visual and structural layout of this room.")]
    public GameObject roomPrefab;

    [Header("Door Configuration")]
    /// <summary>
    /// A list of potential door configurations for this room template.
    /// </summary>
    [Tooltip("A list of potential door configurations for this room template.")]
    public List<DoorConfig> possibleDoors = new();

    [Header("Content")]
    /// <summary>
    /// Defines where enemies can spawn within this room.
    /// </summary>
    [Tooltip("Defines where enemies can spawn within this room.")]
    public List<EnemySpawnPoint> enemySpawnPoints = new();

    [Header("Difficulty Settings")]
    /// <summary>
    /// A numerical rating of how difficult this room is.
    /// </summary>
    [Tooltip("A numerical rating of how difficult this room is.")]
    public int difficultyRating = 1;

    /// <summary>
    /// The earliest stage number this room template can appear on.
    /// </summary>
    [Tooltip("The earliest stage number this room template can appear on.")]
    public int minStageToAppear = 1;

    /// <summary>
    /// Checks if this room template has a door configuration for the specified direction.
    /// </summary>
    /// <param name="direction">The direction to check for a door.</param>
    /// <returns>True if a door configuration exists for the given direction, false otherwise.</returns>
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

/// <summary>
/// Configuration for a door within a room template.
/// </summary>
[System.Serializable]
public class DoorConfig
{
    /// <summary>
    /// The direction this door faces (North, South, East, West).
    /// </summary>
    [Tooltip("The direction this door faces (North, South, East, West).")]
    public Direction direction;

    /// <summary>
    /// The grid position of this door within the room.
    /// </summary>
    [Tooltip("The grid position of this door within the room.")]
    public Vector2Int position;

    /// <summary>
    /// Whether this door is optional or guaranteed to exist if the room layout allows.
    /// </summary>
    [Tooltip("Whether this door is optional or guaranteed to exist if the room layout allows.")]
    public bool isOptional = false;
}

/// <summary>
/// Defines a point where enemies can spawn within a room.
/// </summary>
[System.Serializable]
public class EnemySpawnPoint
{
    /// <summary>
    /// The position where enemies will spawn, relative to the room's origin.
    /// </summary>
    [Tooltip("The position where enemies will spawn, relative to the room's origin.")]
    public Vector2 localPosition; // Position relative to the room\'s origin

    /// <summary>
    /// The prefab of the enemy to be spawned at this point.
    /// </summary>
    [Tooltip("The prefab of the enemy to be spawned at this point.")]
    public GameObject enemyPrefab;  // Prefab of the enemy to spawn

    /// <summary>
    /// The number of enemies of this type to spawn here.
    /// </summary>
    [Tooltip("The number of enemies of this type to spawn here.")]
    public int count = 1;           // Number of enemies of this type to spawn at this point

    /// <summary>
    /// If count > 1, enemies will spawn randomly within this radius around the localPosition.
    /// </summary>
    [Tooltip("If count > 1, enemies will spawn randomly within this radius around the localPosition.")]
    public float spawnRadius = 0f; // If count > 1, spawn within this radius around localPosition
}
