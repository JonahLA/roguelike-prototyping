using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for LINQ

public class IsaacStageGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private Vector2Int _gridSize = new(8, 8);
    [SerializeField] private int _minMainPathLength = 3;
    [SerializeField] private int _maxMainPathLength = 7;
    [SerializeField] private int _specialRoomsCount = 2;
    [SerializeField] private float _branchProbability = 0.5f;
    [SerializeField] private int _seed = 0;
    [SerializeField] private bool _useRandomSeed = true;

    [Header("Room Placement")] // New Header
    [SerializeField] private float _roomWorldSeparation = 50f; // New field for world separation

    [Header("Room Templates")]
    [SerializeField] private List<RoomTemplate> _startRoomTemplates = new();
    [SerializeField] private List<RoomTemplate> _normalRoomTemplates = new();
    [SerializeField] private List<RoomTemplate> _bossRoomTemplates = new();
    [SerializeField] private List<RoomTemplate> _treasureRoomTemplates = new();
    [SerializeField] private List<RoomTemplate> _shopRoomTemplates = new();

    [Header("Prefabs")]
    [SerializeField] private GameObject _doorPrefab;

    private StageGrid _stageGrid;
    private System.Random _random;
    private Dictionary<RoomType, List<RoomTemplate>> _roomTemplatesByType = new();
    private SpecialRoomFactory _roomFactory;
    private RoomContentPopulator _roomContentPopulator;

    // Added for branch difficulty calculation consistency
    private const float BranchDifficultyIncrement = 0.1f; 

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
        
        if (startRoomResult.PlacedRoom.gameObject != null)
        {
            _roomContentPopulator.PopulateRoom(startRoomResult.PlacedRoom.gameObject, startRoomResult.PlacedRoom.template, 0f); // Difficulty 0 for start room
        }
        else
        {
            Debug.LogWarning($"[StageGen] Start room '{startRoomResult.PlacedRoom.template.roomName}' GameObject is null after placement. Cannot populate.");
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

    private List<Vector2Int> GenerateMainPath(Vector2Int startPos, int length, RoomPlacementResult initialPreviousRoomResult)
    {
        List<Vector2Int> path = new() { startPos };
        Vector2Int currentPos = startPos;
        RoomPlacementResult previousRoomResult = initialPreviousRoomResult;

        for (int i = 0; i < length; i++)
        {
            Direction requiredDoorDirectionOnNewRoom;
            Vector2Int nextPos = GetNextRoomPosition(currentPos, path, previousRoomResult, out requiredDoorDirectionOnNewRoom);
            
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

            if (newRoomResult.PlacedRoom.gameObject != null)
            {
                float difficulty = Mathf.Clamp01((float)path.Count / _maxMainPathLength); 
                _roomContentPopulator.PopulateRoom(newRoomResult.PlacedRoom.gameObject, newRoomResult.PlacedRoom.template, difficulty);
            }
            else
            {
                Debug.LogWarning($"[StageGen] Normal room '{newRoomResult.PlacedRoom.template.roomName}' GameObject is null after placement in MainPath. Cannot populate.");
            }
        }

        if (previousRoomResult != null && previousRoomResult.Success && previousRoomResult.PlacedRoom != null &&
            (previousRoomResult.PlacedRoom.template.roomType != RoomType.Start || (previousRoomResult.PlacedRoom.template.roomType == RoomType.Start && path.Count > 1)))
        {
            Direction bossRoomRequiredDoorDir;
            Vector2Int bossRoomPos = GetNextRoomPosition(currentPos, path, previousRoomResult, out bossRoomRequiredDoorDir);

            if (bossRoomPos != Vector2Int.one * -1)
            {
                RoomPlacementResult bossRoomResult = PlaceRoom(bossRoomPos, RoomType.Boss, bossRoomRequiredDoorDir);
                if (bossRoomResult != null && bossRoomResult.Success && bossRoomResult.PlacedRoom != null)
                {
                    path.Add(bossRoomPos);
                    Debug.Log($"[StageGen] Boss room placed at {bossRoomPos} connected to {currentPos}");

                    if (bossRoomResult.PlacedRoom.gameObject != null)
                    {
                        _roomContentPopulator.PopulateRoom(bossRoomResult.PlacedRoom.gameObject, bossRoomResult.PlacedRoom.template, 1.0f); // Max difficulty for boss
                    }
                    else
                    {
                        Debug.LogWarning($"[StageGen] Boss room '{bossRoomResult.PlacedRoom.template.roomName}' GameObject is null after placement. Cannot populate.");
                    }
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
                    // Corrected call to CreateBranch, passing the necessary parentRoomInfo and parentDifficulty
                    CreateBranch(branchParentGridPos, parentRoomInfo, branchLength, parentDifficulty);
                }
                else
                {
                    Debug.LogWarning($"[StageGen] Branch parent room at {branchParentGridPos} is null. Skipping branch.");
                }
            }
        }
    }

    // Corrected CreateBranch method signature and logic
    private void CreateBranch(Vector2Int parentInitialPos, RoomPlacementResult parentInitialRoomResult, int length, float initialParentDifficulty)
    {
        List<Vector2Int> currentBranchPath = new List<Vector2Int>(); 
        currentBranchPath.Add(parentInitialPos); 

        Vector2Int currentPosInBranch = parentInitialPos;
        RoomPlacementResult previousRoomResultInBranch = parentInitialRoomResult;

        for (int j = 0; j < length; j++) 
        {
            Direction requiredDoorOnNewRoom;
            Vector2Int nextPos = GetNextRoomPosition(currentPosInBranch, currentBranchPath, previousRoomResultInBranch, out requiredDoorOnNewRoom);

            if (nextPos == Vector2Int.one * -1)
            {
                Debug.LogWarning($"[StageGen] CreateBranch: No valid position found from {currentPosInBranch} for branch room {j+1}/{length}. Branch may be shorter.");
                break; 
            }

            RoomPlacementResult branchRoomPlacement = PlaceRoom(nextPos, RoomType.Normal, requiredDoorOnNewRoom);

            if (branchRoomPlacement != null && branchRoomPlacement.Success && branchRoomPlacement.PlacedRoom != null)
            {
                if (_roomContentPopulator != null && branchRoomPlacement.PlacedRoom.gameObject != null)
                {
                    // Difficulty for branch rooms increases from the parent's difficulty
                    float branchRoomDifficulty = Mathf.Clamp(initialParentDifficulty + (j + 1) * BranchDifficultyIncrement, 0f, 1f);
                    _roomContentPopulator.PopulateRoom(branchRoomPlacement.PlacedRoom.gameObject, branchRoomPlacement.PlacedRoom.template, branchRoomDifficulty);
                }
                else
                {
                     Debug.LogWarning($"[StageGen] Branch room '{branchRoomPlacement.PlacedRoom?.template?.roomName ?? "N/A"}' GameObject is null or populator is null. Cannot populate.");
                }

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

    private void PlaceSpecialRooms()
    {
        int maxAttempts = 20;
        int treasureRoomsPlaced = 0;
        int shopRoomsPlaced = 0;
        int totalSpecialRoomsToPlace = _specialRoomsCount;
        int targetTreasureRooms = Mathf.CeilToInt(totalSpecialRoomsToPlace / 2f);
        int targetShopRooms = Mathf.FloorToInt(totalSpecialRoomsToPlace / 2f);

        // Place Treasure Rooms
        while (treasureRoomsPlaced < targetTreasureRooms && maxAttempts > 0)
        {
            List<Vector2Int> candidates = FindCandidatePositionsForSpecialRooms();
            if (candidates.Count == 0) break;

            Vector2Int normalRoomPos = candidates[_random.Next(candidates.Count)];
            Room connectingRoom = _stageGrid.GetRoom(normalRoomPos); 
            if (connectingRoom == null) { maxAttempts--; continue; }

            List<Direction> availableDoorsFromConnecting = connectingRoom.template.possibleDoors.Select(dc => dc.direction).ToList();
            RoomPlacementResult connectingRoomPlacementResult = new RoomPlacementResult(connectingRoom, availableDoorsFromConnecting);

            Direction requiredDoorDir; 
            Vector2Int specialRoomPos = GetNextRoomPosition(normalRoomPos, new List<Vector2Int>(), connectingRoomPlacementResult, out requiredDoorDir);

            if (specialRoomPos != Vector2Int.one * -1)
            {
                RoomPlacementResult placedResult = PlaceRoom(specialRoomPos, RoomType.Treasure, requiredDoorDir);
                if (placedResult != null && placedResult.Success && placedResult.PlacedRoom != null) 
                {
                    treasureRoomsPlaced++;
                    if (placedResult.PlacedRoom.gameObject != null)
                    {
                        // Fixed low difficulty for Treasure rooms
                        _roomContentPopulator.PopulateRoom(placedResult.PlacedRoom.gameObject, placedResult.PlacedRoom.template, 0.1f);
                    }
                    else
                    {
                        Debug.LogWarning($"[StageGen] Treasure room '{placedResult.PlacedRoom.template.roomName}' GameObject is null after placement. Cannot populate.");
                    }
                }
                else Debug.LogWarning($"[StageGen] Failed to place Treasure room at {specialRoomPos} requiring door {requiredDoorDir} or PlacedRoom is null.");
            }
            maxAttempts--;
        }

        // Place Shop Rooms
        maxAttempts = 20; 
        while (shopRoomsPlaced < targetShopRooms && maxAttempts > 0)
        {
            List<Vector2Int> candidates = FindCandidatePositionsForSpecialRooms();
            if (candidates.Count == 0) break;

            Vector2Int normalRoomPos = candidates[_random.Next(candidates.Count)];
            Room connectingRoom = _stageGrid.GetRoom(normalRoomPos); 
            if (connectingRoom == null) { maxAttempts--; continue; }

            List<Direction> availableDoorsFromConnecting = connectingRoom.template.possibleDoors.Select(dc => dc.direction).ToList();
            RoomPlacementResult connectingRoomPlacementResult = new RoomPlacementResult(connectingRoom, availableDoorsFromConnecting);

            Direction requiredDoorDir; 
            Vector2Int specialRoomPos = GetNextRoomPosition(normalRoomPos, new List<Vector2Int>(), connectingRoomPlacementResult, out requiredDoorDir);

            if (specialRoomPos != Vector2Int.one * -1)
            {
                RoomPlacementResult placedResult = PlaceRoom(specialRoomPos, RoomType.Shop, requiredDoorDir);
                if (placedResult != null && placedResult.Success && placedResult.PlacedRoom != null)
                {
                     shopRoomsPlaced++;
                    if (placedResult.PlacedRoom.gameObject != null)
                    {
                        // Fixed zero difficulty for Shop rooms
                        _roomContentPopulator.PopulateRoom(placedResult.PlacedRoom.gameObject, placedResult.PlacedRoom.template, 0f);
                    }
                    else
                    {
                        Debug.LogWarning($"[StageGen] Shop room '{placedResult.PlacedRoom.template.roomName}' GameObject is null after placement. Cannot populate.");
                    }
                }
                else Debug.LogWarning($"[StageGen] Failed to place Shop room at {specialRoomPos} requiring door {requiredDoorDir} or PlacedRoom is null.");
            }
            maxAttempts--;
        }
        if (treasureRoomsPlaced < targetTreasureRooms || shopRoomsPlaced < targetShopRooms)
        {
            Debug.LogWarning($"[StageGen] Could not place all special rooms. Treasure: {treasureRoomsPlaced}/{targetTreasureRooms}, Shop: {shopRoomsPlaced}/{targetShopRooms}");
        }
    }

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

    private bool RoomTemplateExistsForTypeWithDoor(RoomType type, Direction requiredDoor)
    {
        if (_roomTemplatesByType.TryGetValue(type, out List<RoomTemplate> templates))
        {
            return templates.Any(t => t.HasDoor(requiredDoor));
        }
        return false;
    }

    private Vector2Int GetNextRoomPosition(Vector2Int currentPos, List<Vector2Int> excludePositions, RoomPlacementResult previousRoomResult, out Direction requiredDoorDirectionOnNewRoom)
    {
        requiredDoorDirectionOnNewRoom = Direction.North; 
        if (previousRoomResult == null || !previousRoomResult.Success || previousRoomResult.AvailableOutgoingDoors == null)
        {
            Debug.LogWarning($"[StageGen] GetNextRoomPosition: previousRoomResult is invalid or has no available outgoing doors for room at {currentPos}.");
            return Vector2Int.one * -1; 
        }

        List<Direction> possiblePlacementDirections = new List<Direction>(previousRoomResult.AvailableOutgoingDoors);

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
                return nextPos;
            }
        }
        return Vector2Int.one * -1; 
    }

    private RoomPlacementResult PlaceRoom(Vector2Int position, RoomType type)
    {
        return PlaceRoom(position, type, null);
    }

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
        // Population is handled by the calling methods (GenerateMainPath, CreateBranch, PlaceSpecialRooms)
        return roomPlacementResult; 
    }

    private void ConnectRooms()
    {
        // Corrected call to ConnectRoomsInGrid
        _roomFactory.ConnectRoomsInGrid(_stageGrid);
    }

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
