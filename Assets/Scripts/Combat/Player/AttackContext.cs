using UnityEngine;

namespace Flare.PlayerCombat
{
    /// <summary>
    /// Holds contextual information for executing a player attack.
    /// </summary>
    public struct AttackContext
    {
        public PlayerCombat Instigator { get; }
        public Vector2 AttackOrigin { get; }
        public Vector2 AttackDirection { get; }
        public float BaseDamage { get; }

        public AttackContext(PlayerCombat instigator, Vector2 attackOrigin, Vector2 attackDirection, float baseDamage)
        {
            Instigator = instigator;
            AttackOrigin = attackOrigin;
            AttackDirection = attackDirection;
            BaseDamage = baseDamage;
        }
    }
}
