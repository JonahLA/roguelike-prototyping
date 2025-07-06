using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a standard combat or puzzle room in the game. Inherits from <see cref="Room"/>.
/// </summary>
/// <remarks>
/// Normal rooms contain enemies that must be cleared to progress.
/// They lock their doors upon player entry and unlock them once all threats are neutralized.
/// </remarks>
public class NormalRoom : Room
{
    [Tooltip("List of enemies currently active in this room.")]
    private readonly List<GameObject> _spawnedEnemies = new();

    [Tooltip("Reference to the content populator utility.")]
    [SerializeField] private RoomContentPopulator _roomContentPopulator;

    protected override void Awake()
    {
        base.Awake();

        // Fallback to find the populator if not assigned in the inspector
        if (_roomContentPopulator == null)
        {
            _roomContentPopulator = FindFirstObjectByType<RoomContentPopulator>();
            if (_roomContentPopulator == null)
            {
                Debug.LogError("RoomContentPopulator not found in the scene.", this);
            }
        }
    }

    /// <summary>
    /// Called when the player enters this room.
    /// Spawns enemies and locks doors on first entry if the room is not already cleared.
    /// </summary>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();

        if (isCleared) return;

        if (template != null && template.enemySpawnPoints.Any())
        {
            SpawnEnemiesAndSecureRoom();
        }
        else
        {
            // If there are no enemies to spawn, the room is considered clear.
            OnRoomClear();
        }
    }

    private void SpawnEnemiesAndSecureRoom()
    {
        var spawned = _roomContentPopulator.PopulateRoom(gameObject, template);
        _spawnedEnemies.AddRange(spawned);

        if (_spawnedEnemies.Count > 0)
        {
            Debug.Log($"NormalRoom {gameObject.name}: Player entered. Spawning {_spawnedEnemies.Count} enemies and locking doors.");
            HealthManager.EntityDeath += HandleEnemyDeath;
            CloseDoors();
        }
        else
        {
            // No enemies were actually spawned, so clear the room immediately.
            OnRoomClear();
        }
    }

    private void HandleEnemyDeath(GameObject deadEnemy)
    {
        if (_spawnedEnemies.Contains(deadEnemy))
        {
            _spawnedEnemies.Remove(deadEnemy);
            CheckEnemiesCleared();
        }
    }

    private void CheckEnemiesCleared()
    {
        if (_spawnedEnemies.Count == 0 && !isCleared)
        {
            OnRoomClear();
        }
    }

    /// <summary>
    /// Called when the room is cleared of all enemies.
    /// Unlocks doors and marks the room as cleared.
    /// </summary>
    public override void OnRoomClear()
    {
        if (isCleared) return;
        base.OnRoomClear();
        HealthManager.EntityDeath -= HandleEnemyDeath;
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe from the event when the room is destroyed
        // to prevent memory leaks.
        if (_spawnedEnemies.Count > 0)
        {
            HealthManager.EntityDeath -= HandleEnemyDeath;
        }
    }
}
