using UnityEngine;
using UnityEngine.InputSystem;
// PlayerController2D is in the global namespace based on provided files

namespace Flare.PlayerCombat
{
    /// <summary>
    /// Handles player combat, coordinating attacks based on input and player state.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    [AddComponentMenu("Gameplay/Player Combat")]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Configuration")]
        [Tooltip("The current attack strategy (ScriptableObject) the player will use.")]
        [SerializeField]
        private PlayerAttackSO _currentAttackSO;

        [Tooltip("Transform defining the origin point of attacks. If null, player's position is used.")]
        [SerializeField]
        private Transform _attackOriginPoint;

        [Header("Input Actions")]
        [Tooltip("Reference to the Input Action for attacking.")]
        [SerializeField]
        private InputActionReference _attackActionReference;

        private PlayerController2D _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController2D>();

            if (_attackActionReference == null)
            {
                Debug.LogError("Attack Action Reference is not set in PlayerCombat. Please assign it in the Inspector.", this);
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
            else if (_attackActionReference != null)
            {
                Debug.LogWarning("Attack action was not available on Enable. Ensure the Input Action Reference is correctly set and the action exists.", this);
            }
        }

        private void OnDisable()
        {
            if (_attackActionReference != null && _attackActionReference.action != null)
            {
                _attackActionReference.action.performed -= OnAttackPerformed;
                // Consider disabling the action if it's not shared: _attackActionReference.action.Disable();
            }
        }

        private void Update()
        {
            _currentAttackSO?.UpdateCooldown(Time.deltaTime);
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (_currentAttackSO == null)
            {
                // Debug.LogWarning("PlayerCombat: CurrentAttackSO is not assigned.", this);
                return;
            }

            if (_playerController == null) {
                Debug.LogError("PlayerCombat: PlayerController2D not found.", this);
                return;
            }

            if (!_playerController.CanPerformActions)
            {
                // Debug.Log("PlayerCombat: Attack prevented by PlayerController (CanPerformActions is false).");
                return;
            }

            if (!_currentAttackSO.CanAttack())
            {
                // Debug.Log("PlayerCombat: Attack is on cooldown.");
                return;
            }

            Vector2 attackOrigin = _attackOriginPoint != null ? (Vector2)_attackOriginPoint.position : (Vector2)transform.position;
            Vector2 attackDirection = _playerController.FacingDirection;
            float baseDamage = _currentAttackSO.BaseDamage;

            AttackContext attackContext = new AttackContext(this, attackOrigin, attackDirection, baseDamage);
            _currentAttackSO.Attack(attackContext);

            // Debug.Log($"PlayerCombat: Attack performed with {_currentAttackSO.name}");
        }

        /// <summary>
        /// Allows changing the player's current attack strategy at runtime.
        /// </summary>
        /// <param name="newAttackSO">The new attack ScriptableObject.</param>
        public void SetAttackStrategy(PlayerAttackSO newAttackSO)
        {
            _currentAttackSO = newAttackSO;
            // Consider resetting cooldown of the new attack here if desired by game design.
            // e.g., if (_currentAttackSO != null) { _currentAttackSO.ResetCooldown(); }
            Debug.Log($"PlayerCombat: Attack strategy changed to {newAttackSO?.name ?? "None"}.");
        }
    }
}
