using UnityEngine;
using System.Collections.Generic; // For _spawnedEnemies

public class NormalRoom : Room
{
    private List<GameObject> _spawnedEnemies = new();

    // Example: Enemy spawning logic might go here or be triggered by an event
    // public void SpawnEnemies(List<GameObject> enemyPrefabs) { ... }

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

    // private void LockDoors() { ... }
    // private void UnlockDoors() { ... }
    // private void CheckEnemiesCleared() { ... if all enemies defeated, call OnRoomClear(); ... }
}
