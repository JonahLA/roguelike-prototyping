using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Card effect that applies a debuff to the target(s).
/// </summary>
[CreateAssetMenu(fileName = "DebuffEffect", menuName = "Flare/Cards/Effects/DebuffEffect")]
public class DebuffEffectSO : CardEffectSO
{
    /// <summary>
    /// The duration of the debuff to apply, in seconds.
    /// </summary>
    public float DebuffDuration;

    /// <summary>
    /// Applies a debuff to each target in the list.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <param name="targets">The list of GameObjects to debuff.</param>
    public override void ApplyEffect(GameObject user, List<GameObject> targets)
    {
        // foreach (var target in targets)
        // {
        //     // TODO: Implement debuff logic (e.g., add a DebuffComponent)
        //     var debuffable = target.GetComponent<DebuffableComponent>();
        //     if (debuffable != null)
        //     {
        //         debuffable.ApplyDebuff(DebuffDuration);
        //     }
        // }
    }
}
