/// <summary>
/// Defines the possible states for the enemy's AI.
/// </summary>
public enum EnemyAIState
{
    /// <summary>
    /// The enemy is idle, typically wandering around.
    /// </summary>
    Passive,

    /// <summary>
    /// The enemy has detected the player and is moving towards them.
    /// </summary>
    Pursuing,

    /// <summary>
    /// The enemy is in range and is actively attacking the player.
    /// </summary>
    Attacking
}
