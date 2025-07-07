using UnityEngine;

/// <summary>
/// ScriptableObject representing a card, including its cost, type, description, effect, and targeting strategy.
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "Flare/Cards/Card")]
public class CardSO : ScriptableObject
{
    /// <summary>
    /// The display name of the card.
    /// </summary>
    public string CardName;
    /// <summary>
    /// The flare cost required to play this card.
    /// </summary>
    public int FlareCost;
    /// <summary>
    /// The type/category of this card.
    /// </summary>
    public CardType CardType;
    /// <summary>
    /// The description of the card's effect, shown in the UI.
    /// </summary>
    [TextArea]
    public string Description;
    /// <summary>
    /// The effect that will be applied when this card is played.
    /// </summary>
    public CardEffectSO Effect;
    /// <summary>
    /// The targeting strategy used to select targets for this card's effect.
    /// </summary>
    public CardTargetingStrategySO TargetingStrategy;
    /// <summary>
    /// The default usage type for the card (base, degraded, or single-use).
    /// </summary>
    public CardUsageType defaultUsageType;
    /// <summary>
    /// The default max number of uses 
    /// </summary>
    public int defaultMaxUses;
}
