using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Added for Tilemap support

public enum RoomType
{
    Start,
    Normal,
    Boss,
    Treasure,
    Shop,
    // Add more as needed (e.g., Secret, Challenge, etc.)
}

public abstract class Room : MonoBehaviour
{
    public RoomTemplate template;
    public Vector2Int gridPosition;
    public bool isCleared = false;

    public Dictionary<Direction, DoorController> doors = new();

    // This field will now be populated from RoomContext
    [HideInInspector] // Hide this from the Room component inspector as it's set via RoomContext
    public Tilemap WallsTilemap;
    
    // Optional: If you also want to access the FloorsTilemap from RoomContext
    // [HideInInspector]
    // public Tilemap FloorsTilemap;

    private RoomContext _roomContext; // Reference to the RoomContext component

    protected virtual void Awake()
    {
        _roomContext = GetComponent<RoomContext>();

        if (_roomContext != null)
        {
            WallsTilemap = _roomContext.WallsTilemap;
            // If you added FloorsTilemap to RoomContext and want to use it:
            // FloorsTilemap = _roomContext.FloorsTilemap;

            if (WallsTilemap == null)
            {
                Debug.LogError($"[Room] '{gameObject.name}': The WallsTilemap is not assigned in the RoomContext component. " +
                               "Please assign it on the Room prefab.", this);
            }
            // Optional: Check for FloorsTilemap
            // if (FloorsTilemap == null && _roomContext.FloorsTilemap != null) // Only error if it was expected
            // {
            //     Debug.LogWarning($"[Room] '{gameObject.name}': The FloorsTilemap is not assigned in the RoomContext component.", this);
            // }
        }
        else
        {
            // If this room type *requires* a RoomContext (most will for tilemaps), log an error.
            // Some very simple rooms might not, but it's good practice to have it.
            Debug.LogError($"[Room] '{gameObject.name}': RoomContext component not found. This is required for WallsTilemap.", this);
        }
    }

    public virtual void OnPlayerEnter()
    {
        // Base implementation, can be overridden by derived classes
        Debug.Log($"[Room] Player entered {gameObject.name} of type {template.roomType} at {gridPosition}");
    }

    public virtual void OnPlayerExit()
    {
        // Base implementation
        Debug.Log($"[Room] Player exited {gameObject.name}");
    }

    public virtual void OnRoomClear()
    {
        isCleared = true;
        // UnlockDoors(); // Assuming an UnlockDoors method exists
        Debug.Log($"[Room] {gameObject.name} cleared!");
        // Potentially notify a game manager or trigger other events
    }

    // Common methods like LockDoors/UnlockDoors could be here if all rooms share the exact same logic,
    // or be abstract/virtual if they differ but should be implemented.
    // For now, specific rooms handle their own locking if needed.
}

// Derived class definitions (StartRoom, NormalRoom, BossRoom, TreasureRoom, ShopRoom) have been moved to their own files.
