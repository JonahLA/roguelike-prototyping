using UnityEngine;
using System;

/// <summary>
/// Central manager for health-related events across the game.
/// Allows UI and other systems to respond to health changes without direct references.
/// Supports multiple entity types (Player, Enemies, Bosses) for future extensibility.
/// </summary>
public static class HealthManager
{
    // Player-specific events
    public static event Action<float, float> PlayerHealthChanged;
    public static event Action<float, GameObject> PlayerDamageTaken;
    public static event Action PlayerDeath;
    
    // General entity events (for future use with enemies, bosses, etc.)
    public static event Action<GameObject, float, float> EntityHealthChanged;
    public static event Action<GameObject, float, GameObject> EntityDamageTaken;
    public static event Action<GameObject> EntityDeath;
    
    /// <summary>
    /// Notify that player health has changed.
    /// Called by the Player's Health component.
    /// </summary>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    public static void NotifyPlayerHealthChanged(float currentHealth, float maxHealth)
    {
        PlayerHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Also trigger general entity event for systems that care about all entities
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            EntityHealthChanged?.Invoke(player, currentHealth, maxHealth);
        }
    }
    
    /// <summary>
    /// Notify that player took damage.
    /// Called by the Player's Health component.
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    /// <param name="source">Source of the damage</param>
    public static void NotifyPlayerDamageTaken(float damage, GameObject source)
    {
        PlayerDamageTaken?.Invoke(damage, source);
        
        // Also trigger general entity event
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            EntityDamageTaken?.Invoke(player, damage, source);
        }
    }
    
    /// <summary>
    /// Notify that player has died.
    /// Called by the Player's Health component.
    /// </summary>
    public static void NotifyPlayerDeath()
    {
        PlayerDeath?.Invoke();
        
        // Also trigger general entity event
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            EntityDeath?.Invoke(player);
        }
    }
    
    /// <summary>
    /// Notify that any entity's health has changed.
    /// Used for enemies, bosses, and other entities in the future.
    /// </summary>
    /// <param name="entity">The entity whose health changed</param>
    /// <param name="currentHealth">Current health value</param>
    /// <param name="maxHealth">Maximum health value</param>
    public static void NotifyEntityHealthChanged(GameObject entity, float currentHealth, float maxHealth)
    {
        EntityHealthChanged?.Invoke(entity, currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Notify that any entity took damage.
    /// Used for enemies, bosses, and other entities in the future.
    /// </summary>
    /// <param name="entity">The entity that took damage</param>
    /// <param name="damage">Amount of damage taken</param>
    /// <param name="source">Source of the damage</param>
    public static void NotifyEntityDamageTaken(GameObject entity, float damage, GameObject source)
    {
        EntityDamageTaken?.Invoke(entity, damage, source);
    }
    
    /// <summary>
    /// Notify that any entity has died.
    /// Used for enemies, bosses, and other entities in the future.
    /// </summary>
    /// <param name="entity">The entity that died</param>
    public static void NotifyEntityDeath(GameObject entity)
    {
        EntityDeath?.Invoke(entity);
    }
    
    // Debug/utility methods
    #if UNITY_EDITOR
    /// <summary>
    /// Get the number of subscribers to player health events (for debugging).
    /// </summary>
    public static int GetPlayerHealthSubscriberCount()
    {
        return PlayerHealthChanged?.GetInvocationList().Length ?? 0;
    }
    
    /// <summary>
    /// Clear all event subscriptions (useful for scene transitions in editor).
    /// </summary>
    public static void ClearAllSubscriptions()
    {
        PlayerHealthChanged = null;
        PlayerDamageTaken = null;
        PlayerDeath = null;
        EntityHealthChanged = null;
        EntityDamageTaken = null;
        EntityDeath = null;
        
        Debug.Log("HealthManager: All event subscriptions cleared.");
    }
    #endif
}
