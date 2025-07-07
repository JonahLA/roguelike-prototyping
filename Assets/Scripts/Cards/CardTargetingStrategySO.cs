using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all card targeting strategies. Targeting strategies determine which GameObjects are affected by a card.
/// </summary>
public abstract class CardTargetingStrategySO : ScriptableObject
{
    /// <summary>
    /// Returns a list of targets for the card effect, based on the user and game state.
    /// </summary>
    /// <param name="user">The GameObject using the card (typically the player).</param>
    /// <returns>A list of GameObjects to be targeted by the card effect.</returns>
    public abstract List<GameObject> GetTargets(GameObject user);
}
