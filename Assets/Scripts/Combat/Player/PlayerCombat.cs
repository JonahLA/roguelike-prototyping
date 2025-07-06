using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player combat, coordinating attacks based on input and player state.
/// Requires a <see cref="PlayerMovementController"/> component on the same GameObject.
/// </summary>
[RequireComponent(typeof(PlayerMovementController))]
[AddComponentMenu("Gameplay/Player Combat")]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Configuration")]
    [Tooltip("The current attack strategy (ScriptableObject implementing IPlayerAttack) the player will use.")]
    [SerializeField]
    private PlayerAttackSO _currentAttackSO;

    [Tooltip("Transform defining the origin point of attacks. If null, this GameObject's position is used.")]
    [SerializeField]
    private Transform _attackOriginPoint;

    [Header("Input Actions")]
    [Tooltip("Reference to the Input Action used for triggering attacks.")]
    [SerializeField]
    private InputActionReference _attackActionReference;

    private PlayerMovementController _playerMovementController;

    /// <summary>
    /// Gets or sets the current attack strategy.
    /// </summary>
    public PlayerAttackSO CurrentAttackSO
    {
        get => _currentAttackSO;
        set => _currentAttackSO = value;
    }

    private void Awake()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();

        if (_attackActionReference == null)
        {
            Debug.LogError("Attack Action Reference is not set in PlayerCombat. Please assign it in the Inspector.", this);
            enabled = false; // Disable component if input is not set up
            return;
        }
        if (_attackActionReference.action == null)
        {
            Debug.LogError($"Attack Action Reference '{_attackActionReference.name}' does not have an action assigned. Please check the Input Actions asset.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (_attackActionReference != null && _attackActionReference.action != null)
        {
            _attackActionReference.action.performed += OnAttackPerformed;
            _attackActionReference.action.Enable();
        }
        // No warning here if null, Awake already handled it.
    }

    private void OnDisable()
    {
        if (_attackActionReference != null && _attackActionReference.action != null)
        {
            _attackActionReference.action.performed -= OnAttackPerformed;
            // It's generally good practice to disable actions if they are not shared or managed elsewhere.
            // However, if the action is part of a shared asset enabled/disabled globally, this might not be necessary.
            // For now, let's assume it might be shared and avoid disabling it here unless issues arise.
            // _attackActionReference.action.Disable(); 
        }
    }

    private void Update()
    {
        // Update cooldown for the current attack strategy, if one is assigned.
        if (_currentAttackSO != null) _currentAttackSO.UpdateCooldown(Time.deltaTime);
    }

    /// <summary>
    /// Called when the attack input action is performed.
    /// </summary>
    /// <param name="context">Callback context from the Input System.</param>
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (_currentAttackSO == null)
        {
            Debug.LogWarning("[PlayerCombat] CurrentAttackSO is not assigned. Cannot attack.", this);
            return;
        }

        if (_playerMovementController == null) { // Should have been caught in Awake, but good for safety.
            Debug.LogError("[PlayerCombat] PlayerMovementController not found. Cannot attack.", this);
            return;
        }

        if (!_playerMovementController.CanPerformActions)
        {
            Debug.Log("[PlayerCombat] Attack prevented by PlayerMovementController (CanPerformActions is false).");
            return;
        }

        if (!_currentAttackSO.CanAttack())
        {
            Debug.Log($"[PlayerCombat] Attack '{_currentAttackSO.name}' is on cooldown ({_currentAttackSO.CurrentCooldown}s remaining).");
            return;
        }

        Vector2 attackOrigin = _attackOriginPoint != null ? (Vector2)_attackOriginPoint.position : (Vector2)transform.position;
        Vector2 attackDirection = _playerMovementController.FacingDirection; 
        // Ensure direction is normalized if PlayerMovementController doesn't guarantee it.
        // For FacingDirection, it usually should be.

        // BaseDamage is now retrieved from the AttackContext within the SO, or directly from SO if needed.
        // float baseDamage = _currentAttackSO.BaseDamage; 

        AttackContext attackContext = new(this, attackOrigin, attackDirection, _currentAttackSO.BaseDamage);
        _currentAttackSO.Attack(attackContext);

        // Debug.Log($"[PlayerCombat] Attack performed with {_currentAttackSO.name} from {attackOrigin} in direction {attackDirection}.");
    }
}
