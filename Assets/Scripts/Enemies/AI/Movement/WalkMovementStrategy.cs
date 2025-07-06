using UnityEngine;

/// <summary>
/// A movement strategy where the enemy walks towards a target using Rigidbody2D.MovePosition.
/// </summary>
[CreateAssetMenu(fileName = "WalkMovementStrategy", menuName = "Flare/Enemies/Movement/Walk")]
public class WalkMovementStrategy : MovementStrategySO
{
    /// <summary>
    /// Moves the enemy towards the target position at the speed defined in its stats.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="rb">The Rigidbody2D component of the enemy.</param>
    /// <param name="target">The position to move towards.</param>
    public override void Move(Enemy enemy, Rigidbody2D rb, Vector2 target)
    {
        Vector2 direction = (target - (Vector2)enemy.transform.position).normalized;
        rb.MovePosition(rb.position + enemy.Stats.moveSpeed * Time.fixedDeltaTime * direction);
    }
}
