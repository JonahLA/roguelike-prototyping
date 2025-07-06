using UnityEngine;

/// <summary>
/// A targeting strategy where the enemy doesn't do anything.
/// </summary>
[CreateAssetMenu(fileName = "DisabledPlayerTargetingStrategy", menuName = "Flare/Enemies/Targeting/Disabled")]
public class DisabledPlayerTargetingStrategy : TargetingStrategySO
{
    /// <summary>
    /// Doesn't do anything.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="playerTransform">The transform of the player.</param>
    /// <returns>The player's position, or the enemy's own position if the player is not found.</returns>
    public override Vector2 GetTarget(Enemy enemy, Transform playerTransform)
    {
        return Vector2.zero;
    }
}
