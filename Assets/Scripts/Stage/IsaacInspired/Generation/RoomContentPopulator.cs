using UnityEngine;
using System.Collections.Generic; // Required for List

/// <summary>
/// Handles the population of rooms with enemies and other content based on room templates and game difficulty.
/// </summary>
public class RoomContentPopulator : MonoBehaviour
{
    /// <summary>
    /// Defines the maximum scaling factor for enemy counts at the highest difficulty (1.0f).
    /// For example, a value of 1.0f means the count can double, while 0.5f means it can increase by 50%.
    /// </summary>
    [Tooltip("Maximum scaling factor for enemy count at full difficulty. E.g., 1.0 means count can double.")]
    [SerializeField] private float maxCountScaleFactor = 1.0f;

    /// <summary>
    /// Populates the given room with enemies based on its template and the current difficulty.
    /// </summary>
    /// <param name="roomInstance">The GameObject of the instantiated room. This object will be the parent for spawned content.</param>
    /// <param name="roomTemplate">The <see cref="RoomTemplate"/> defining what content to spawn and where.</param>
    /// <param name="difficulty">The current difficulty level, typically ranging from 0.0 (easiest) to 1.0 (hardest). This influences enemy count and potentially other factors.</param>
    /// <remarks>
    /// Logs errors if <paramref name="roomInstance"/> or <paramref name="roomTemplate"/> is null.
    /// Skips spawn points if their <c>enemyPrefab</c> is null.
    /// Future enhancements could include tiered enemy selection and stat scaling based on difficulty.
    /// </remarks>
    public void PopulateRoom(GameObject roomInstance, RoomTemplate roomTemplate, float difficulty)
    {
        if (roomTemplate == null)
        {
            Debug.LogError("RoomTemplate is null. Cannot populate room.");
            return;
        }

        if (roomInstance == null)
        {
            Debug.LogError("Room instance is null. Cannot populate room.");
            return;
        }

        // Optional: Log the difficulty for this room to help with balancing.
        // Debug.Log($"Populating room {roomTemplate.name} (Instance: {roomInstance.name}) with difficulty: {difficulty}");

        Transform roomTransform = roomInstance.transform;

        foreach (EnemySpawnPoint spawnPoint in roomTemplate.enemySpawnPoints)
        {
            if (spawnPoint.enemyPrefab == null)
            {
                Debug.LogWarning($"EnemyPrefab is null for a spawn point in {roomTemplate.name}. Skipping this spawn point.");
                continue;
            }

            // --- Difficulty Influence on Count ---
            // Ensure difficulty is clamped between 0 and 1 to prevent unintended scaling issues.
            float clampedDifficulty = Mathf.Clamp01(difficulty);
            float scaledCount = Mathf.Lerp(spawnPoint.count, spawnPoint.count * (1f + maxCountScaleFactor), clampedDifficulty);
            
            int actualCount = 0;
            if (spawnPoint.count > 0)
            {
                actualCount = Mathf.Max(1, Mathf.CeilToInt(scaledCount));
            }
            
            // --- Placeholder for Tiered Enemy Selection (Future) ---
            // GameObject enemyPrefabToSpawn = spawnPoint.enemyPrefab; // Default
            // if (spawnPoint.difficultyTiers != null && spawnPoint.difficultyTiers.Count > 0) {
            //     // Logic to select prefab based on difficulty and defined tiers
            // }

            // Debug.Log($"SpawnPoint: {spawnPoint.enemyPrefab.name}, BaseCount: {spawnPoint.count}, Difficulty: {difficulty}, ScaledCountFloat: {scaledCount}, ActualCount: {actualCount}");

            for (int i = 0; i < actualCount; i++)
            {
                Vector2 spawnPosition = spawnPoint.localPosition;
                if (actualCount > 1 && spawnPoint.spawnRadius > 0)
                {
                    spawnPosition += Random.insideUnitCircle * spawnPoint.spawnRadius;
                }

                GameObject enemy = Instantiate(spawnPoint.enemyPrefab, roomTransform); 
                enemy.transform.localPosition = spawnPosition;
                
                // --- Placeholder for Modifying Enemy Stats (Future) ---
                // EnemyStats stats = enemy.GetComponent<EnemyStats>();
                // if (stats != null) {
                //     stats.ScaleHealth(1f + (difficulty * 0.5f)); 
                //     stats.ScaleDamage(1f + (difficulty * 0.25f)); 
                // }
            }
        }
    }
}
