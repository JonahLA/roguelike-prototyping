using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialRoomFactory
{
    private Dictionary<RoomType, List<RoomTemplate>> _roomTemplates;
    private Transform _defaultParent;
    private float _roomWorldSeparation; // New field for separation
    
    public SpecialRoomFactory(Dictionary<RoomType, List<RoomTemplate>> roomTemplates, Transform defaultParent = null, float roomWorldSeparation = 50f) // Added roomWorldSeparation
    {
        _roomTemplates = roomTemplates;
        _defaultParent = defaultParent;
        _roomWorldSeparation = roomWorldSeparation; // Assign to the new field
    }

    public RoomPlacementResult CreateRoom(RoomType type, Vector2Int position, RoomTemplate specificTemplate = null, Transform parent = null, GameObject doorPrefab = null, System.Random random = null, Direction? entryDirection = null)
    {
        try
        {
            parent = parent ?? _defaultParent;

            RoomTemplate templateToUse;

            if (specificTemplate != null)
            {
                templateToUse = specificTemplate;
            }
            else 
            {
                if (_roomTemplates == null)
                {
                    Debug.LogError("[RoomFactory] Room templates dictionary is null");
                    return null;
                }
                if (!_roomTemplates.ContainsKey(type))
                {
                    Debug.LogError($"[RoomFactory] Room type {type} doesn't exist in templates dictionary");
                    return null;
                }
                if (!_roomTemplates.TryGetValue(type, out var templates) || templates == null || templates.Count == 0)
                {
                    Debug.LogError($"[RoomFactory] No templates available for room type: {type}");
                    return null;
                }
                int templateIndex = random != null ? random.Next(templates.Count) : Random.Range(0, templates.Count);
                templateToUse = templates[templateIndex];
            }

            if (templateToUse == null) 
            {
                Debug.LogError($"[RoomFactory] templateToUse is null for type {type} at position {position}. This should not happen.");
                return null;
            }

            if (templateToUse.roomPrefab == null)
            {
                Debug.LogError($"[RoomFactory] Room template '{templateToUse.roomName}' for type {type} has no roomPrefab assigned");
                return null;
            }

            Vector3 worldPos = GetWorldPosition(position); // This will now use _roomWorldSeparation
            GameObject roomObj = Object.Instantiate(templateToUse.roomPrefab, worldPos, Quaternion.identity, parent);

            Room room = type switch
            {
                RoomType.Start => roomObj.AddComponent<StartRoom>(),
                RoomType.Boss => roomObj.AddComponent<BossRoom>(),
                RoomType.Treasure => roomObj.AddComponent<TreasureRoom>(),
                RoomType.Shop => roomObj.AddComponent<ShopRoom>(),
                _ => roomObj.AddComponent<NormalRoom>()
            };

            // Initialize the room
            room.template = templateToUse; // Assign the chosen/provided template
            room.gridPosition = position;
            roomObj.name = $"Room_{type}_{position.x}_{position.y}";

            // Create doors if a door prefab was provided
            if (doorPrefab != null)
            {
                CreateDoorsForRoom(room, doorPrefab);
            }
            else
            {
                Debug.LogWarning("[RoomFactory] Door prefab is null, skipping door creation");
            }

            // Populate RoomPlacementResult
            List<Direction> availableDoors = templateToUse.possibleDoors.Select(dc => dc.direction).ToList();
            if (entryDirection.HasValue)
            {
                availableDoors.Remove(entryDirection.Value);
            }
            return new RoomPlacementResult(room, availableDoors);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RoomFactory] Error creating room: {e.Message}\n{e.StackTrace}");
            return new RoomPlacementResult(null, new List<Direction>());
        }
    }
    
    public void CreateDoorsForRoom(Room room, GameObject doorPrefab)
    {
        if (room.template == null)
        {
            Debug.LogError("[RoomFactory] Cannot create doors: Room template is null");
            return;
        }

        if (room.template.possibleDoors == null || room.template.possibleDoors.Count == 0)
        {
            Debug.LogWarning($"[RoomFactory] Room template has no doors defined");
            return;
        }

        foreach (DoorConfig doorConfig in room.template.possibleDoors)
        {
            Vector3 doorLocalPos = new Vector3(doorConfig.position.x, doorConfig.position.y, 0);
            GameObject doorObj = Object.Instantiate(doorPrefab, room.transform);
            doorObj.transform.localPosition = doorLocalPos;
            doorObj.name = $"Door_{doorConfig.direction}";

            // Set up the door controller
            DoorController doorController = doorObj.GetComponent<DoorController>();
            if (doorController != null)
            {
                doorController.direction = doorConfig.direction;
                doorController.SetState(DoorState.Hidden); // Start hidden until connected

                // Add to room's door dictionary
                room.doors[doorConfig.direction] = doorController;

                // Orient door based on direction
                OrientDoorBasedOnDirection(doorObj, doorConfig.direction);
            }
            else
            {
                Debug.LogError("[RoomFactory] Door prefab doesn't have a DoorController component");
            }
        }
    }
    
    private void OrientDoorBasedOnDirection(GameObject doorObj, Direction direction)
    {
        // Rotate door based on its direction
        float rotation = direction switch
        {
            Direction.North => 0f,
            Direction.East => 90f,
            Direction.South => 180f,
            Direction.West => 270f,
            _ => 0f
        };
        
        doorObj.transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        // Use _roomWorldSeparation for spacing instead of _roomSize
        return new Vector3(gridPosition.x * _roomWorldSeparation, gridPosition.y * _roomWorldSeparation, 0);
    }

    public void ConnectRoomsInGrid(StageGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError("[RoomFactory] Cannot connect rooms: Grid is null");
            return;
        }
        Debug.Log($"[RoomFactory] Starting ConnectRoomsInGrid for grid size: {grid.GridSize}");

        Vector2Int gridSize = grid.GridSize;
        int connectionsCreated = 0;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int pos = new(x, y);
                Room room = grid.GetRoom(pos);

                if (room == null) continue;

                Debug.Log($"[RoomFactory] Processing room at {pos} ({room.GetType().Name}) for connections.");

                bool north = ConnectDoorsInDirection(room, grid, Direction.North, pos + Vector2Int.up);
                bool east = ConnectDoorsInDirection(room, grid, Direction.East, pos + Vector2Int.right);
                bool south = ConnectDoorsInDirection(room, grid, Direction.South, pos + Vector2Int.down);
                bool west = ConnectDoorsInDirection(room, grid, Direction.West, pos + Vector2Int.left);

                connectionsCreated += (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
            }
        }
        Debug.Log($"[RoomFactory] Finished ConnectRoomsInGrid. Total connections attempted/made in this call: {connectionsCreated}");
    }

    private bool ConnectDoorsInDirection(Room room, StageGrid grid, Direction direction, Vector2Int adjacentPos)
    {
        Debug.Log($"[RoomFactory] Attempting to connect doors for room {room.gridPosition} in direction {direction} to {adjacentPos}");
        if (!room.doors.ContainsKey(direction))
        {
            Debug.LogWarning($"[RoomFactory] Room {room.gridPosition} has no door in direction {direction}.");
            return false;
        }

        DoorController currentRoomDoor = room.doors[direction];
        Room adjacentRoom = grid.GetRoom(adjacentPos);

        if (adjacentRoom == null)
        {
            // No adjacent room, so this door should become a wall.
            Debug.Log($"[RoomFactory] No adjacent room at {adjacentPos} for room {room.gridPosition} ({direction}). Setting door to Wall state.");
            currentRoomDoor.SetState(DoorState.Wall); 
            return false;
        }

        Direction oppositeDirection = DoorController.GetOppositeDirection(direction);

        if (!adjacentRoom.doors.ContainsKey(oppositeDirection))
        {
            Debug.LogWarning($"[RoomFactory] Adjacent room {adjacentPos} ({adjacentRoom.GetType().Name}) has no door in the opposite direction {oppositeDirection} to connect with room {room.gridPosition}. Setting current room's door to Wall.");
            currentRoomDoor.SetState(DoorState.Wall); // Also set this door to wall if its potential partner doesn't exist
            return false;
        }

        DoorController doorInCurrentRoom = room.doors[direction]; // Renamed for clarity
        DoorController doorInAdjacentRoom = adjacentRoom.doors[oppositeDirection]; // Renamed for clarity

        try
        {
            Debug.Log($"[RoomFactory] Connecting door {room.gridPosition}({direction}) to {adjacentPos}({oppositeDirection})");
            doorInCurrentRoom.ConnectTo(adjacentRoom, doorInAdjacentRoom);
            // The ConnectTo method in DoorController handles connecting the other door back and setting states.
            Debug.Log($"[RoomFactory] Successfully connected {room.name} ({direction}) to {adjacentRoom.name} ({oppositeDirection}).");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RoomFactory] Error connecting doors between {room.gridPosition} and {adjacentPos}: {e.Message}\\n{e.StackTrace}");
            // Potentially set both doors to Wall state here if connection fails mid-way, though ConnectTo should handle its own state.
            // doorInCurrentRoom.SetState(DoorState.Wall);
            // if(doorInAdjacentRoom != null) doorInAdjacentRoom.SetState(DoorState.Wall); // If it exists
            return false;
        }
    }

    public void DrawRoomGizmos(StageGrid grid)
    {
        if (grid == null) return;
        
        Vector2Int gridSize = grid.GridSize;
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int pos = new(x, y);
                Room room = grid.GetRoom(pos);
                
                if (room != null)
                {
                    Vector3 worldPos = GetWorldPosition(pos);
                    
                    Gizmos.color = room switch
                    {
                        StartRoom => Color.green,
                        BossRoom => Color.red,
                        TreasureRoom => Color.yellow,
                        ShopRoom => Color.cyan,
                        _ => Color.white
                    };
                    
                    Gizmos.DrawWireCube(worldPos, new Vector3(_roomWorldSeparation, _roomWorldSeparation, 0.1f));
                    
                    DrawDoorGizmos(room, worldPos);
                }
            }
        }
    }
    
    private void DrawDoorGizmos(Room room, Vector3 roomCenter)
    {
        Gizmos.color = Color.blue;
        foreach (var doorPair in room.doors)
        {
            if (doorPair.Value.isConnected)
            {
                Vector3 doorOffset = doorPair.Key switch
                {
                    Direction.North => new Vector3(0, _roomWorldSeparation/2, 0),
                    Direction.East => new Vector3(_roomWorldSeparation/2, 0, 0),
                    Direction.South => new Vector3(0, -_roomWorldSeparation/2, 0),
                    Direction.West => new Vector3(-_roomWorldSeparation/2, 0, 0),
                    _ => Vector3.zero
                };
                
                Vector3 doorPos = roomCenter + doorOffset;
                Gizmos.DrawSphere(doorPos, 1f);
            }
        }
    }
}
