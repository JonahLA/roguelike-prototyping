using UnityEngine;

public class IsaacStageManager : MonoBehaviour
{
    [SerializeField] private IsaacStageGenerator _stageGenerator;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private MinimapController _minimapController;
    [SerializeField] private RoomCameraController _cameraController;
    
    private StageGrid _currentStage;
    private Room _currentRoom;

    public event System.Action<Room, Room> OnPlayerChangedRoom; // previousRoom, newRoom

    public void Start()
    {
        GenerateNewStage();
    }
    
    public void GenerateNewStage()
    {
        // Clear any existing stage first
        ClearCurrentStage();
        
        // Generate new stage
        _currentStage = _stageGenerator.GenerateStage();
        
        if (_currentStage == null)
        {
            Debug.LogError("[StageManager] Failed to generate stage!");
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
                        door.OnPlayerEnterDoor += OnPlayerEnteredDoor;
                }
            }
        }

        Debug.Log($"[StageManager] Stage generated with {_currentStage.RoomCount} rooms");
        
        // Place player at starting room
        Vector2Int startPos = _currentStage.GetStartRoomPosition();
        
        _currentRoom = _currentStage.GetRoom(startPos);
        
        if (_currentRoom == null)
        {
            Debug.LogError("[StageManager] Start room not found at position " + startPos);
            return;
        }
        
        if (_playerTransform != null && _currentRoom != null)
        {
            _playerTransform.position = _currentRoom.transform.position;
            _currentRoom.OnPlayerEnter();
            
            // Move camera to the start room
            if (_cameraController != null)
            {
                _cameraController.SnapToRoom(_currentRoom);
            }
            
        }
        
        // Initialize minimap
        InitializeMiniMap();
    }

    private void ClearCurrentStage()
    {
        // Clean up old stage if it exists
        if (_currentStage != null)
        {
            for (int x = 0; x < _currentStage.GridSize.x; x++)
            {
                for (int y = 0; y < _currentStage.GridSize.y; y++)
                {
                    Room room = _currentStage.GetRoom(new Vector2Int(x, y));
                    if (room != null)
                    {
                        Destroy(room.gameObject);
                    }
                }
            }
        }
    }

    private void InitializeMiniMap()
    {
        if (_minimapController != null && _currentStage != null)
            _minimapController.InitializeMinimap(_currentStage);
        else
            Debug.LogError("[IsaacStageManager] MinimapController reference is not set or CurrentStage is null. Cannot initialize minimap.");
    }
    
    // This method would be called by the OnPlayerEnterDoor event from DoorController
    public void OnPlayerEnteredDoor(DoorController door)
    {
        if (door.isConnected && _playerTransform != null)
        {
            // Get the connected room
            Room previousRoom = _currentRoom;
            Room targetRoom = door.connectedRoom;
            
            if (targetRoom != null)
            {
                // Notify rooms
                previousRoom.OnPlayerExit();
                _currentRoom = targetRoom;
                
                // Find the entry door in the target room
                Direction entryDirection = DoorController.GetOppositeDirection(door.direction);
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
}