using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Added for Tilemap support

/// <summary>
/// Defines the different types of rooms that can exist in a stage.
/// </summary>
public enum RoomType
{
    /// <summary>The starting room where the player begins.</summary>
    Start,
    /// <summary>A standard room, often containing enemies or puzzles.</summary>
    Normal,
    /// <summary>A room containing a boss encounter.</summary>
    Boss,
    /// <summary>A room containing treasure or items for the player.</summary>
    Treasure,
    /// <summary>A room where the player can buy items.</summary>
    Shop,
    // Add more as needed (e.g., Secret, Challenge, etc.)
}

/// <summary>
/// Abstract base class for all room types in the game.
/// Provides common functionality and properties for rooms.
/// </summary>
/// <remarks>
/// Each room is associated with a <see cref="RoomTemplate"/> and has a position on the grid.
/// It manages its cleared status and references to its doors.
/// It also expects a <see cref="RoomContext"/> component on the same GameObject to access tilemaps.
/// </remarks>
public abstract class Room : MonoBehaviour
{
    /// <summary>
    /// The template defining the layout and potential contents of this room.
    /// </summary>
    [Tooltip("The RoomTemplate asset that defines this room's structure and content.")]
    public RoomTemplate template;

    /// <summary>
    /// The position of this room within the stage grid.
    /// </summary>
    [Tooltip("The grid coordinates of this room within the stage.")]
    public Vector2Int gridPosition;

    /// <summary>
    /// Indicates whether the room has been cleared (e.g., all enemies defeated, puzzle solved).
    /// </summary>
    [Tooltip("Is this room considered cleared (e.g., enemies defeated, puzzle solved)?")]
    public bool isCleared = false;

    /// <summary>
    /// A dictionary mapping door directions to their <see cref="DoorController"/> instances.
    /// </summary>
    [Tooltip("References to the door controllers for this room, indexed by direction.")]
    public Dictionary<Direction, DoorController> doors = new();

    /// <summary>
    /// Reference to the Tilemap used for rendering walls in this room.
    /// Populated from <see cref="RoomContext"/> during Awake.
    /// </summary>
    [Tooltip("Reference to the Walls Tilemap. Set via RoomContext.")]
    [HideInInspector] // Hide this from the Room component inspector as it's set via RoomContext
    public Tilemap WallsTilemap;

    private RoomContext _roomContext;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    /// <remarks>
    /// Initializes references to <see cref="RoomContext"/> and its associated tilemaps.
    /// Logs errors if the <see cref="RoomContext"/> or required tilemaps are not found.
    /// </remarks>
    protected virtual void Awake()
    {
        _roomContext = GetComponent<RoomContext>();

        if (_roomContext != null)
        {
            WallsTilemap = _roomContext.WallsTilemap;

            if (WallsTilemap == null)
            {
                Debug.LogError($"[Room] '{gameObject.name}': The WallsTilemap is not assigned in the RoomContext component. " +
                               "Please assign it on the Room prefab.", this);
            }
        }
        else
        {
            // If this room type *requires* a RoomContext (most will for tilemaps), log an error.
            // Some very simple rooms might not, but it's good practice to have it.
            Debug.LogError($"[Room] '{gameObject.name}': RoomContext component not found. This is required for WallsTilemap.", this);
        }
    }

    /// <summary>
    /// Called when the player character enters this room.
    /// </summary>
    /// <remarks>
    /// This is a virtual method that can be overridden by derived room types
    /// to implement specific behaviors upon player entry (e.g., spawning enemies, locking doors).
    /// The base implementation logs the entry event.
    /// </remarks>
    public virtual void OnPlayerEnter()
    {
        Debug.Log($"[Room] Player entered {gameObject.name} of type {template.roomType} at {gridPosition}");
    }

    /// <summary>
    /// Called when the player character exits this room.
    /// </summary>
    /// <remarks>
    /// This is a virtual method that can be overridden by derived room types
    /// to implement specific behaviors upon player exit.
    /// The base implementation logs the exit event.
    /// </remarks>
    public virtual void OnPlayerExit()
    {
        Debug.Log($"[Room] Player exited {gameObject.name}");
    }

    /// <summary>
    /// Called when the conditions for clearing the room have been met.
    /// </summary>
    /// <remarks>
    /// This method sets <see cref="isCleared"/> to true and logs the event.
    /// Derived classes can override this to add specific behaviors like unlocking doors.
    /// </remarks>
    public virtual void OnRoomClear()
    {
        if (isCleared) return;
        isCleared = true;
        OpenDoors();
        Debug.Log($"[Room] {gameObject.name} cleared!");
    }

    protected virtual void CloseDoors()
    {
        Debug.Log($"[Room] Closing doors in {gameObject.name}.");
        foreach (var door in doors.Values)
        {
            door.Close();
        }
    }

    protected virtual void OpenDoors()
    {
        Debug.Log($"[Room] Opening doors in {gameObject.name}.");
        foreach (var door in doors.Values)
        {
            door.Open();
        }
    }

    protected virtual void LockDoors()
    {
        Debug.Log($"[Room] Locking doors in {gameObject.name}.");
        foreach (var door in doors.Values)
        {
            door.Lock();
        }
    }

    protected virtual void UnlockDoors()
    {
        Debug.Log($"[Room] Unlocking doors in {gameObject.name}.");
        foreach (var door in doors.Values)
        {
            door.Unlock();
        }
    }
}
