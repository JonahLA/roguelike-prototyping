/// <summary>
/// Represents the type of a card in the game.
/// </summary>
public enum CardType
{
    /// <summary>
    /// A base card with no special properties.
    /// </summary>
    Base,
    /// <summary>
    /// A radiant card, typically associated with solid buggs with little to no risk.
    /// </summary>
    Radiant,
    /// <summary>
    /// A cursed card, typically associated with huge benefits with huge risk.
    /// </summary>
    Cursed,
    /// <summary>
    /// A degrading card, which may lose power or have diminishing returns.
    /// </summary>
    Degrading,
    /// <summary>
    /// A buff card, used to enhance the player or allies.
    /// </summary>
    Buff
}
