using UnityEngine;

/// <summary>
/// A targeting strategy where the enemy wanders to random points within a specified radius.
/// </summary>
[CreateAssetMenu(fileName = "WanderTargetingStrategy", menuName = "Flare/Enemies/Targeting/Wander")]
public class WanderTargetingStrategy : TargetingStrategySO
{
    /// <summary>
    /// Gets a random target position within the enemy's wander radius.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="playerTransform">The transform of the player (not used in this strategy).</param>
    /// <returns>A random position for the enemy to wander towards.</returns>
    public override Vector2 GetTarget(Enemy enemy, Transform playerTransform)
    {
        Vector2 randomPoint = (Random.insideUnitCircle * enemy.Stats.wanderRadius) + (Vector2)enemy.transform.position;
        return randomPoint;
    }
}
