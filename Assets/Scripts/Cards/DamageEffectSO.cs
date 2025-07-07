using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Card effect that deals damage to the target(s).
/// </summary>
[CreateAssetMenu(fileName = "DamageEffect", menuName = "Flare/Cards/Effects/DamageEffect")]
public class DamageEffectSO : CardEffectSO
{
    /// <summary>
    /// The amount of damage to deal to each target.
    /// </summary>
    public int DamageAmount;

    /// <summary>
    /// Applies damage to each target in the list using the Health component.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <param name="targets">The list of GameObjects to damage.</param>
    public override void ApplyEffect(GameObject user, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            var health = target.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(DamageAmount, user);
            }
        }
    }
}
