using UnityEngine;

/// <summary>
/// Base class for all attack strategies. Attack strategies define how an enemy performs an attack.
/// </summary>
public abstract class AttackStrategySO : ScriptableObject
{
    /// <summary>
    /// Executes the attack logic.
    /// </summary>
    /// <param name="enemy">The enemy component performing the attack.</param>
    /// <param name="playerTransform">The transform of the player to be attacked.</param>
    public abstract void Attack(Enemy enemy, Transform playerTransform);
}
