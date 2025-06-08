using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Core health management component with event broadcasting and UI-friendly heart system.
/// Supports float-based health values for half-heart display (e.g., 2.5 health = 2.5 hearts).
/// </summary>
[AddComponentMenu("Combat/Health")]
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [Tooltip("Maximum health value. Each full point represents one heart in UI.")]
    [SerializeField, Range(0.5f, 10f)]
    private float _maxHealth = 5f;
    
    [Tooltip("Starting health value. Will be clamped to max health.")]
    [SerializeField, Range(0f, 10f)]
    private float _startingHealth = 5f;
    
    [Header("Damage Immunity")]
    [Tooltip("Duration of immunity frames after taking damage (prevents damage spam)")]
    [SerializeField, Range(0f, 2f)]
    private float _immunityDuration = 0.5f;
    
    [Header("Events")]
    [Tooltip("Triggered when health changes. Passes (current health, max health)")]
    public UnityEvent<float, float> OnHealthChanged;
    
    [Tooltip("Triggered when this entity takes damage. Passes (damage amount, source GameObject)")]
    public UnityEvent<float, GameObject> OnDamageTaken;
    
    [Tooltip("Triggered when this entity dies")]
    public UnityEvent OnDeath;
    
    // Private fields
    private float _currentHealth;
    private float _lastDamageTime;
    private bool _isDead;
    
    // C# Events for code-based subscriptions (more efficient than UnityEvents)
    public event Action<float, float> HealthChanged;
    public event Action<float, GameObject> DamageTaken;
    public event Action<Health> Death;
    
    // Properties implementing IDamageable
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsAlive => !_isDead && _currentHealth > 0f;
    
    // Additional properties for UI integration
    public float HealthPercentage => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
    public int FullHearts => Mathf.FloorToInt(_currentHealth);
    public bool HasHalfHeart => (_currentHealth % 1f) >= 0.5f;
    public int TotalHeartsToDisplay => Mathf.CeilToInt(_maxHealth);
    
    private void Awake()
    {
        // Initialize health to starting value, clamped to max
        _currentHealth = Mathf.Clamp(_startingHealth, 0f, _maxHealth);
        _isDead = false;
        _lastDamageTime = -_immunityDuration; // Allow immediate damage
    }
    
    private void Start()
    {
        // Broadcast initial health state
        BroadcastHealthChanged();
    }
    
    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    /// <param name="source">The GameObject that caused the damage</param>
    /// <returns>True if damage was applied, false if blocked by immunity or death</returns>
    public bool TakeDamage(float damage, GameObject source = null)
    {
        // Validate damage parameters
        if (damage <= 0f)
        {
            Debug.LogWarning($"Invalid damage amount: {damage}. Damage must be positive.", this);
            return false;
        }
        
        // Check if already dead
        if (_isDead || _currentHealth <= 0f)
        {
            return false;
        }
        
        // Check immunity frames
        if (Time.time - _lastDamageTime < _immunityDuration)
        {
            return false;
        }
        
        // Apply damage
        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        _lastDamageTime = Time.time;        // Broadcast damage taken event
        OnDamageTaken?.Invoke(damage, source);
        DamageTaken?.Invoke(damage, source);
        
        // Spawn damage number visual feedback
        DamageNumberSpawner.SpawnDamage(damage, transform.position);
        
        // Broadcast health changed event
        BroadcastHealthChanged();
        
        // Check for death
        if (_currentHealth <= 0f && !_isDead)
        {
            HandleDeath();
        }
        
        return true;
    }
    
    /// <summary>
    /// Heal this entity by the specified amount.
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    /// <returns>Actual amount healed</returns>
    public float Heal(float healAmount)
    {
        if (healAmount <= 0f || _isDead)
        {
            return 0f;
        }
        
        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);
        float actualHealed = _currentHealth - previousHealth;
          if (actualHealed > 0f)
        {
            // Spawn healing number visual feedback
            DamageNumberSpawner.SpawnDamage(actualHealed, transform.position, false, true);
            
            BroadcastHealthChanged();
        }
        
        return actualHealed;
    }
    
    /// <summary>
    /// Set health to full.
    /// </summary>
    public void FullHeal()
    {
        if (_isDead)
        {
            return;
        }
        
        if (_currentHealth < _maxHealth)
        {
            _currentHealth = _maxHealth;
            BroadcastHealthChanged();
        }
    }
    
    /// <summary>
    /// Set the maximum health and optionally adjust current health.
    /// </summary>
    /// <param name="newMaxHealth">New maximum health value</param>
    /// <param name="adjustCurrentHealth">If true, scales current health proportionally</param>
    public void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = false)
    {
        if (newMaxHealth <= 0f)
        {
            Debug.LogError("Max health must be positive", this);
            return;
        }
        
        float oldMaxHealth = _maxHealth;
        _maxHealth = newMaxHealth;
        
        if (adjustCurrentHealth && oldMaxHealth > 0f)
        {
            // Scale current health proportionally
            float healthRatio = _currentHealth / oldMaxHealth;
            _currentHealth = Mathf.Min(newMaxHealth, newMaxHealth * healthRatio);
        }
        else
        {
            // Just clamp current health to new max
            _currentHealth = Mathf.Min(_currentHealth, newMaxHealth);
        }
        
        BroadcastHealthChanged();
    }
    
    /// <summary>
    /// Force death without dealing damage.
    /// </summary>
    public void Kill()
    {
        if (_isDead)
        {
            return;
        }
        
        _currentHealth = 0f;
        HandleDeath();
    }
    
    /// <summary>
    /// Check if entity is currently immune to damage.
    /// </summary>
    public bool IsImmune => Time.time - _lastDamageTime < _immunityDuration;
    
    /// <summary>
    /// Get remaining immunity time in seconds.
    /// </summary>
    public float RemainingImmunityTime => Mathf.Max(0f, _immunityDuration - (Time.time - _lastDamageTime));
      private void HandleDeath()
    {
        _isDead = true;
        
        // Broadcast death events
        OnDeath?.Invoke();
        Death?.Invoke(this);
        
        // Notify HealthManager for centralized death events
        if (gameObject.CompareTag("Player"))
        {
            HealthManager.NotifyPlayerDeath();
        }
        else
        {
            HealthManager.NotifyEntityDeath(gameObject);
        }
        
        // Note: We don't destroy the GameObject here to allow death animations, etc.
        // The death event subscribers should handle cleanup if needed.
    }
    
    private void BroadcastHealthChanged()
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        // Notify HealthManager for centralized health events
        if (gameObject.CompareTag("Player"))
        {
            HealthManager.NotifyPlayerHealthChanged(_currentHealth, _maxHealth);
        }
        else
        {
            HealthManager.NotifyEntityHealthChanged(gameObject, _currentHealth, _maxHealth);
        }
    }
    
    /// <summary>
    /// Utility method to convert health value to heart display information.
    /// </summary>
    /// <param name="healthValue">Health value to convert</param>
    /// <returns>Tuple of (full hearts, has half heart)</returns>
    public static (int fullHearts, bool hasHalfHeart) HealthToHearts(float healthValue)
    {
        int fullHearts = Mathf.FloorToInt(healthValue);
        bool hasHalfHeart = (healthValue % 1f) >= 0.5f;
        return (fullHearts, hasHalfHeart);
    }
    
    /// <summary>
    /// Utility method to convert heart display back to health value.
    /// </summary>
    /// <param name="fullHearts">Number of full hearts</param>
    /// <param name="hasHalfHeart">Whether there's a half heart</param>
    /// <returns>Health value</returns>
    public static float HeartsToHealth(int fullHearts, bool hasHalfHeart)
    {
        return fullHearts + (hasHalfHeart ? 0.5f : 0f);
    }
    
    // Editor/Debug utilities
    #if UNITY_EDITOR
    [ContextMenu("Take 1 Damage")]
    private void DebugTakeDamage()
    {
        TakeDamage(1f);
    }
    
    [ContextMenu("Heal 1 Health")]
    private void DebugHeal()
    {
        Heal(1f);
    }
    
    [ContextMenu("Full Heal")]
    private void DebugFullHeal()
    {
        FullHeal();
    }
    
    [ContextMenu("Kill")]
    private void DebugKill()
    {
        Kill();
    }
    #endif
}
