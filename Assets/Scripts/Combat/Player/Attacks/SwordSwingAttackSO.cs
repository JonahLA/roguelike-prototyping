using UnityEngine;

/// <summary>
/// A <see cref="PlayerAttackSO"/> that executes a melee sword swing using Physics2D.OverlapBoxAll.
/// </summary>
[CreateAssetMenu(fileName = "NewSwordSwingAttack", menuName = "Flare/Player Attacks/Sword Swing Attack")]
public class SwordSwingAttackSO : PlayerAttackSO
{
    [Header("Sword Swing Specifics")]
    [Tooltip("The effective range of the sword swing, measured from the attack origin along the attack direction.")]
    [SerializeField, Min(0.1f)]
    private float _attackRange = 1.5f;

    [Tooltip("The width of the sword swing hitbox, perpendicular to the attack direction.")]
    [SerializeField, Min(0.1f)]
    private float _attackWidth = 1f;

    [Tooltip("The layer(s) that this attack can hit. Entities on these layers must have an IDamageable component to take damage.")]
    [SerializeField]
    private LayerMask _hittableLayers;
    
    // private const float OVERLAP_BOX_ANGLE = 0f; // Not used as angle is dynamic

    /// <summary>
    /// Executes the sword swing logic.
    /// Creates a physics overlap box in the direction of the attack to detect and damage targets.
    /// </summary>
    /// <param name="context">The context of the attack, providing origin, direction, and instigator information.</param>
    protected override void ExecuteAttackLogic(AttackContext context)
    {
        // The boxCenter should be offset along the attack direction by half the range.
        Vector2 boxCenter = context.AttackOrigin + (context.AttackDirection.normalized * (_attackRange / 2f));
        
        // For OverlapBox, size is (width, height) before rotation.
        // We want the length of the box to be _attackRange (along attack direction) 
        // and its thickness to be _attackWidth (perpendicular to attack direction).
        Vector2 effectiveBoxSize = new(_attackWidth, _attackRange);
        
        // Calculate the angle of the attack direction relative to the world's up vector.
        // This is used to rotate the OverlapBox to align with the attack.
        float angle = Vector2.SignedAngle(Vector2.up, context.AttackDirection.normalized);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, effectiveBoxSize, angle, _hittableLayers);

        Debug.Log($"SwordSwing: Attacking at {boxCenter} with size {effectiveBoxSize}, angle {angle}. Hits: {hits.Length}. Direction: {context.AttackDirection}");

        foreach (Collider2D hit in hits)
        {
            // Prevent hitting self if player is on a hittable layer by mistake
            if (context.Instigator != null && hit.gameObject == context.Instigator.gameObject) continue;

            if (hit.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(context.BaseDamage); // Use BaseDamage from context
                Debug.Log($"SwordSwing: Hit {hit.name} for {context.BaseDamage} damage.");
            }
        }

        // TODO: Instantiate visual effects for the swing
    }
}
