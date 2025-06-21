using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackStrategy", menuName = "Flare/Enemies/Attack/Melee")]
public class MeleeAttackStrategy : AttackStrategySO
{
    public override void Attack(Enemy enemy, Transform playerTransform)
    {
        IDamageable playerHealth = playerTransform.GetComponent<IDamageable>();
        playerHealth?.TakeDamage(enemy.Stats.attackDamage);
    }
}
