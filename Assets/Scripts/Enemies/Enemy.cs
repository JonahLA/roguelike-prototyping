using UnityEngine;

/// <summary>
/// The core component for an enemy GameObject. It holds references to the enemy's
/// stats and its health component, acting as a central point for enemy data.
/// </summary>
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [Tooltip("The ScriptableObject containing the stats for this enemy.")]
    [SerializeField]
    private EnemyStatsSO _stats;

    /// <summary>
    /// The stats configuration for this enemy.
    /// </summary>
    public EnemyStatsSO Stats => _stats;

    /// <summary>
    /// A reference to the Health component attached to this enemy.
    /// </summary>
    public Health Health { get; private set; }

    private void Awake()
    {
        Health = GetComponent<Health>();
    }

    private void Start()
    {
        if (_stats != null)
        {
            Health.SetMaxHealth(_stats.maxHealth);
        }
        else
        {
            Debug.LogError("Enemy stats are not assigned!", this);
        }
    }
}
