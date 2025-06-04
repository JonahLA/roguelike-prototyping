using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a standard combat or puzzle room in the game. Inherits from <see cref="Room"/>.
/// </summary>
/// <remarks>
/// Normal rooms typically contain enemies that must be cleared to progress.
/// They may lock doors upon player entry and unlock them once all threats are neutralized.
/// </remarks>
public class NormalRoom : Room
{
    /// <summary>
    /// List to keep track of enemies spawned in this room.
    /// </summary>
    /// <remarks>
    /// This list can be used to check if all enemies have been defeated.
    /// Consider making this private if only managed internally by the room.
    /// </remarks>
    [Tooltip("List of enemies currently active in this room.")]
    [SerializeField] // If you want to see it in inspector for debugging, otherwise can be private
    private List<GameObject> _spawnedEnemies = new();

    // Example: Enemy spawning logic might go here or be triggered by an event
    // public void SpawnEnemies(List<GameObject> enemyPrefabs, RoomContentPopulator populator, float difficulty) 
    // {
    //     // Use the populator to spawn enemies based on a template or specific prefabs
    //     // Add spawned enemies to _spawnedEnemies list
    // }

    /// <summary>
    /// Called when the player enters this room.
    /// </summary>
    /// <remarks>
    /// If the room has not been cleared, this method logs a message indicating that
    /// enemy spawning and door locking logic should be implemented.
    /// It calls the base class <see cref="Room.OnPlayerEnter()"/> method.
    /// </remarks>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (!isCleared)
        {
            // LockDoors(); // Assuming a LockDoors method exists
            // SpawnEnemies(); // Assuming a SpawnEnemies method exists
            Debug.Log($"NormalRoom {gameObject.name}: Player entered, room not cleared. Implement enemy spawning/door locking.");
        }
    }

    // Example of a private method that could be used for room-specific logic.
    // private void LockDoors()
    // {
    //     Debug.Log($"NormalRoom {gameObject.name}: Locking doors.");
    //     // Implementation for locking all doors in this room
    //     foreach (var door in doors.Values)
    //     {
    //         if (door != null) door.Lock(); // Assuming a Lock() method exists on DoorController
    //     }
    // }

    // Example of a private method to unlock doors, typically called when the room is cleared.
    // private void UnlockDoors()
    // {
    //     Debug.Log($"NormalRoom {gameObject.name}: Unlocking doors.");
    //     // Implementation for unlocking all doors
    //     foreach (var door in doors.Values)
    //     {
    //         if (door != null) door.Unlock(); // Assuming an Unlock() method exists on DoorController
    //     }
    // }

    /// <summary>
    /// Called when the room is cleared of all enemies or objectives.
    /// </summary>
    /// <remarks>
    /// This method should be called when the conditions for clearing the room are met (e.g., all enemies defeated).
    /// It calls the base <see cref="Room.OnRoomClear()"/> method and could unlock doors.
    /// </remarks>
    public override void OnRoomClear()
    {
        base.OnRoomClear();
        // UnlockDoors();
        Debug.Log($"NormalRoom {gameObject.name}: Room cleared. Doors should be unlocked.");
    }

    // Example method to check if all enemies are cleared.
    // This would typically be called after an enemy is defeated.
    // private void CheckEnemiesCleared()
    // {
    //     // Remove null entries (defeated enemies) from the list
    //     _spawnedEnemies.RemoveAll(enemy => enemy == null);
    // 
    //     if (_spawnedEnemies.Count == 0 && !isCleared) // Ensure not already cleared
    //     {
    //         OnRoomClear();
    //     }
    // }
}
