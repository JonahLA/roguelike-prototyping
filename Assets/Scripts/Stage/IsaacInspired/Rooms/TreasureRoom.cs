using UnityEngine;

/// <summary>
/// Represents a treasure room where the player can find valuable items or rewards.
/// Treasure rooms typically contain special loot and may be cleared upon entry or after collecting items.
/// </summary>
public class TreasureRoom : Room
{
    // TODO: Implement treasure spawning system
    // /// <summary>
    // /// The prefab to spawn as treasure in this room.
    // /// </summary>
    // [Tooltip("The prefab to spawn as treasure in this room.")]
    // public GameObject treasurePrefab;
    // private bool _treasureSpawned = false;

    /// <summary>
    /// Called when the player enters the treasure room.
    /// Handles treasure spawning logic and room clearing behavior.
    /// </summary>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();

        if (isCleared) return;
        OnRoomClear(); // treasure rooms are often cleared on entry or after taking item
        // SpawnTreasure();
    }
    
    // TODO: Implement treasure spawning method
    // /// <summary>
    // /// Spawns treasure items in the room at designated spawn points.
    // /// </summary>
    // private void SpawnTreasure() 
    // { 
    //     // Implementation for spawning treasure items
    // }
}
