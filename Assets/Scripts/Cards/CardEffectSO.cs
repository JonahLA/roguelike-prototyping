using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all card effects. Card effects define what happens when a card is played.
/// </summary>
public abstract class CardEffectSO : ScriptableObject
{
    /// <summary>
    /// Applies the effect to the specified targets.
    /// </summary>
    /// <param name="user">The GameObject using the card (typically the player).</param>
    /// <param name="targets">The list of GameObjects affected by the effect.</param>
    public abstract void ApplyEffect(GameObject user, List<GameObject> targets);
}
