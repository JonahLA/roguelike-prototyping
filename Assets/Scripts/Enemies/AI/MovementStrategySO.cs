using UnityEngine;

/// <summary>
/// Base class for all movement strategies. Movement strategies define how an enemy moves.
/// </summary>
public abstract class MovementStrategySO : ScriptableObject
{
    /// <summary>
    /// Executes the movement logic for the enemy.
    /// </summary>
    /// <param name="enemy">The enemy component.</param>
    /// <param name="rb">The Rigidbody2D component of the enemy.</param>
    /// <param name="target">The position the enemy is moving towards.</param>
    public abstract void Move(Enemy enemy, Rigidbody2D rb, Vector2 target);
}
