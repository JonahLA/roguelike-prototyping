using UnityEngine;

/// <summary>
/// Interface for entities that can receive damage.
/// Provides a consistent way to apply damage across different entity types.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="source">The GameObject that caused the damage (optional)</param>
    /// <returns>True if damage was successfully applied, false if it was blocked (immunity, etc.)</returns>
    bool TakeDamage(float damage, GameObject source = null);
    
    /// <summary>
    /// Current health value.
    /// </summary>
    float CurrentHealth { get; }
    
    /// <summary>
    /// Maximum health value.
    /// </summary>
    float MaxHealth { get; }
    
    /// <summary>
    /// Whether this entity is currently alive.
    /// </summary>
    bool IsAlive { get; }
}
