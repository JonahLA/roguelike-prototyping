using UnityEngine;

/// <summary>
/// Controls the camera movement between rooms in the Isaac-style stage system.
/// Provides smooth transitions when the player moves between rooms.
/// </summary>
public class RoomCameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main camera to control. If not assigned, Camera.main will be used.")]
    [SerializeField] private Camera _mainCamera;
    // [SerializeField] private Transform _targetTransform; // Usually the player

    [Header("Camera Settings")]
    [Tooltip("Enable smooth camera transitions between rooms.")]
    [SerializeField] private bool _smoothTransition = true;
    [Tooltip("How quickly the camera transitions to a new room if smooth transition is enabled.")]
    [SerializeField] private float _transitionSpeed = 8f;
    [Tooltip("The orthographic size of the camera.")]
    [SerializeField] private float _cameraSize = 10f;
    [Tooltip("Offset of the camera from the center of the room (typically Z offset for 2D).")]
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 0, -10f);

    [Header("Bounds")]
    [Tooltip("Restrict camera movement to defined minimum and maximum bounds.")]
    [SerializeField] private bool _restrictToBounds = true;
    [Tooltip("Minimum X and Y coordinates the camera can move to.")]
    [SerializeField] private Vector2 _minBounds = new Vector2(-float.MaxValue, -float.MaxValue);
    [Tooltip("Maximum X and Y coordinates the camera can move to.")]
    [SerializeField] private Vector2 _maxBounds = new Vector2(float.MaxValue, float.MaxValue);

    private Vector3 _currentRoomCenter;
    private Vector3 _targetPosition;
    private bool _isTransitioning;
    private Room _currentRoom;

    private void Awake()
    {
        // Use the main camera if none specified
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera == null)
        {
            Debug.LogError("[CameraController] No camera assigned and couldn't find main camera!");
        }
        else
        {
            // Set initial camera size
            _mainCamera.orthographicSize = _cameraSize;
        }
    }

    private void LateUpdate()
    {
        // Only update camera position if we have a camera and a target room
        if (_mainCamera == null || _currentRoom == null) return;

        if (_smoothTransition && _isTransitioning)
        {
            // Smoothly move toward target position
            Vector3 newPosition = Vector3.Lerp(_mainCamera.transform.position, _targetPosition, Time.deltaTime * _transitionSpeed);
            
            // Check if we're close enough to snap
            if (Vector3.Distance(newPosition, _targetPosition) < 0.05f)
            {
                newPosition = _targetPosition;
                _isTransitioning = false;
            }
            
            _mainCamera.transform.position = newPosition;
        }
        else if (!_smoothTransition)
        {
            _mainCamera.transform.position = _targetPosition;
        }
    }

    /// <summary>
    /// Moves the camera to focus on a specific room.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> to move the camera to.</param>
    /// <remarks>
    /// If <see cref="_smoothTransition"/> is true, the camera will move smoothly to the room's center.
    /// Otherwise, it will snap instantly (though <see cref="SnapToRoom"/> is preferred for explicit snapping).
    /// Logs an error if the provided room or the main camera is null.
    /// </remarks>
    public void MoveCameraToRoom(Room room)
    {
        if (room == null || _mainCamera == null)
        {
            Debug.LogError("[CameraController] Cannot move camera - room or camera is null");
            return;
        }

        Debug.Log($"[CameraController] Moving camera to room at {room.gridPosition}");
        
        _currentRoom = room;
        _currentRoomCenter = room.transform.position;
        _targetPosition = _currentRoomCenter + _cameraOffset;
        
        if (_restrictToBounds)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minBounds.x, _maxBounds.x);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, _minBounds.y, _maxBounds.y);
        }
        
        // Start transition
        _isTransitioning = true;
    }

    /// <summary>
    /// Immediately snaps the camera to the target room's position without a smooth transition.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> to snap the camera to.</param>
    /// <remarks>
    /// This method ensures the camera is instantly positioned at the center of the specified room (plus offset).
    /// Logs an error if the provided room or the main camera is null.
    /// </remarks>
    public void SnapToRoom(Room room)
    {
        MoveCameraToRoom(room);
        _mainCamera.transform.position = _targetPosition;
        _isTransitioning = false;
    }

    /// <summary>
    /// Sets the minimum and maximum bounds for camera movement.
    /// </summary>
    /// <param name="min">The minimum X and Y coordinates for the camera.</param>
    /// <param name="max">The maximum X and Y coordinates for the camera.</param>
    /// <remarks>
    /// This is effective only if <see cref="_restrictToBounds"/> is true.
    /// </remarks>
    public void SetCameraBounds(Vector2 min, Vector2 max)
    {
        _minBounds = min;
        _maxBounds = max;
    }
}
