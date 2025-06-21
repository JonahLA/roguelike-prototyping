using UnityEngine;

/// <summary>
/// Base class for all targeting strategies. Targeting strategies determine where the enemy should move towards.
/// </summary>
public abstract class TargetingStrategySO : ScriptableObject
{
    /// <summary>
    /// Gets the target position for the enemy.
    /// </summary>
    /// <param name="enemy">The enemy AI component.</param>
    /// <param name="playerTransform">The transform of the player.</param>
    /// <returns>The position the enemy should target.</returns>
    public abstract Vector2 GetTarget(Enemy enemy, Transform playerTransform);
}
