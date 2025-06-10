using UnityEngine;

namespace Flare.PlayerCombat
{
    [CreateAssetMenu(fileName = "NewSwordSwingAttack", menuName = "Flare/Player Attacks/Sword Swing Attack")]
    public class SwordSwingAttackSO : PlayerAttackSO
    {
        [Header("Sword Swing Specifics")]
        [Tooltip("The range of the sword swing from the attack origin.")]
        [SerializeField, Min(0.1f)]
        private float _attackRange = 1.5f;

        [Tooltip("The width of the sword swing hitbox.")]
        [SerializeField, Min(0.1f)]
        private float _attackWidth = 1f;

        [Tooltip("The layer(s) that this attack can hit.")]
        [SerializeField]
        private LayerMask _hittableLayers;

        // It's good practice to define a constant for the angle if using OverlapBox
        private const float OVERLAP_BOX_ANGLE = 0f;

        protected override void ExecuteAttackLogic(AttackContext context)
        {
            // Calculate the center of the OverlapBox
            // The attack origin is typically the player's position or a child transform.
            // The direction determines where the box is placed relative to the origin.
            Vector2 boxCenter = context.AttackOrigin + context.AttackDirection * (_attackRange / 2f);
            Vector2 boxSize = new Vector2(_attackWidth, _attackRange); // Width along perpendicular, Range along direction
            
            // If attackDirection is (1,0), box is horizontal. If (0,1), box is vertical.
            // For a top-down game, if FacingDirection is (0,1) (up), we want a tall box.
            // If FacingDirection is (1,0) (right), we want a wide box.
            // The current boxSize assumes the "range" is along Y and "width" is along X of the box local space.
            // We need to orient the box correctly based on attackDirection.
            // A simpler way for a top-down 2D game is to have range always be the length of the box
            // and width be its thickness, and rotate the query if necessary, or adjust size based on direction.

            // Let's assume attackRange is the dimension *along* the attack direction,
            // and attackWidth is the dimension *perpendicular* to it.
            Vector2 effectiveBoxSize;
            float angle = Vector2.SignedAngle(Vector2.up, context.AttackDirection);

            // For OverlapBox, size is (width, height) before rotation.
            // We want the length of the box to be _attackRange and its thickness to be _attackWidth.
            effectiveBoxSize = new Vector2(_attackWidth, _attackRange);
            // The boxCenter should be offset along the attack direction by half the range.
            boxCenter = context.AttackOrigin + (context.AttackDirection.normalized * (_attackRange / 2f));

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, effectiveBoxSize, angle, _hittableLayers);

            Debug.Log($"SwordSwing: Attacking at {boxCenter} with size {effectiveBoxSize}, angle {angle}. Hits: {hits.Length}");

            foreach (Collider2D hit in hits)
            {
                // Prevent hitting self if player is on a hittable layer by mistake
                if (context.Instigator != null && hit.gameObject == context.Instigator.gameObject) continue;

                if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(_baseDamage); // BaseDamage from PlayerAttackSO
                    Debug.Log($"SwordSwing: Hit {hit.name} for {_baseDamage} damage.");
                }
            }

            // TODO: Instantiate visual effects for the swing
        }
    }
}
