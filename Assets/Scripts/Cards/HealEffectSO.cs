using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Card effect that heals the target(s).
/// </summary>
[CreateAssetMenu(fileName = "HealEffect", menuName = "Flare/Cards/Effects/HealEffect")]
public class HealEffectSO : CardEffectSO
{
    /// <summary>
    /// The amount of health to restore to each target.
    /// </summary>
    public int HealAmount;

    /// <summary>
    /// Applies healing to each target in the list using the Health component.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <param name="targets">The list of GameObjects to heal.</param>
    public override void ApplyEffect(GameObject user, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            var health = target.GetComponent<Health>();
            if (health != null)
            {
                health.Heal(HealAmount);
            }
        }
    }
}
