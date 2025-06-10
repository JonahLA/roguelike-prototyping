using UnityEngine;

namespace Flare.PlayerCombat
{
    /// <summary>
    /// Interface for all player attack strategies.
    /// </summary>
    public interface IPlayerAttack
    {
        float CooldownTime { get; }
        float CurrentCooldown { get; }
        bool CanAttack();
        void Attack(AttackContext context);
        void UpdateCooldown(float deltaTime);
    }
}
