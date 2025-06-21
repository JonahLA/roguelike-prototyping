using UnityEngine;

/// <summary>
/// A targeting strategy where the enemy directly pursues the player.
/// </summary>
[CreateAssetMenu(fileName = "PursuePlayerTargetingStrategy", menuName = "Flare/Enemies/Targeting/Pursue Player")]
public class PursuePlayerTargetingStrategy : TargetingStrategySO
{
    /// <summary>
    /// Gets the player's current position as the target.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="playerTransform">The transform of the player.</param>
    /// <returns>The player's position, or the enemy's own position if the player is not found.</returns>
    public override Vector2 GetTarget(Enemy enemy, Transform playerTransform)
    {
        if (playerTransform != null)
            return playerTransform.position;

        // Fallback if the player transform is somehow null
        return enemy.transform.position;
    }
}
