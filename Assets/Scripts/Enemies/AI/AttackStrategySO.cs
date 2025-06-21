using UnityEngine;

public abstract class AttackStrategySO : ScriptableObject
{
    public abstract void Attack(Transform enemyTransform, Transform playerTransform, float attackDamage);
}
