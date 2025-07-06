using UnityEngine;

/// <summary>
/// A movement strategy where the enemy doesn't move.
/// </summary>
[CreateAssetMenu(fileName = "DisabledMovementStrategy", menuName = "Flare/Enemies/Movement/Disabled")]
public class DisabledMovementStrategy : MovementStrategySO
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="rb">The Rigidbody2D component of the enemy.</param>
    /// <param name="target">The position to move towards.</param>
    public override void Move(Enemy enemy, Rigidbody2D rb, Vector2 target)
    {
        return;
    }
}
