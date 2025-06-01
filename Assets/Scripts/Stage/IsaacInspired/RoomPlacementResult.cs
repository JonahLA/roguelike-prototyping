using System.Collections.Generic;

public class RoomPlacementResult
{
    public Room PlacedRoom { get; }
    public List<Direction> AvailableOutgoingDoors { get; }

    public RoomPlacementResult(Room placedRoom, List<Direction> availableOutgoingDoors)
    {
        PlacedRoom = placedRoom;
        AvailableOutgoingDoors = availableOutgoingDoors ?? new List<Direction>();
    }

    // Optional: Add a property to easily check if placement was successful
    public bool Success => PlacedRoom != null;
}