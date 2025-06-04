using UnityEngine;

/// <summary>
/// Represents a boss room in the game. Inherits from the base <see cref="Room"/> class.
/// </summary>
/// <remarks>
/// This room type typically initiates a boss encounter when the player enters
/// and may have specific logic for locking doors until the boss is defeated.
/// </remarks>
public class BossRoom : Room
{
    /// <summary>
    /// Called when the player enters this room.
    /// </summary>
    /// <remarks>
    /// If the room has not been cleared, this method logs a message indicating
    /// that boss fight logic and door locking should be implemented.
    /// It calls the base class <see cref="Room.OnPlayerEnter()"/> method.
    /// </remarks>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (!isCleared)
        {
            // LockDoors();
            Debug.Log($"BossRoom {gameObject.name}: Player entered, room not cleared. Implement boss fight/door locking.");
        }
    }

    // Example of a private method that could be used for boss-specific logic.
    // private void LockDoors()
    // {
    //     Debug.Log($"BossRoom {gameObject.name}: Locking doors.");
    //     // Implementation for locking all doors in this room
    //     foreach (var door in doors.Values)
    //     {
    //         if (door != null) door.Lock(); // Assuming a Lock() method exists on DoorController
    //     }
    // }

    // Example of how boss defeat might unlock doors
    // public void OnBossDefeated()
    // {
    //     Debug.Log($"BossRoom {gameObject.name}: Boss defeated, unlocking doors.");
    //     isCleared = true; // Mark room as cleared
    //     // foreach (var door in doors.Values)
    //     // {
    //     //     if (door != null) door.Unlock(); // Assuming an Unlock() method exists on DoorController
    //     // }
    //     // Potentially open a path to the next stage or trigger other events
    // }
}
