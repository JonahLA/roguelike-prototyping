using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the grid structure of a stage, managing rooms and their positions.
/// </summary>
public class StageGrid
{
    private readonly Dictionary<Vector2Int, Room> _rooms = new();
    private readonly Vector2Int _gridSize; // Made readonly as it's set in constructor and not changed
    private Vector2Int _startRoomPosition;
    private Vector2Int _bossRoomPosition;
    private readonly List<Vector2Int> _specialRoomPositions = new();

    /// <summary>
    /// Gets the grid position of the starting room.
    /// </summary>
    /// <returns>The grid position of the start room.</returns>
    public Vector2Int GetStartRoomPosition() => _startRoomPosition;

    /// <summary>
    /// Gets the grid position of the boss room.
    /// </summary>
    /// <returns>The grid position of the boss room.</returns>
    public Vector2Int GetBossRoomPosition() => _bossRoomPosition;

    /// <summary>
    /// Gets a list of grid positions for all special rooms (excluding start and boss rooms).
    /// </summary>
    /// <returns>A list of <see cref="Vector2Int"/> representing special room positions.</returns>
    public List<Vector2Int> GetSpecialRoomPositions() => _specialRoomPositions; // Consider returning IReadOnlyList

    /// <summary>
    /// Gets the total number of rooms currently in the grid.
    /// </summary>
    public int RoomCount => _rooms.Count;

    /// <summary>
    /// Gets the dimensions of the grid.
    /// </summary>
    public Vector2Int GridSize => _gridSize;

    /// <summary>
    /// Gets a dictionary of all rooms in the grid, keyed by their position.
    /// </summary>
    /// <remarks>
    /// This returns a direct reference to the internal dictionary.
    /// For a read-only view, consider using <see cref="GetAllRoomsReadOnly"/>.
    /// </remarks>
    /// <returns>A dictionary containing all rooms by their <see cref="Vector2Int"/> position.</returns>
    public Dictionary<Vector2Int, Room> GetAllRooms()
    {
        return _rooms;
    }

    /// <summary>
    /// Gets a read-only dictionary of all rooms in the grid.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of rooms by their position.</returns>
    public IReadOnlyDictionary<Vector2Int, Room> GetAllRoomsReadOnly()
    {
        return _rooms;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StageGrid"/> class with the specified size.
    /// </summary>
    /// <param name="size">The dimensions of the grid.</param>
    public StageGrid(Vector2Int size)
    {
        _gridSize = size;
    }

    /// <summary>
    /// Checks if a room exists at the specified grid position.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns>True if a room exists at the position; otherwise, false.</returns>
    public bool HasRoom(Vector2Int position)
    {
        return _rooms.ContainsKey(position);
    }

    /// <summary>
    /// Gets the room at the specified grid position.
    /// </summary>
    /// <param name="position">The grid position of the room to retrieve.</param>
    /// <returns>The <see cref="Room"/> at the specified position, or null if no room exists there.</returns>
    public Room GetRoom(Vector2Int position)
    {
        _rooms.TryGetValue(position, out Room room);
        return room;
    }

    /// <summary>
    /// Adds a room to the grid at the specified position.
    /// </summary>
    /// <param name="position">The grid position where the room will be added.</param>
    /// <param name="room">The <see cref="Room"/> object to add.</param>
    /// <remarks>
    /// The room will not be added if a room already exists at the position or if the position is invalid.
    /// Updates special room tracking based on the type of room added.
    /// </remarks>
    public void AddRoom(Vector2Int position, Room room)
    {
        if (!HasRoom(position) && IsValidPosition(position))
        {
            _rooms[position] = room;
            room.gridPosition = position;

            if (room is StartRoom)
            {
                _startRoomPosition = position;
            }
            else if (room is BossRoom)
            {
                _bossRoomPosition = position;
            }
            else if (!(room is NormalRoom))
            {
                _specialRoomPositions.Add(position);
            }
        }
    }

    /// <summary>
    /// Determines if the given grid position is within the bounds of the stage grid.
    /// </summary>
    /// <param name="position">The grid position to validate.</param>
    /// <returns>True if the position is within the grid boundaries; otherwise, false.</returns>
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridSize.x &&
               position.y >= 0 && position.y < _gridSize.y;
    }

    /// <summary>
    /// Gets a list of positions connected to the room at the given position via doors.
    /// </summary>
    /// <param name="position">The grid position of the room whose connections are to be found.</param>
    /// <returns>A list of <see cref="Vector2Int"/> representing connected room positions. Returns an empty list if the room does not exist or has no connected doors.</returns>
    public List<Vector2Int> GetConnectedPositions(Vector2Int position)
    {
        List<Vector2Int> connectedPositions = new();
        if (HasRoom(position))
        {
            Room room = GetRoom(position);

            // Check for each door and add connected rooms
            foreach (var doorPair in room.doors)
            {
                Direction direction = doorPair.Key;
                DoorController door = doorPair.Value;

                if (door != null && door.isConnected)
                {
                    Vector2Int offset = DirectionToOffset(direction);
                    connectedPositions.Add(position + offset);
                }
            }
        }

        return connectedPositions;
    }

    private Vector2Int DirectionToOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector2Int.up,
            Direction.East => Vector2Int.right,
            Direction.South => Vector2Int.down,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero // default case should never happen with proper enum values
        };
    }

    /// <summary>
    /// Tries to find an available, unoccupied neighboring position.
    /// </summary>
    /// <param name="position">The position from which to find a neighbor.</param>
    /// <param name="neighborPosition">When this method returns true, contains the <see cref="Vector2Int"/> of an available neighbor. Otherwise, it contains a default value.</param>
    /// <returns>True if an available neighbor position was found; otherwise, false.</returns>
    public bool TryGetAvailableNeighbor(Vector2Int position, out Vector2Int neighborPosition)
    {
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
        List<Vector2Int> availablePositions = new();

        foreach (Vector2Int dir in directions)
        {
            Vector2Int currentNeighborPos = position + dir;
            if (IsValidPosition(currentNeighborPos) && !HasRoom(currentNeighborPos))
            {
                availablePositions.Add(currentNeighborPos);
            }
        }

        if (availablePositions.Count > 0)
        {
            neighborPosition = availablePositions[Random.Range(0, availablePositions.Count)];
            return true;
        }

        neighborPosition = default; // Indicates no available neighbor found
        return false;
    }

    // Original GetAvailableNeighbor method can be removed or kept if legacy code depends on the -1,-1 return
    // public Vector2Int GetAvailableNeighbor(Vector2Int position)
    // {
    //     Vector2Int[] directions = new Vector2Int[] {
    //         Vector2Int.up,
    //         Vector2Int.right,
    //         Vector2Int.down,
    //         Vector2Int.left
    //     };
    //     List<Vector2Int> availablePositions = new();

    //     foreach (Vector2Int dir in directions)
    //     {
    //         Vector2Int neighborPos = position + dir;
    //         if (IsValidPosition(neighborPos) && !HasRoom(neighborPos))
    //             availablePositions.Add(neighborPos);
    //     }

    //     if (availablePositions.Count > 0)
    //         return availablePositions[Random.Range(0, availablePositions.Count)];

    //     return Vector2Int.one * -1;  // invalid position
    // }
}
