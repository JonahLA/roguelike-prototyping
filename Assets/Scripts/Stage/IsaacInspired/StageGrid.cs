using System.Collections.Generic;
using UnityEngine;

public class StageGrid
{
    private Dictionary<Vector2Int, Room> _rooms = new();
    private Vector2Int _gridSize;
    private Vector2Int _startRoomPosition;
    private Vector2Int _bossRoomPosition;
    private List<Vector2Int> _specialRoomPositions = new();

    public Vector2Int GetStartRoomPosition() => _startRoomPosition;
    public Vector2Int GetBossRoomPosition() => _bossRoomPosition;
    public List<Vector2Int> GetSpecialRoomPositions() => _specialRoomPositions;
    public int RoomCount => _rooms.Count;
    public Vector2Int GridSize => _gridSize;

    public Dictionary<Vector2Int, Room> GetAllRooms() // Added this method
    {
        return _rooms;
    }

    public StageGrid(Vector2Int size)
    {
        _gridSize = size;
    }

    public bool HasRoom(Vector2Int position)
    {
        return _rooms.ContainsKey(position);
    }

    public Room GetRoom(Vector2Int position)
    {
        if (_rooms.TryGetValue(position, out Room room))
        {
            return room;
        }
        return null;
    }

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

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridSize.x &&
               position.y >= 0 && position.y < _gridSize.y;
    }

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

    public Vector2Int GetAvailableNeighbor(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[] {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
        List<Vector2Int> availablePositions = new();

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = position + dir;
            if (IsValidPosition(neighborPos) && !HasRoom(neighborPos))
                availablePositions.Add(neighborPos);
        }

        if (availablePositions.Count > 0)
            return availablePositions[Random.Range(0, availablePositions.Count)];

        return Vector2Int.one * -1;  // invalid position
    }
}
