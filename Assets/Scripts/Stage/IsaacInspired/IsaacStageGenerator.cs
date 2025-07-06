using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for LINQ

/// <summary>
/// Responsible for procedural generation of the stage layout, 
/// including the main path, branches, and special rooms like treasure and shop rooms.
/// It uses various settings to control the generation process, such as grid size, path lengths, and room templates.
/// </summary>
public class IsaacStageGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("The dimensions of the grid on which rooms will be placed (e.g., 8x8).")]
    [SerializeField] private Vector2Int _gridSize = new(8, 8);
    [Tooltip("The minimum number of rooms in the main path, excluding start and boss rooms.")]
    [SerializeField] private int _minMainPathLength = 3;
    [Tooltip("The maximum number of rooms in the main path, excluding start and boss rooms.")]
    [SerializeField] private int _maxMainPathLength = 7;
    [Tooltip("The total number of special rooms (e.g., Treasure, Shop) to attempt to place.")]
    [SerializeField] private int _specialRoomsCount = 2;
    [Tooltip("The probability (0.0 to 1.0) that a branch will attempt to generate off a main path room.")]
    [SerializeField] private float _branchProbability = 0.5f;
    [Tooltip("The seed used for the random number generator. If Use Random Seed is true, this is ignored.")]
    [SerializeField] private int _seed = 0;
    [Tooltip("If true, a random seed will be used for generation. If false, the specified Seed will be used.")]
    [SerializeField] private bool _useRandomSeed = true;

    [Header("Room Placement")]
    [Tooltip("The separation distance in world units between the centers of adjacent rooms.")]
    [SerializeField] private float _roomWorldSeparation = 50f;

    [Header("Room Templates")]
    [Tooltip("List of templates to be used for the Start room.")]
    [SerializeField] private List<RoomTemplate> _startRoomTemplates = new();
    [Tooltip("List of templates for regular rooms found in the main path and branches.")]
    [SerializeField] private List<RoomTemplate> _normalRoomTemplates = new();
    [Tooltip("List of templates for the Boss room, typically found at the end of the main path.")]
    [SerializeField] private List<RoomTemplate> _bossRoomTemplates = new();
    [Tooltip("List of templates for Treasure rooms, containing special rewards.")]
    [SerializeField] private List<RoomTemplate> _treasureRoomTemplates = new();
    [Tooltip("List of templates for Shop rooms, where players can purchase items.")]
    [SerializeField] private List<RoomTemplate> _shopRoomTemplates = new();

    [Header("Prefabs")]
    [Tooltip("The prefab used to instantiate doors between connected rooms.")]
    [SerializeField] private GameObject _doorPrefab;

    private StageGrid _stageGrid;
    private System.Random _random;
    private Dictionary<RoomType, List<RoomTemplate>> _roomTemplatesByType = new();
    private SpecialRoomFactory _roomFactory;
    private RoomContentPopulator _roomContentPopulator;

    private static readonly Vector2Int InvalidPosition = new(-1, -1); // Sentinel for invalid/not-found positions
    private const int MaxSpecialRoomPlacementAttempts = 20;

    /// <summary>
    /// Initializes and executes the entire stage generation process.
    /// This includes placing the start room, generating the main path to a boss room,
    /// creating side branches, placing special rooms, and finally connecting all rooms with doors.
    /// </summary>
    /// <returns>The generated <see cref="StageGrid"/> containing all placed rooms and their connections.</returns>
    public StageGrid GenerateStage()
    {
        InitializeGenerator();

        Vector2Int startPos = _gridSize / 2;
        RoomPlacementResult startRoomResult = PlaceRoom(startPos, RoomType.Start);

        if (startRoomResult == null || !startRoomResult.Success || startRoomResult.PlacedRoom == null)
        {
            Debug.LogError("[StageGen] Failed to place Start Room or PlacedRoom is null. Aborting generation.");
            return _stageGrid; 
        }

        int mainPathLength = _random.Next(_minMainPathLength, _maxMainPathLength + 1);
        List<Vector2Int> mainPath = GenerateMainPath(startPos, mainPathLength, startRoomResult);

        if (mainPath == null || mainPath.Count <= 1) 
        {
            Debug.LogError("[StageGen] Failed to generate a valid main path including a boss room. Aborting generation.");
            return _stageGrid; 
        }
        
        GenerateBranches(mainPath);
        PlaceSpecialRooms();
        ConnectRooms();

        Debug.Log($"[StageGen] Stage generation complete with {_stageGrid.RoomCount} rooms");
        return _stageGrid;
    }

    private void InitializeGenerator()
    {
        int seed = _useRandomSeed ? Random.Range(0, int.MaxValue) : _seed;
        _random = new System.Random(seed);
        Debug.Log($"[StageGen] Generating stage with seed: {seed}");

        _stageGrid = new StageGrid(_gridSize); 

        _roomTemplatesByType.Clear();
        _roomTemplatesByType[RoomType.Start] = _startRoomTemplates;
        _roomTemplatesByType[RoomType.Normal] = _normalRoomTemplates;
        _roomTemplatesByType[RoomType.Boss] = _bossRoomTemplates;
        _roomTemplatesByType[RoomType.Treasure] = _treasureRoomTemplates;
        _roomTemplatesByType[RoomType.Shop] = _shopRoomTemplates;

        // Correctly initialize SpecialRoomFactory
        _roomFactory = new SpecialRoomFactory(_roomTemplatesByType, transform, _roomWorldSeparation);

        // Get or add RoomContentPopulator component
        _roomContentPopulator = GetComponent<RoomContentPopulator>();
        if (_roomContentPopulator == null)
        {
            _roomContentPopulator = gameObject.AddComponent<RoomContentPopulator>();
        }
    }

    /// <summary>
    /// Generates the main path of rooms, starting from the initial room and attempting to end with a boss room.
    /// The path consists primarily of Normal rooms, with their content difficulty increasing along the path.
    /// </summary>
    /// <param name="startPos">The grid position of the starting room.</param>
    /// <param name="length">The desired number of Normal rooms in the main path before attempting to place the boss room.</param>
    /// <param name="initialPreviousRoomResult">The placement result of the starting room, used to determine initial connection possibilities and door requirements.</param>
    /// <returns>A list of <see cref="Vector2Int"/> representing the grid positions of the rooms in the main path, including the boss room if successfully placed. Returns null or a short list if generation fails at critical steps.</returns>
    private List<Vector2Int> GenerateMainPath(Vector2Int startPos, int length, RoomPlacementResult initialPreviousRoomResult)
    {
        List<Vector2Int> path = new() { startPos };
        Vector2Int currentPos = startPos;
        RoomPlacementResult previousRoomResult = initialPreviousRoomResult;

        for (int i = 0; i < length; i++)
        {
            Vector2Int nextPos = GetNextRoomPosition(currentPos, path, previousRoomResult, out Direction requiredDoorDirectionOnNewRoom);

            if (nextPos == Vector2Int.one * -1)
            {
                Debug.LogWarning($"[StageGen] MainPath: No valid position found after {currentPos} for a Normal room. Path may be shorter than intended.");
                break; 
            }

            RoomPlacementResult newRoomResult = PlaceRoom(nextPos, RoomType.Normal, requiredDoorDirectionOnNewRoom);
            if (newRoomResult == null || !newRoomResult.Success || newRoomResult.PlacedRoom == null)
            {
                Debug.LogWarning($"[StageGen] MainPath: Could not place Normal room at {nextPos} requiring door {requiredDoorDirectionOnNewRoom} or PlacedRoom is null. Path may be shorter.");
                break;
            }
            path.Add(nextPos);
            currentPos = nextPos;
            previousRoomResult = newRoomResult;
        }

        if (previousRoomResult != null && previousRoomResult.Success && previousRoomResult.PlacedRoom != null &&
            (previousRoomResult.PlacedRoom.template.roomType != RoomType.Start || (previousRoomResult.PlacedRoom.template.roomType == RoomType.Start && path.Count > 1)))
        {
            Vector2Int bossRoomPos = GetNextRoomPosition(currentPos, path, previousRoomResult, out Direction bossRoomRequiredDoorDir);

            if (bossRoomPos != Vector2Int.one * -1)
            {
                RoomPlacementResult bossRoomResult = PlaceRoom(bossRoomPos, RoomType.Boss, bossRoomRequiredDoorDir);
                if (bossRoomResult != null && bossRoomResult.Success && bossRoomResult.PlacedRoom != null)
                {
                    path.Add(bossRoomPos);
                    Debug.Log($"[StageGen] Boss room placed at {bossRoomPos} connected to {currentPos}");
                }
                else
                {
                    Debug.LogWarning($"[StageGen] Failed to place Boss room connected to {currentPos} or PlacedRoom is null.");
                }
            }
            else
            {
                Debug.LogWarning($"[StageGen] No valid position found for Boss room connected to {currentPos}.");
            }
        }
        else
        {
            Debug.LogWarning("[StageGen] Main path too short or previous room invalid/null, cannot place Boss room.");
        }
        
        return path;
    }

    /// <summary>
    /// Iterates through the main path (excluding the boss room) and randomly attempts to generate branches off each room.
    /// The probability of a branch forming is determined by <see cref="_branchProbability"/>.
    /// </summary>
    /// <param name="mainPath">The list of grid positions representing the main path rooms.</param>
    private void GenerateBranches(List<Vector2Int> mainPath)
    {
        for (int i = 0; i < mainPath.Count - 1; i++)  // Iterate up to the second to last room
        {
            if (_random.NextDouble() <= _branchProbability)
            {
                Vector2Int branchParentGridPos = mainPath[i];
                Room branchParentRoom = _stageGrid.GetRoom(branchParentGridPos); 
                
                if (branchParentRoom != null)
                {
                    float parentDifficulty;
                    if (i == 0 && branchParentRoom.template.roomType == RoomType.Start) 
                    {
                        parentDifficulty = 0f; 
                    }
                    else
                    {
                        parentDifficulty = Mathf.Clamp01((float)(i + 1) / _maxMainPathLength);
                    }

                    List<Direction> parentPotentialOutgoingDoors = branchParentRoom.template.possibleDoors
                                                               .Select(dc => dc.direction)
                                                               .ToList();
                    RoomPlacementResult parentRoomInfo = new RoomPlacementResult(branchParentRoom, parentPotentialOutgoingDoors);

                    int branchLength = 1 + _random.Next(Mathf.Max(1, _maxMainPathLength / 3));
                    CreateBranch(branchParentGridPos, parentRoomInfo, branchLength, parentDifficulty);
                }
                else
                {
                    Debug.LogWarning($"[StageGen] Branch parent room at {branchParentGridPos} is null. Skipping branch.");
                }
            }
        }
    }

    /// <summary>
    /// Creates a branch of a specified length, consisting of Normal rooms, starting from a parent room.
    /// The difficulty of rooms in the branch increases incrementally from the parent room's difficulty.
    /// </summary>
    /// <param name="parentInitialPos">The grid position of the room from which this branch originates.</param>
    /// <param name="parentInitialRoomResult">The placement result of the parent room, used for initial connection and door requirements.</param>
    /// <param name="length">The desired number of rooms in the branch.</param>
    /// <param name="initialParentDifficulty">The difficulty rating of the parent room, used as a base for calculating the difficulty of rooms in this branch.</param>
    private void CreateBranch(Vector2Int parentInitialPos, RoomPlacementResult parentInitialRoomResult, int length, float initialParentDifficulty)
    {
        List<Vector2Int> currentBranchPath = new()
        {
            parentInitialPos
        }; 

        Vector2Int currentPosInBranch = parentInitialPos;
        RoomPlacementResult previousRoomResultInBranch = parentInitialRoomResult;

        for (int j = 0; j < length; j++) 
        {
            Vector2Int nextPos = GetNextRoomPosition(currentPosInBranch, currentBranchPath, previousRoomResultInBranch, out Direction requiredDoorOnNewRoom);

            if (nextPos == Vector2Int.one * -1)
            {
                Debug.LogWarning($"[StageGen] CreateBranch: No valid position found from {currentPosInBranch} for branch room {j+1}/{length}. Branch may be shorter.");
                break; 
            }

            RoomPlacementResult branchRoomPlacement = PlaceRoom(nextPos, RoomType.Normal, requiredDoorOnNewRoom);

            if (branchRoomPlacement != null && branchRoomPlacement.Success && branchRoomPlacement.PlacedRoom != null)
            {
                currentBranchPath.Add(nextPos); 
                currentPosInBranch = nextPos;
                previousRoomResultInBranch = branchRoomPlacement;
            }
            else
            {
                Debug.LogWarning($"[StageGen] CreateBranch: Failed to place branch room at {nextPos} (template: {branchRoomPlacement?.PlacedRoom?.template?.roomName ?? "N/A"}). Stopping branch.");
                break;
            }
        }
    }

    /// <summary>
    /// Attempts to place a predefined number of special rooms (Treasure, Shop) in valid locations.
    /// Special rooms are typically placed adjacent to existing Normal rooms that are not already overcrowded.
    /// </summary>
    private void PlaceSpecialRooms()
    {
        int treasureRoomsPlaced = 0;
        int shopRoomsPlaced = 0;
        int totalSpecialRoomsToPlace = _specialRoomsCount;
        int targetTreasureRooms = Mathf.CeilToInt(totalSpecialRoomsToPlace / 2f);
        int targetShopRooms = Mathf.FloorToInt(totalSpecialRoomsToPlace / 2f);
        int attemptsLeft;

        // Place Treasure Rooms
        attemptsLeft = MaxSpecialRoomPlacementAttempts;
        while (treasureRoomsPlaced < targetTreasureRooms && attemptsLeft > 0)
        {
            List<Vector2Int> candidates = FindCandidatePositionsForSpecialRooms();
            if (candidates.Count == 0) break;

            Vector2Int normalRoomPos = candidates[_random.Next(candidates.Count)];
            Room connectingRoom = _stageGrid.GetRoom(normalRoomPos); 
            if (connectingRoom == null) { attemptsLeft--; continue; }

            List<Direction> availableDoorsFromConnecting = connectingRoom.template.possibleDoors.Select(dc => dc.direction).ToList();
            RoomPlacementResult connectingRoomPlacementResult = new RoomPlacementResult(connectingRoom, availableDoorsFromConnecting);

            Direction requiredDoorDir; 
            Vector2Int specialRoomPos = GetNextRoomPosition(normalRoomPos, new List<Vector2Int>(), connectingRoomPlacementResult, out requiredDoorDir);

            if (specialRoomPos != InvalidPosition)
            {
                RoomPlacementResult placedResult = PlaceRoom(specialRoomPos, RoomType.Treasure, requiredDoorDir);
                if (placedResult != null && placedResult.Success && placedResult.PlacedRoom != null) 
                {
                    treasureRoomsPlaced++;
                }
                else Debug.LogWarning($"[StageGen] Failed to place Treasure room at {specialRoomPos} requiring door {requiredDoorDir} or PlacedRoom is null.");
            }
            attemptsLeft--;
        }

        // Place Shop Rooms
        attemptsLeft = MaxSpecialRoomPlacementAttempts; 
        while (shopRoomsPlaced < targetShopRooms && attemptsLeft > 0)
        {
            List<Vector2Int> candidates = FindCandidatePositionsForSpecialRooms();
            if (candidates.Count == 0) break;

            Vector2Int normalRoomPos = candidates[_random.Next(candidates.Count)];
            Room connectingRoom = _stageGrid.GetRoom(normalRoomPos); 
            if (connectingRoom == null) { attemptsLeft--; continue; }

            List<Direction> availableDoorsFromConnecting = connectingRoom.template.possibleDoors.Select(dc => dc.direction).ToList();
            RoomPlacementResult connectingRoomPlacementResult = new RoomPlacementResult(connectingRoom, availableDoorsFromConnecting);

            Direction requiredDoorDir; 
            Vector2Int specialRoomPos = GetNextRoomPosition(normalRoomPos, new List<Vector2Int>(), connectingRoomPlacementResult, out requiredDoorDir);

            if (specialRoomPos != InvalidPosition)
            {
                RoomPlacementResult placedResult = PlaceRoom(specialRoomPos, RoomType.Shop, requiredDoorDir);
                if (placedResult != null && placedResult.Success && placedResult.PlacedRoom != null)
                {
                    shopRoomsPlaced++;
                }
                else Debug.LogWarning($"[StageGen] Failed to place Shop room at {specialRoomPos} requiring door {requiredDoorDir} or PlacedRoom is null.");
            }
            attemptsLeft--;
        }
        if (treasureRoomsPlaced < targetTreasureRooms || shopRoomsPlaced < targetShopRooms)
        {
            Debug.LogWarning($"[StageGen] Could not place all special rooms. Treasure: {treasureRoomsPlaced}/{targetTreasureRooms}, Shop: {shopRoomsPlaced}/{targetShopRooms}");
        }
    }

    /// <summary>
    /// Identifies suitable grid positions adjacent to existing Normal rooms where special rooms (like Treasure or Shop) can be placed.
    /// A position is a candidate if it's valid, empty, and the adjacent Normal room has a door leading to it,
    /// and if templates exist for special rooms that can connect via that door.
    /// </summary>
    /// <returns>A list of <see cref="Vector2Int"/> representing candidate grid positions of Normal rooms that can connect to a new special room.</returns>
    private List<Vector2Int> FindCandidatePositionsForSpecialRooms()
    {
        List<Vector2Int> candidates = new();
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                Vector2Int pos = new(x, y);
                Room room = _stageGrid.GetRoom(pos);
                if (room != null && room.template != null && room.template.roomType == RoomType.Normal)
                {
                    foreach (DoorConfig doorConfig in room.template.possibleDoors)
                    {
                        Direction placementDirEnum = doorConfig.direction;
                        Vector2Int adjacentPos = pos + DirectionUtils.ToVector2Int(placementDirEnum);
                        if (_stageGrid.IsValidPosition(adjacentPos) && !_stageGrid.HasRoom(adjacentPos))
                        {
                            Direction requiredDoorOnSpecial = DirectionUtils.GetOppositeDirection(placementDirEnum);
                            if (RoomTemplateExistsForTypeWithDoor(RoomType.Treasure, requiredDoorOnSpecial) || 
                                RoomTemplateExistsForTypeWithDoor(RoomType.Shop, requiredDoorOnSpecial))
                            {
                                candidates.Add(pos); 
                                break; 
                            }
                        }
                    }
                }
            }
        }
        return candidates;
    }

    /// <summary>
    /// Checks if there is at least one room template of the specified type that has a door opening in the required direction.
    /// </summary>
    /// <param name="type">The <see cref="RoomType"/> to check for (e.g., Treasure, Shop).</param>
    /// <param name="requiredDoor">The <see cref="Direction"/> the door in the template must face.</param>
    /// <returns><c>true</c> if a suitable template exists with such a door, <c>false</c> otherwise.</returns>
    private bool RoomTemplateExistsForTypeWithDoor(RoomType type, Direction requiredDoor)
    {
        if (_roomTemplatesByType.TryGetValue(type, out List<RoomTemplate> templates))
        {
            return templates.Any(t => t.HasDoor(requiredDoor));
        }
        return false;
    }

    /// <summary>
    /// Finds a valid, unoccupied adjacent grid position to place the next room, 
    /// based on the previous room's available outgoing doors and a list of positions to exclude.
    /// It shuffles the possible placement directions to ensure randomness.
    /// </summary>
    /// <param name="currentPos">The grid position of the current room from which to find the next position.</param>
    /// <param name="excludePositions">A list of grid positions to exclude from consideration (e.g., the current path being built, to avoid loops or immediate backtracking).</param>
    /// <param name="previousRoomResult">The placement result of the previous room, which contains its available outgoing door directions.</param>
    /// <param name="requiredDoorDirectionOnNewRoom">An <c>out</c> parameter that will be set to the <see cref="Direction"/> the new room must have a door to connect back to the <paramref name="currentPos"/>.</param>
    /// <returns>The <see cref="Vector2Int"/> grid position for the next room if a valid one is found; otherwise, <see cref="InvalidPosition"/>.</returns>
    private Vector2Int GetNextRoomPosition(Vector2Int currentPos, List<Vector2Int> excludePositions, RoomPlacementResult previousRoomResult, out Direction requiredDoorDirectionOnNewRoom)
    {
        requiredDoorDirectionOnNewRoom = Direction.North; 
        if (previousRoomResult == null || !previousRoomResult.Success || previousRoomResult.AvailableOutgoingDoors == null)
        {
            Debug.LogWarning($"[StageGen] GetNextRoomPosition: previousRoomResult is invalid or has no available outgoing doors for room at {currentPos}.");
            return InvalidPosition; 
        }

        List<Direction> possiblePlacementDirections = new List<Direction>(previousRoomResult.AvailableOutgoingDoors);

        // Shuffle directions to randomize choice
        for (int i = 0; i < possiblePlacementDirections.Count; i++)
        {
            int j = _random.Next(i, possiblePlacementDirections.Count);
            (possiblePlacementDirections[i], possiblePlacementDirections[j]) = (possiblePlacementDirections[j], possiblePlacementDirections[i]);
        }

        foreach (Direction placementDirection in possiblePlacementDirections)
        {
            Vector2Int nextPos = currentPos + DirectionUtils.ToVector2Int(placementDirection);

            if (_stageGrid.IsValidPosition(nextPos) && !_stageGrid.HasRoom(nextPos) && 
                (excludePositions == null || !excludePositions.Contains(nextPos)))
            {
                requiredDoorDirectionOnNewRoom = DirectionUtils.GetOppositeDirection(placementDirection);
                // Check if a room template exists that can satisfy this required door for the *type* of room we intend to place next.
                // This check is implicitly handled by PlaceRoom, which will fail if no suitable template is found.
                return nextPos;
            }
        }
        return InvalidPosition; 
    }

    /// <summary>
    /// Places a room of a specific type at the given grid position.
    /// This overload does not require the new room to have a door in a specific direction.
    /// </summary>
    /// <param name="position">The grid position (<see cref="Vector2Int"/>) where the room should be placed.</param>
    /// <param name="type">The <see cref="RoomType"/> of the room to place (e.g., Start, Normal, Boss).</param>
    /// <returns>A <see cref="RoomPlacementResult"/> indicating the success of the placement and details of the placed room. Returns a result with Success=false if placement fails.</returns>
    private RoomPlacementResult PlaceRoom(Vector2Int position, RoomType type)
    {
        return PlaceRoom(position, type, null);
    }

    /// <summary>
    /// Places a room of a specific type at the given grid position, optionally requiring the new room to have a door in a specific direction.
    /// It selects a suitable template from the available ones for the given type and door requirement.
    /// </summary>
    /// <param name="position">The grid position (<see cref="Vector2Int"/>) where the room should be placed.</param>
    /// <param name="type">The <see cref="RoomType"/> of the room to place.</param>
    /// <param name="requiredDoorDirection">An optional <see cref="Direction"/> specifying a required door for the new room. If null, any template for the type can be chosen based on other criteria.</param>
    /// <returns>A <see cref="RoomPlacementResult"/> indicating the success of the placement and details of the placed room. Returns a result with Success=false if placement fails (e.g., no suitable template found).</returns>
    private RoomPlacementResult PlaceRoom(Vector2Int position, RoomType type, Direction? requiredDoorDirection)
    {
        List<RoomTemplate> suitableTemplates = new List<RoomTemplate>();
        if (_roomTemplatesByType.TryGetValue(type, out List<RoomTemplate> templatesForType))
        {
            suitableTemplates.AddRange(templatesForType);
        }
        else
        {
            Debug.LogError($"[StageGen] No templates list found for RoomType {type}.");
            return new RoomPlacementResult(null, new List<Direction>());
        }

        if (requiredDoorDirection.HasValue)
        {
            suitableTemplates = suitableTemplates.FindAll(t => t.HasDoor(requiredDoorDirection.Value));
            if (suitableTemplates.Count == 0)
            {
                Debug.LogWarning($"[StageGen] No {type} templates found with a door facing {requiredDoorDirection.Value} for position {position}.");
                return new RoomPlacementResult(null, new List<Direction>()); 
            }
        }
        
        if (suitableTemplates.Count == 0)
        {
            Debug.LogError($"[StageGen] No suitable templates available for RoomType {type} at {position} (after filtering for door: {requiredDoorDirection}).");
            return new RoomPlacementResult(null, new List<Direction>()); 
        }

        RoomTemplate selectedTemplate = suitableTemplates[_random.Next(suitableTemplates.Count)];
        
        RoomPlacementResult roomPlacementResult = _roomFactory.CreateRoom(
            type, 
            position, 
            selectedTemplate, 
            transform, 
            _doorPrefab, 
            _random, 
            requiredDoorDirection
        );

        if (roomPlacementResult == null || !roomPlacementResult.Success || roomPlacementResult.PlacedRoom == null) 
        {
            Debug.LogError($"[StageGen] SpecialRoomFactory failed to create {type} room with template \'{selectedTemplate?.roomName ?? "N/A"}\' at {position}, or PlacedRoom is null.");
            return roomPlacementResult ?? new RoomPlacementResult(null, new List<Direction>()); 
        }

        _stageGrid.AddRoom(position, roomPlacementResult.PlacedRoom);
        return roomPlacementResult; 
    }

    /// <summary>
    /// Delegates to the <see cref="SpecialRoomFactory"/> to connect all placed rooms in the grid.
    /// This typically involves instantiating door prefabs between adjacent rooms that have corresponding door configurations.
    /// </summary>
    private void ConnectRooms()
    {
        _roomFactory.ConnectRoomsInGrid(_stageGrid);
    }

    /// <summary>
    /// Unity callback to draw gizmos in the editor.
    /// This is used for visualizing the stage generation grid, the positions of placed rooms,
    /// and potentially other debug information related to the generation process.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_stageGrid == null) return;

        // Draw grid cells
        Gizmos.color = Color.grey;
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                Vector3 worldPos = new Vector3(x * _roomWorldSeparation, y * _roomWorldSeparation, 0);
                if (_roomFactory != null) 
                {
                    worldPos = _roomFactory.GetWorldPosition(new Vector2Int(x,y));
                }
                Gizmos.DrawWireCube(worldPos, new Vector3(_roomWorldSeparation, _roomWorldSeparation, 1f) * 0.9f);
            }
        }

        // Draw placed rooms
        Gizmos.color = Color.cyan;
        foreach (var roomKeyValuePair in _stageGrid.GetAllRooms()) // Renamed for clarity
        {
            Room roomNode = roomKeyValuePair.Value; // Get the Room object
            if (roomNode != null && roomNode.gameObject != null && _roomFactory != null)
            {
                Vector3 roomWorldPos = _roomFactory.GetWorldPosition(roomNode.gridPosition); // Access gridPosition from Room object
                Gizmos.DrawCube(roomWorldPos, new Vector3(_roomWorldSeparation, _roomWorldSeparation, 1f) * 0.5f); // Smaller cube for rooms
            }
            else if (roomNode != null && roomNode.gameObject != null) // Fallback if _roomFactory is null but room exists
            {
                 // Attempt to use room's actual transform if factory isn't available for grid to world conversion
                Gizmos.DrawCube(roomNode.gameObject.transform.position, new Vector3(_roomWorldSeparation, _roomWorldSeparation, 1f) * 0.5f);
            }
        }
    }
}
