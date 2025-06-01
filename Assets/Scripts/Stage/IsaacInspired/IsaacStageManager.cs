using UnityEngine;
/// <summary>
/// Manages the overall state and flow of an Isaac-style stage.
/// Responsibilities include initiating stage generation, handling player transitions between rooms,
/// and coordinating updates to other systems like the minimap and camera.
/// </summary>
public class IsaacStageManager : MonoBehaviour
{
    [Tooltip("Reference to the IsaacStageGenerator responsible for creating the stage layout.")]
    [SerializeField] private IsaacStageGenerator _stageGenerator;
    [Tooltip("Reference to the player's Transform component for positioning.")]
    [SerializeField] private Transform _playerTransform;
    [Tooltip("Reference to the MinimapController for updating the minimap display.")]
    [SerializeField] private MinimapController _minimapController;
    [Tooltip("Reference to the RoomCameraController for controlling camera movements between rooms.")]
    [SerializeField] private RoomCameraController _cameraController;
    
    private StageGrid _currentStage;
    private Room _currentRoom;

    /// <summary>
    /// Event triggered when the player moves from one room to another.
    /// Provides the previous room and the new current room as arguments.
    /// </summary>
    /// <remarks>Subscribers should ensure they unsubscribe, typically in OnDisable or OnDestroy, if their lifecycle is shorter than the IsaacStageManager.</remarks>
    public event System.Action<Room, Room> OnPlayerChangedRoom; // previousRoom, newRoom

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Used here to generate the initial stage.
    /// </summary>
    public void Start()
    {
        GenerateNewStage();
    }
    
    /// <summary>
    /// Initiates the generation of a new stage. This involves clearing any existing stage data,
    /// invoking the stage generator, setting up room connections and events, placing the player
    /// in the start room, and initializing the minimap.
    /// </summary>
    public void GenerateNewStage()
    {
        // Clear any existing stage first
        ClearCurrentStage();
        
        if (_stageGenerator == null)
        {
            Debug.LogError("[StageManager] StageGenerator is not assigned. Cannot generate stage.");
            return;
        }
        _currentStage = _stageGenerator.GenerateStage();
        
        if (_currentStage == null || _currentStage.RoomCount == 0) // Added check for RoomCount
        {
            Debug.LogError("[StageManager] Failed to generate stage or stage is empty!");
            return;
        }

        // Subscribe to door events for room transitions
        for (int x = 0; x < _currentStage.GridSize.x; x++)
        {
            for (int y = 0; y < _currentStage.GridSize.y; y++)
            {
                var room = _currentStage.GetRoom(new Vector2Int(x, y));
                if (room == null) continue;
                foreach (var door in room.doors.Values)
                {
                    if (door != null)
                    {
                        door.OnPlayerEnterDoor += OnPlayerEnteredDoor;
                    }
                }
            }
        }

        Debug.Log($"[StageManager] Stage generated with {_currentStage.RoomCount} rooms");
        
        Vector2Int startPos = _currentStage.GetStartRoomPosition();
        if (startPos == new Vector2Int(-1,-1)) // Check if start position is valid
        {
            Debug.LogError("[StageManager] No valid start room position found in the generated stage.");
            return;
        }
        _currentRoom = _currentStage.GetRoom(startPos);
        
        if (_currentRoom == null)
        {
            Debug.LogError("[StageManager] Start room not found at position " + startPos);
            return;
        }
        
        if (_playerTransform != null)
        {
            _playerTransform.position = _currentRoom.transform.position;
            _currentRoom.OnPlayerEnter();
            
            if (_cameraController != null)
            {
                _cameraController.SnapToRoom(_currentRoom);
            }
            else
            {
                Debug.LogWarning("[StageManager] CameraController is not assigned. Camera will not snap to start room.");
            }
        }
        else
        {
            Debug.LogWarning("[StageManager] PlayerTransform is not assigned. Player will not be positioned in the start room.");
        }
        
        InitializeMiniMap();
    }

