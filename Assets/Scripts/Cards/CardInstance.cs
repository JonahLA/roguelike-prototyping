using System;
using System.Collections.Generic;

/// <summary>
/// Represents the upgrade type of a card.
/// </summary>
[Serializable]
public enum CardUpgradeType { Base, Radiant, Cursed }

/// <summary>
/// Represents the usage type of a card.
/// </summary>
[Serializable]
public enum CardUsageType { Infinite, Degrading, SingleUse }

/// <summary>
/// Represents the burnt level visual state of a card.
/// </summary>
[Serializable]
public enum BurntLevel { None = 0, Slightly = 1, Severely = 2, Burnt = 3 }

/// <summary>
/// An instance of a card in the player's deck, including runtime state and temporary effects.
/// </summary>
[Serializable]
public class CardInstance
{
    /// <summary>
    /// The card asset this instance is based on.
    /// </summary>
    private CardSO _cardAsset;

    /// <summary>
    /// The upgrade type of this card instance.
    /// </summary>
    public CardUpgradeType UpgradeType { get; set; } = CardUpgradeType.Base;

    /// <summary>
    /// The usage type of this card instance.
    /// </summary>
    public CardUsageType UsageType { get; set; } = CardUsageType.Infinite;

    /// <summary>
    /// The number of uses remaining for this card instance.
    /// </summary>
    public int UsesRemaining { get; set; } = 3;

    /// <summary>
    /// Unique identifier for this card instance.
    /// </summary>
    public Guid UniqueId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Temporary modifier to the card's cost (e.g., from relics or effects).
    /// </summary>
    public int TemporaryCostModifier { get; set; } = 0;

    /// <summary>
    /// Temporary modifier to the card's damage (e.g., from relics or effects).
    /// </summary>
    public int TemporaryDamageModifier { get; set; } = 0;

    /// <summary>
    /// List of temporary effect identifiers currently applied to this card instance.
    /// </summary>
    public List<string> TemporaryEffects { get; set; } = new();

    /// <summary>
    /// The burnt level visual state of this card instance.
    /// </summary>
    public BurntLevel BurntLevel { get; set; } = BurntLevel.None;

    /// <summary>
    /// Whether this card has been single-used and is now spent.
    /// </summary>
    public bool IsSingleUsed { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardInstance"/> class.
    /// </summary>
    /// <param name="cardAsset">The card asset to base this instance on.</param>
    public CardInstance(CardSO cardAsset)
    {
        _cardAsset = cardAsset;
        UsageType = cardAsset.defaultUsageType;
        UsesRemaining = cardAsset.defaultMaxUses;
    }

    /// <summary>
    /// Applies a use to this card instance, decrementing uses and updating state as appropriate.
    /// </summary>
    public void Use()
    {
        if (UsageType == CardUsageType.Degrading && UsesRemaining > 0)
        {
            UsesRemaining--;
            // BurntLevel logic can be updated here as needed
        }
        else if (UsageType == CardUsageType.SingleUse)
        {
            UsesRemaining = 0;
            IsSingleUsed = true;
        }
    }

    /// <summary>
    /// Refreshes the uses for this card instance, resetting state as appropriate.
    /// </summary>
    public void RefreshUses()
    {
        if (UsageType == CardUsageType.Degrading)
        {
            UsesRemaining = _cardAsset.defaultMaxUses;
            BurntLevel = BurntLevel.None;
        }
        else if (UsageType == CardUsageType.SingleUse)
        {
            UsesRemaining = 1;
            IsSingleUsed = false;
        }
    }
    // Serialization helpers can be added here as needed
}
