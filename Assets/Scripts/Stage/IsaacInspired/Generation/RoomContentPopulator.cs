using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the population of rooms with enemies and other content based on room templates.
/// </summary>
public class RoomContentPopulator : MonoBehaviour
{
    /// <summary>
    /// Populates the given room with enemies based on its template.
    /// </summary>
    /// <param name="roomInstance">The GameObject of the instantiated room. This object will be the parent for spawned content.</param>
    /// <param name="roomTemplate">The <see cref="RoomTemplate"/> defining what content to spawn and where.</param>
    /// <returns>A list of the GameObjects that were spawned.</returns>
    /// <remarks>
    /// Logs errors if <paramref name="roomInstance"/> or <paramref name="roomTemplate"/> is null.
    /// Skips spawn points if their <c>enemyPrefab</c> is null.
    /// </remarks>
    public List<GameObject> PopulateRoom(GameObject roomInstance, RoomTemplate roomTemplate)
    {
        if (roomTemplate == null)
        {
            Debug.LogError("RoomTemplate is null. Cannot populate room.");
            return new List<GameObject>();
        }

        if (roomInstance == null)
        {
            Debug.LogError("Room instance is null. Cannot populate room.");
            return new List<GameObject>();
        }

        List<GameObject> spawnedEnemies = new();

        Transform roomTransform = roomInstance.transform;

        foreach (EnemySpawnPoint spawnPoint in roomTemplate.enemySpawnPoints)
        {
            if (spawnPoint.enemyPrefab == null)
            {
                Debug.LogWarning($"EnemyPrefab is null for a spawn point in {roomTemplate.name}. Skipping this spawn point.");
                continue;
            }

            int actualCount = spawnPoint.count;

            for (int i = 0; i < actualCount; i++)
            {
                Vector2 spawnPosition = spawnPoint.localPosition;
                if (actualCount > 1 && spawnPoint.spawnRadius > 0)
                {
                    spawnPosition += Random.insideUnitCircle * spawnPoint.spawnRadius;
                }

                GameObject enemy = Instantiate(spawnPoint.enemyPrefab, roomTransform);
                enemy.transform.localPosition = spawnPosition;
                spawnedEnemies.Add(enemy);
            }
        }
        return spawnedEnemies;
    }
}
