using UnityEngine;

/// <summary>
/// An attack strategy where the enemy performs a simple melee attack on the player.
/// </summary>
[CreateAssetMenu(fileName = "MeleeAttackStrategy", menuName = "Flare/Enemies/Attack/Melee")]
public class MeleeAttackStrategy : AttackStrategySO
{
    /// <summary>
    /// Finds the IDamageable component on the player and deals damage.
    /// </summary>
    /// <param name="enemy">The enemy performing the attack.</param>
    /// <param name="playerTransform">The transform of the player to be attacked.</param>
    public override void Attack(Enemy enemy, Transform playerTransform)
    {
        if (playerTransform == null) return;

        IDamageable playerHealth = playerTransform.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            // Pass the enemy's GameObject as the source of the damage
            playerHealth.TakeDamage(enemy.Stats.attackDamage, enemy.gameObject);
        }
    }
}
