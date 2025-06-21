using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Flare/Enemies/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float wanderRadius = 5f;

    [Header("Combat")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float attackDamage = 1f;

    [Header("Health")]
    public float maxHealth = 3f;
}
