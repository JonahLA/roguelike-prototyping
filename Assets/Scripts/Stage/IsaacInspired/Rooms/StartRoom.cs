using UnityEngine;

/// <summary>
/// Represents the starting room where the player begins their journey.
/// Start rooms are typically safe areas with no enemies.
/// </summary>
public class StartRoom : Room
{
    /// <summary>
    /// Called when the player enters the start room.
    /// Start rooms are automatically marked as cleared since they typically have no enemies.
    /// </summary>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        base.OnRoomClear();
    }
}
