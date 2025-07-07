using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Card effect that applies a buff to the target(s).
/// </summary>
[CreateAssetMenu(fileName = "BuffEffect", menuName = "Flare/Cards/Effects/BuffEffect")]
public class BuffEffectSO : CardEffectSO
{
    /// <summary>
    /// The duration of the buff to apply, in seconds.
    /// </summary>
    public float BuffDuration;

    /// <summary>
    /// Applies a buff to each target in the list.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <param name="targets">The list of GameObjects to buff.</param>
    public override void ApplyEffect(GameObject user, List<GameObject> targets)
    {
        foreach (var target in targets)
        {
            // TODO: Implement buff logic (e.g., add a BuffComponent)
            // var buffable = target.GetComponent<BuffableComponent>();
            // if (buffable != null)
            // {
            //     buffable.ApplyBuff(BuffDuration);
            // }
        }
    }
}
