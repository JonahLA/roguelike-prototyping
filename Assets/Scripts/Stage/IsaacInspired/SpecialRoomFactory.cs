using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Factory class responsible for creating, initializing, and connecting rooms and their doors within the stage.
/// It uses room templates and a grid system to manage room placement and instantiation.
/// </summary>
public class SpecialRoomFactory
{
    private readonly Dictionary<RoomType, List<RoomTemplate>> _roomTemplates;
    private readonly Transform _defaultParent;
    private readonly float _roomWorldSeparation;

    private const float DefaultRoomWorldSeparation = 50f;
    private const float RoomGizmoDepth = 0.1f;
    private const float DoorGizmoRadius = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialRoomFactory"/> class.
    /// </summary>
    /// <param name="roomTemplates">A dictionary mapping room types to lists of available templates.</param>
    /// <param name="defaultParent">The default parent transform for instantiated room GameObjects. Can be null.</param>
    /// <param name="roomWorldSeparation">The separation distance in world units between the centers of adjacent rooms.</param>
    public SpecialRoomFactory(Dictionary<RoomType, List<RoomTemplate>> roomTemplates, Transform defaultParent = null, float roomWorldSeparation = DefaultRoomWorldSeparation)
    {
        _roomTemplates = roomTemplates;
        _defaultParent = defaultParent;
        _roomWorldSeparation = roomWorldSeparation;
    }

    /// <summary>
    /// Creates and initializes a room of a specified type at a given grid position.
    /// </summary>
    /// <param name="type">The <see cref="RoomType"/> of the room to create.</param>
    /// <param name="position">The grid position (<see cref="Vector2Int"/>) where the room should be placed.</param>
    /// <param name="specificTemplate">A specific <see cref="RoomTemplate"/> to use. If null, a random template for the type will be chosen.</param>
    /// <param name="parent">The parent transform for the new room GameObject. If null, <see cref="_defaultParent"/> will be used.</param>
    /// <param name="doorPrefab">The prefab to use for instantiating doors. If null, doors will not be created.</param>
    /// <param name="random">A <see cref="System.Random"/> instance for template selection. If null, <see cref="UnityEngine.Random"/> will be used.</param>
    /// <param name="entryDirection">An optional <see cref="Direction"/> indicating an entry point. Doors in this direction might be excluded from <see cref="RoomPlacementResult.AvailableOutgoingDoors"/>.</param>
    /// <returns>A <see cref="RoomPlacementResult"/> containing the placed room and its available outgoing doors. Returns null or a result with Success=false if creation fails.</returns>
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
    
    /// <summary>
    /// Creates and configures doors for a given room based on its template's door configurations.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> for which to create doors.</param>
    /// <param name="doorPrefab">The <see cref="GameObject"/> prefab to use for instantiating doors.</param>
    /// <remarks>
    /// This method assumes the <paramref name="room"/> has a valid <see cref="Room.template"/> assigned.
    /// Doors are initially set to a hidden state.
    /// </remarks>
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
    
    /// <summary>
    /// Orients a door GameObject based on its specified <see cref="Direction"/>.
    /// </summary>
    /// <param name="doorObj">The door <see cref="GameObject"/> to orient.</param>
    /// <param name="direction">The <see cref="Direction"/> the door should face.</param>
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

    /// <summary>
    /// Calculates the world position for a room based on its grid position and the configured room separation.
    /// </summary>
    /// <param name="gridPosition">The <see cref="Vector2Int"/> grid coordinates of the room.</param>
    /// <returns>The <see cref="Vector3"/> world position for the center of the room.</returns>
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        // Use _roomWorldSeparation for spacing instead of _roomSize
        return new Vector3(gridPosition.x * _roomWorldSeparation, gridPosition.y * _roomWorldSeparation, 0);
    }

    /// <summary>
    /// Iterates through all rooms in the provided <see cref="StageGrid"/> and attempts to connect adjacent rooms by linking their doors.
    /// </summary>
    /// <param name="grid">The <see cref="StageGrid"/> containing the rooms to connect.</param>
    /// <remarks>
    /// This method calls <see cref="ConnectDoorsInDirection"/> for each potential connection.
    /// Doors that cannot be connected (e.g., no adjacent room or no corresponding door) are typically set to a "Wall" state.
    /// </remarks>
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

    /// <summary>
    /// Attempts to connect a door in the current room to a corresponding door in an adjacent room.
    /// </summary>
    /// <param name="room">The current <see cref="Room"/> from which the connection originates.</param>
    /// <param name="grid">The <see cref="StageGrid"/> used to find the adjacent room.</param>
    /// <param name="direction">The <see cref="Direction"/> from the current room towards the potential adjacent room.</param>
    /// <param name="adjacentPos">The grid position (<see cref="Vector2Int"/>) of the potential adjacent room.</param>
    /// <returns><c>true</c> if the doors were successfully connected; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// If no adjacent room exists, or if the adjacent room does not have a corresponding door,
    /// the door in the current room is set to a "Wall" state.
    /// Uses <see cref="DirectionUtils.GetOppositeDirection"/> for finding the matching door direction.
    /// </remarks>
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

        Direction oppositeDirection = DirectionUtils.GetOppositeDirection(direction); // Changed for consistency

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

    /// <summary>
    /// Draws Gizmos in the Unity editor to visualize the rooms and their types within the grid.
    /// </summary>
    /// <param name="grid">The <see cref="StageGrid"/> containing the rooms to visualize.</param>
    /// <remarks>This method should typically be called from an `OnDrawGizmos` method of a MonoBehaviour.</remarks>
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
                    
                    Gizmos.DrawWireCube(worldPos, new Vector3(_roomWorldSeparation, _roomWorldSeparation, RoomGizmoDepth));
                    
                    DrawDoorGizmos(room, worldPos);
                }
            }
        }
    }
    
    /// <summary>
    /// Draws Gizmos for the connected doors of a specific room.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> whose connected doors are to be visualized.</param>
    /// <param name="roomCenter">The world center position of the room.</param>
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
                Gizmos.DrawSphere(doorPos, DoorGizmoRadius);
            }
        }
    }
}
