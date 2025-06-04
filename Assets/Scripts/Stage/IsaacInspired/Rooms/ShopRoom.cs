using UnityEngine;

/// <summary>
/// Represents a shop room where the player can interact with vendors or purchase items.
/// </summary>
public class ShopRoom : Room
{
    /// <summary>
    /// Called when the player enters the shop room.
    /// Initializes shop-specific UI and logic.
    /// Shop rooms are typically marked as cleared upon entry.
    /// </summary>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        // Logic for shop interaction
        Debug.Log($"ShopRoom {gameObject.name}: Player entered. Implement shop UI/logic.");
        if (!isCleared) OnRoomClear(); // Shop rooms are often cleared on entry
    }
}
