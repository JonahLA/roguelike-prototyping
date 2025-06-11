using UnityEngine;

    /// <summary>
    /// Interface for all player attack strategies.
    /// </summary>
    public interface IPlayerAttack
    {
        /// <summary>
        /// Gets the total time in seconds this attack takes to cool down.
        /// </summary>
        float CooldownTime { get; }

        /// <summary>
        /// Gets the current remaining cooldown time in seconds.
        /// </summary>
        float CurrentCooldown { get; }

        /// <summary>
        /// Checks if the attack can currently be performed (e.g., not on cooldown).
        /// </summary>
        /// <returns>True if the attack can be performed, false otherwise.</returns>
        bool CanAttack();

        /// <summary>
        /// Executes the attack logic.
        /// </summary>
        /// <param name="context">The context of the attack, providing necessary information like origin and direction.</param>
        void Attack(AttackContext context);

        /// <summary>
        /// Updates the attack's cooldown timer.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        void UpdateCooldown(float deltaTime);
    }
