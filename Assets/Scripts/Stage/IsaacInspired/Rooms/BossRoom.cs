using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a boss room in the game. Inherits from the base <see cref="Room"/> class.
/// </summary>
/// <remarks>
/// This room type typically initiates a boss encounter when the player enters
/// and may have specific logic for locking doors until the boss is defeated.
/// </remarks>
public class BossRoom : Room
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
                Debug.LogError($"[BossRoom {gameObject.name}] RoomContentPopulator not found in the scene.", this);
            }
        }
    }

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

        if (isCleared) return;
        CloseDoors();

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
            Debug.Log($"[BossRoom {gameObject.name}] Player entered. Spawning {_spawnedEnemies.Count} enemies and locking doors.");
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
