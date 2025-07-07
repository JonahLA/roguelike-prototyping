using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Targeting strategy that selects the user as the only target (self-targeting).
/// </summary>
[CreateAssetMenu(fileName = "SelfTargeting", menuName = "Flare/Cards/Targeting/Self")]
public class SelfTargetingSO : CardTargetingStrategySO
{
    /// <summary>
    /// Returns a list containing only the user GameObject.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <returns>A list with the user GameObject as the only element.</returns>
    public override List<GameObject> GetTargets(GameObject user)
    {
        return new List<GameObject> { user };
    }
}
