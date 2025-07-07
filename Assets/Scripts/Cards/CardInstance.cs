using System;
using System.Collections.Generic;

[Serializable]
public enum CardUpgradeType { Base, Radiant, Cursed }
[Serializable]
public enum CardUsageType { Infinite, Degrading, SingleUse }
[Serializable]
public enum BurntLevel { None, Slightly, Severely, Burnt }

[Serializable]
public class CardInstance(Card cardAsset)
{
    public Card cardAsset = cardAsset;
    public CardUpgradeType UpgradeType { get; set; } = CardUpgradeType.Base;
    public CardUsageType UsageType { get; set; } = cardAsset.defaultUsageType;
    public int UsesRemaining { get; set; } = cardAsset.defaultMaxUses;
    public Guid UniqueId { get; set; } = Guid.NewGuid();

    // Temporary/relic-based buffs/debuffs
    public int TemporaryCostModifier { get; set; } = 0;
    public int TemporaryDamageModifier { get; set; } = 0;
    public List<string> TemporaryEffects { get; set; } = new();

    // Visual state
    public BurntLevel BurntLevel { get; set; } = BurntLevel.None;
    public bool IsSingleUsed { get; set; } = false;

    public void ApplyUpgrade(CardUpgradeType upgrade)
    {
        UpgradeType = upgrade;
        switch (upgrade)
        {
            case CardUpgradeType.Radiant:
                UsageType = CardUsageType.SingleUse;
                UsesRemaining = 1;
                break;
            case CardUpgradeType.Cursed:
                UsageType = CardUsageType.Degrading;
                UsesRemaining = 3;
                break;
            default:
                UsageType = cardAsset.defaultUsageType;
                UsesRemaining = cardAsset.defaultMaxUses;
                break;
        }
    }

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

    public void RefreshUses()
    {
        if (UsageType == CardUsageType.Degrading)
        {
            UsesRemaining = cardAsset.defaultMaxUses;
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
