using UnityEngine;

namespace Flare.PlayerCombat
{
    /// <summary>
    /// Base ScriptableObject for player attack strategies.
    /// </summary>
    public abstract class PlayerAttackSO : ScriptableObject, IPlayerAttack
    {
        [Header("Base Attack Stats")]
        [Tooltip("Time in seconds before this attack can be used again.")]
        [SerializeField, Min(0f)]
        protected float _cooldownTime = 1f;

        [Tooltip("Base damage dealt by this attack.")]
        [SerializeField, Min(0f)]
        protected float _baseDamage = 10f;

        private float _currentCooldown;

        public float CooldownTime => _cooldownTime;
        public float BaseDamage => _baseDamage; // Added public getter
        public float CurrentCooldown => _currentCooldown;

        protected virtual void OnEnable()
        {
            // Ensure cooldown is reset when the game starts or the object is enabled
            // if it's part of a persistent player setup.
            // For ScriptableObjects, this might be more relevant if they were MonoBehaviours.
            // Consider resetting cooldown in PlayerCombat when an attack is equipped.
            _currentCooldown = 0f; 
        }

        public bool CanAttack()
        {
            return _currentCooldown <= 0f;
        }

        public void Attack(AttackContext context)
        {
            if (!CanAttack()) return;

            ExecuteAttackLogic(context);
            _currentCooldown = _cooldownTime;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (_currentCooldown > 0f)
            {
                _currentCooldown -= deltaTime;
            }
        }

        /// <summary>
        /// Contains the specific logic for how this attack is performed.
        /// </summary>
        /// <param name="context">The context of the attack.</param>
        protected abstract void ExecuteAttackLogic(AttackContext context);
    }
}
