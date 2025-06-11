using UnityEngine;

/// <summary>
/// Base ScriptableObject for player attack strategies.
/// Implements <see cref="IPlayerAttack"/> to provide common cooldown and damage functionality.
/// </summary>
public abstract class PlayerAttackSO : ScriptableObject, IPlayerAttack
{
    [Header("Base Attack Stats")]
    [Tooltip("Time in seconds before this attack can be used again.")]
    [SerializeField, Min(0f)]
    protected float _cooldownTime = 1f;

    [Tooltip("Base damage dealt by this attack before any modifiers.")]
    [SerializeField, Min(0f)]
    protected float _baseDamage = 10f;

    private float _currentCooldown;

    /// <summary>
    /// Gets the total time in seconds this attack takes to cool down.
    /// </summary>
    public float CooldownTime => _cooldownTime;

    /// <summary>
    /// Gets the base damage dealt by this attack.
    /// </summary>
    public float BaseDamage => _baseDamage;

    /// <summary>
    /// Gets the current remaining cooldown time in seconds.
    /// </summary>
    public float CurrentCooldown => _currentCooldown;

    /// <summary>
    /// Called when the ScriptableObject is loaded or a value is changed in the Inspector.
    /// Resets the current cooldown to 0.
    /// </summary>
    protected virtual void OnEnable()
    {
        // Reset cooldown when the game starts or the object is enabled/recompiled.
        _currentCooldown = 0f; 
    }

    /// <summary>
    /// Checks if the attack can currently be performed (i.e., not on cooldown).
    /// </summary>
    /// <returns>True if <see cref="CurrentCooldown"/> is less than or equal to 0, false otherwise.</returns>
    public bool CanAttack()
    {
        return _currentCooldown <= 0f;
    }

    /// <summary>
    /// Executes the attack if <see cref="CanAttack"/> is true.
    /// Calls the specific <see cref="ExecuteAttackLogic"/> and then sets the attack on cooldown.
    /// </summary>
    /// <param name="context">The context of the attack.</param>
    public void Attack(AttackContext context)
    {
        if (!CanAttack()) return;

        ExecuteAttackLogic(context);
        _currentCooldown = _cooldownTime;
    }

    /// <summary>
    /// Updates the attack's cooldown timer by subtracting <paramref name="deltaTime"/>.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame.</param>
    public void UpdateCooldown(float deltaTime)
    {
        if (_currentCooldown > 0f)
        {
            _currentCooldown -= deltaTime;
            if (_currentCooldown < 0f)
            {
                _currentCooldown = 0f; // Ensure cooldown doesn't go negative
            }
        }
    }

    /// <summary>
    /// Abstract method to be implemented by derived classes.
    /// Contains the specific logic for how this attack is performed (e.g., raycasting, spawning projectiles).
    /// </summary>
    /// <param name="context">The context of the attack, providing information like origin, direction, and instigator.</param>
    protected abstract void ExecuteAttackLogic(AttackContext context);
}