    /// <summary>
    /// Clears the currently active stage by destroying all room GameObjects and unsubscribing from door events.
    /// This prepares the manager for generating a new stage.
    /// </summary>
    private void ClearCurrentStage()
    {
        if (_currentStage != null)
        {
            for (int x = 0; x < _currentStage.GridSize.x; x++)
            {
                for (int y = 0; y < _currentStage.GridSize.y; y++)
                {
                    Room room = _currentStage.GetRoom(new Vector2Int(x, y));
                    if (room != null)
                    {
                        // Unsubscribe from door events before destroying the room
                        foreach (var door in room.doors.Values)
                        {
                            if (door != null)
                            {
                                door.OnPlayerEnterDoor -= OnPlayerEnteredDoor;
                            }
                        }
                        Destroy(room.gameObject);
                    }
                }
            }
            _currentStage = null; // Ensure the reference is cleared
            _currentRoom = null;  // Clear current room reference as well
        }
    }

    /// <summary>
    /// Initializes the minimap with the data from the current stage.
    /// Logs an error if the minimap controller or current stage is not available.
    /// </summary>
    private void InitializeMiniMap()
    {
        if (_minimapController != null && _currentStage != null)
            _minimapController.InitializeMinimap(_currentStage);
        else
            Debug.LogError("[IsaacStageManager] MinimapController reference is not set or CurrentStage is null. Cannot initialize minimap.");
    }
    
    /// <summary>
    /// Handles the event triggered when a player enters a door.
    /// This method manages the transition to the connected room, including updating player position,
    /// notifying rooms of player entry/exit, moving the camera, and invoking the OnPlayerChangedRoom event.
    /// </summary>
    /// <param name="door">The <see cref="DoorController"/> instance of the door that was entered.</param>
    public void OnPlayerEnteredDoor(DoorController door)
    {
        if (door == null)
        {
            Debug.LogError("[StageManager] OnPlayerEnteredDoor called with a null door.");
            return;
        }

        if (door.isConnected && _playerTransform != null)
        {
            Room previousRoom = _currentRoom;
            Room targetRoom = door.connectedRoom;
            
            if (targetRoom != null)
            {
                if (previousRoom != null)
                {
                    previousRoom.OnPlayerExit();
                }
                else
                {
                    Debug.LogWarning("[StageManager] previousRoom was null during room transition. This might happen if player enters a door without being in a valid room prior.");
                }
                
                _currentRoom = targetRoom;
                
                Direction entryDirection = DirectionUtils.GetOppositeDirection(door.direction); // Changed from DoorController.GetOppositeDirection for consistency if DirectionUtils is the primary source
                if (targetRoom.doors.TryGetValue(entryDirection, out DoorController entryDoor) && entryDoor != null)
                {
                    // Position player just inside the entry door
                    Vector3 doorOffset = entryDirection switch
                    {
                        Direction.North => new Vector3(0, -1.5f, 0),
                        Direction.East => new Vector3(-1.5f, 0, 0),
                        Direction.South => new Vector3(0, 1.5f, 0),
                        Direction.West => new Vector3(1.5f, 0, 0),
                        _ => Vector3.zero
                    };
                    _playerTransform.position = entryDoor.transform.position + doorOffset;
                }
                else
                {
                    // Fallback or error: if the expected entry door isn't found, position player at room center
                    Debug.LogWarning($"[StageManager] Entry door not found in {targetRoom.name} for direction {entryDirection}. Player placed at room center.");
                    _playerTransform.position = targetRoom.transform.position;
                }
                
                // Trigger the OnPlayerEnter event
                targetRoom.OnPlayerEnter();
                
                // Move camera to the new room
                if (_cameraController != null)
                {
                    _cameraController.MoveCameraToRoom(targetRoom);
                }
                
                // Notify minimap controller (and other listeners)
                OnPlayerChangedRoom?.Invoke(previousRoom, targetRoom);
            }
        }
    }

    /// <summary>
    /// Called by Unity when the MonoBehaviour will be destroyed.
    /// Ensures that event subscriptions are cleaned up to prevent memory leaks or errors.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from all door events when the StageManager itself is destroyed
        // This is a fallback/safety, primary unsubscription should happen in ClearCurrentStage
        if (_currentStage != null)
        {
            for (int x = 0; x < _currentStage.GridSize.x; x++)
            {
                for (int y = 0; y < _currentStage.GridSize.y; y++)
                {
                    Room room = _currentStage.GetRoom(new Vector2Int(x, y));
                    if (room == null) continue;
                    foreach (var door in room.doors.Values)
                    {
                        if (door != null)
                        {
                            door.OnPlayerEnterDoor -= OnPlayerEnteredDoor;
                        }
                    }
                }
            }
        }
    }
}