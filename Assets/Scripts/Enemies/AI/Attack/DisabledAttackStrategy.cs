using UnityEngine;

/// <summary>
/// An attack strategy where the enemy doesn't perform any attacks.
/// </summary>
[CreateAssetMenu(fileName = "DisabledAttackStrategy", menuName = "Flare/Enemies/Attack/Disabled")]
public class DisabledAttackStrategy : AttackStrategySO
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="enemy">The enemy performing the attack.</param>
    /// <param name="playerTransform">The transform of the player to be attacked.</param>
    public override void Attack(Enemy enemy, Transform playerTransform)
    {
        return;
    }
}
