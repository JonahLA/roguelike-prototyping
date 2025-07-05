using UnityEngine;

/// <summary>
/// Represents a boss room in the game. Inherits from the base <see cref="Room"/> class.
/// </summary>
/// <remarks>
/// This room type typically initiates a boss encounter when the player enters
/// and may have specific logic for locking doors until the boss is defeated.
/// </remarks>
public class BossRoom : Room
{
    /// <summary>
    /// Called when the player enters this room.
    /// </summary>
    /// <remarks>
    /// If the room has not been cleared, this method logs a message indicating
    /// that boss fight logic and door locking should be implemented.
    /// It calls the base class <see cref="Room.OnPlayerEnter()"/> method.
    /// </remarks>
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();

        if (isCleared) return;
        CloseDoors();
        // TODO: spawn boss
    }
}
