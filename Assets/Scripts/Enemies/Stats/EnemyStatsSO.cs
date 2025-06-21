using UnityEngine;

/// <summary>
/// A ScriptableObject that holds the core statistics for an enemy type.
/// This allows for easy creation and configuration of different enemies.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Flare/Enemies/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("The speed at which the enemy moves.")]
    public float moveSpeed = 3.5f;

    [Tooltip("The radius within which the enemy will wander when in a passive state.")]
    public float wanderRadius = 5f;

    [Header("Combat")]
    [Tooltip("The range at which the enemy will detect the player and start pursuing them.")]
    public float detectionRange = 10f;

    [Tooltip("The range at which the enemy can perform its attack.")]
    public float attackRange = 1.5f;

    [Tooltip("The time in seconds between enemy attacks.")]
    public float attackCooldown = 2f;

    [Tooltip("The amount of damage the enemy's attack deals.")]
    public float attackDamage = 1f;

    [Header("Health")]
    [Tooltip("The maximum health of the enemy.")]
    public float maxHealth = 3f;
}
