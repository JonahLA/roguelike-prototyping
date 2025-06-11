using UnityEngine;

/// <summary>
/// Holds contextual information for executing a player attack.
/// </summary>
public struct AttackContext
{
    /// <summary>
    /// The PlayerCombat component that initiated the attack.
    /// </summary>
    public PlayerCombat Instigator { get; }

    /// <summary>
    /// The world-space position where the attack originates.
    /// </summary>
    public Vector2 AttackOrigin { get; }

    /// <summary>
    /// The normalized direction of the attack.
    /// </summary>
    public Vector2 AttackDirection { get; }

    /// <summary>
    /// The base damage value of the attack before any modifications.
    /// </summary>
    public float BaseDamage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackContext"/> struct.
    /// </summary>
    /// <param name="instigator">The PlayerCombat component that initiated the attack.</param>
    /// <param name="attackOrigin">The world-space position where the attack originates.</param>
    /// <param name="attackDirection">The normalized direction of the attack.</param>
    /// <param name="baseDamage">The base damage value of the attack.</param>
    public AttackContext(PlayerCombat instigator, Vector2 attackOrigin, Vector2 attackDirection, float baseDamage)
    {
        Instigator = instigator;
        AttackOrigin = attackOrigin;
        AttackDirection = attackDirection;
        BaseDamage = baseDamage;
    }
}
