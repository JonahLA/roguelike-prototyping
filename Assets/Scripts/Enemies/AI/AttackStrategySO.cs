using UnityEngine;

public abstract class AttackStrategySO : ScriptableObject
{
    public abstract void Attack(Enemy enemy, Transform playerTransform);
}
