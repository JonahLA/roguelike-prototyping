using UnityEngine;

/// <summary>
/// Controls the camera movement between rooms in the Isaac-style stage system.
/// Provides smooth transitions when the player moves between rooms.
/// </summary>
public class RoomCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    // [SerializeField] private Transform _targetTransform; // Usually the player

    [Header("Camera Settings")]
    [SerializeField] private bool _smoothTransition = true;
    [SerializeField] private float _transitionSpeed = 8f;
    [SerializeField] private float _cameraSize = 10f;
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 0, -10f); // Usually just z offset for 2D

    [Header("Bounds")]
    [SerializeField] private bool _restrictToBounds = true;
    [SerializeField] private Vector2 _minBounds = new Vector2(-float.MaxValue, -float.MaxValue);
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
    /// Move the camera to focus on a specific room.
    /// </summary>
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
    /// Immediately snap the camera to the target position without transition.
    /// </summary>
    public void SnapToRoom(Room room)
    {
        MoveCameraToRoom(room);
        _mainCamera.transform.position = _targetPosition;
        _isTransitioning = false;
    }

    /// <summary>
    /// Set the camera bounds to restrict movement.
    /// </summary>
    public void SetCameraBounds(Vector2 min, Vector2 max)
    {
        _minBounds = min;
        _maxBounds = max;
    }
}
