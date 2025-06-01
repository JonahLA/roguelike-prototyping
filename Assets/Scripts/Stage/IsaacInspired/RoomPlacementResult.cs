using System.Collections.Generic;

/// <summary>
/// Represents the outcome of an attempt to place a room within the stage grid.
/// It holds a reference to the room that was placed (if successful) and a list of 
/// directions in which this room has doors that could potentially connect to adjacent rooms.
/// </summary>
public class RoomPlacementResult
{
    /// <summary>
    /// Gets the <see cref="Room"/> instance that was placed. 
    /// Will be null if the placement was unsuccessful.
    /// </summary>
    public Room PlacedRoom { get; }

    /// <summary>
    /// Gets a list of <see cref="Direction"/>s representing the outgoing doors 
    /// of the <see cref="PlacedRoom"/> that are available for connection.
    /// This list might be based on the room's template or dynamic placement logic.
    /// </summary>
    public List<Direction> AvailableOutgoingDoors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomPlacementResult"/> class.
    /// </summary>
    /// <param name="placedRoom">The room that was placed. This can be null if placement failed.</param>
    /// <param name="availableOutgoingDoors">A list of directions for potential outgoing connections from the placed room. If null, an empty list will be used.</param>
    public RoomPlacementResult(Room placedRoom, List<Direction> availableOutgoingDoors)
    {
        PlacedRoom = placedRoom;
        AvailableOutgoingDoors = availableOutgoingDoors ?? new List<Direction>();
    }

    /// <summary>
    /// Gets a value indicating whether the room placement was successful.
    /// Placement is considered successful if <see cref="PlacedRoom"/> is not null.
    /// </summary>
    public bool Success => PlacedRoom != null;
}