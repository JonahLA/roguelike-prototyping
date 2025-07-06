using System;
using UnityEngine;
using UnityEngine.Tilemaps; // Required for Tilemap interaction

/// <summary>
/// Defines the possible states of a door.
/// </summary>
public enum DoorState
{
    /// <summary>The door is open and passable.</summary>
    Open,
    /// <summary>The door is locked and impassable until unlocked.</summary>
    Locked,
    /// <summary>The door is closed but can be opened.</summary>
    Closed,
    /// <summary>The door is hidden and inactive.</summary>
    Hidden,
    /// <summary>The door location is a solid wall, indicating no connection.</summary>
    Wall
}

/// <summary>
/// Manages the state, visuals, and interactions of a door within a room,
/// including its connection to other rooms and integration with tilemaps.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Door Configuration")]
    [Tooltip("The cardinal direction this door faces or represents.")]
    public Direction direction;
    [Tooltip("The current operational state of the door.")]
    [SerializeField] private DoorState _state = DoorState.Closed;

    [Header("References")]
    [Tooltip("The SpriteRenderer component for the door's visual representation.")]
    [SerializeField] private SpriteRenderer _doorRenderer;
    [Tooltip("The Collider2D component for the door's physical interaction.")]
    [SerializeField] private Collider2D _doorCollider;

    [Header("Visual States")]
    [Tooltip("Sprite to display when the door is open.")]
    [SerializeField] private Sprite _openSprite;
    [Tooltip("Sprite to display when the door is closed.")]
    [SerializeField] private Sprite _closedSprite;
    [Tooltip("Sprite to display when the door is locked.")]
    [SerializeField] private Sprite _lockedSprite;
    [Tooltip("Sprite to display when the door acts as a wall (fallback if tilemap integration fails or is not used).")]
    [SerializeField] private Sprite _wallSprite;

    [Header("Tilemap Integration (for Wall State)")]
    [Tooltip("The Tilemap used for displaying walls. This door will attempt to place a wall tile on this map when in the 'Wall' state.")]
    [SerializeField] private Tilemap _wallsTilemap;
    [Tooltip("The TileBase (e.g., RuleTile or specific wall tile) to use when this door becomes a wall.")]
    [SerializeField] private TileBase _wallTileAsset;
    private Vector3Int _lastWallTilePosition; // To remember where we placed a wall tile

    [Header("Connection")]
    /// <summary>
    /// Gets a value indicating whether this door is connected to another room.
    /// </summary>
    public bool isConnected { get; private set; } = false;
    private Room _parentRoom; // The room this door belongs to
    private Room _connectedRoom; // The room this door leads to
    private DoorController _connectedDoor; // The corresponding door in the connected room

    // [Header("Audio")]
    // [SerializeField] private AudioClip _openSound;
    // [SerializeField] private AudioClip _lockedSound;
    // private AudioSource _audioSource;

    /// <summary>
    /// Gets the room this door is connected to.
    /// Returns null if the door is not connected.
    /// </summary>
    public Room ConnectedRoom => _connectedRoom;

    /// <summary>
    /// Event triggered when the player enters an open and connected door.
    /// The DoorController instance that was entered is passed as an argument.
    /// </summary>
    public event Action<DoorController> OnPlayerEnterDoor;

    /// <summary>
    /// Gets or privately sets the current state of the door.
    /// Setting the state automatically updates the door's visuals and collider properties.
    /// </summary>
    public DoorState state
    {
        get => _state;
        private set
        {
            DoorState previousState = _state;
            _state = value;
            UpdateVisuals(previousState);
        }
    }

    private void Awake()
    {
        // Initialize components if they weren't assigned in the inspector
        if (_doorRenderer == null) _doorRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_doorCollider == null) _doorCollider = GetComponent<Collider2D>();

        _parentRoom = GetComponentInParent<Room>();

        if (_parentRoom != null)
        {
            // Attempt to get the WallsTilemap from the parent Room
            // This requires Room.cs to have a public Tilemap property (e.g., WallsTilemap) assigned in the Room prefab.
            _wallsTilemap = _parentRoom.WallsTilemap;
            if (_wallsTilemap == null)
            {
                Debug.LogWarning($"[DoorController] ({gameObject.name}) Could not retrieve WallsTilemap from parent Room \'{_parentRoom.name}\' or it was not assigned. Ensure Room script exists and its WallsTilemap property is set. Wall state will not integrate with Rule Tiles.", this);
            }
        }
        else
        {
            Debug.LogError($"[DoorController] ({gameObject.name}) Parent Room component not found. Cannot initialize WallsTilemap. Door will not function correctly with Tilemap wall states.", this);
        }

        // _audioSource = GetComponent<AudioSource>(); // Uncomment if using audio

        UpdateVisuals(_state); // Pass current state as previous for initial setup
    }

    /// <summary>
    /// Updates the door's sprite and collider based on its current state.
    /// </summary>
    /// <param name="previousState">The state of the door before the current update.</param>
    private void UpdateVisuals(DoorState previousState)
    {
        if (_doorRenderer == null || _doorCollider == null) return;

        // Clear any previously placed wall tile if state is changing FROM Wall
        if (previousState == DoorState.Wall && _state != DoorState.Wall && _wallsTilemap != null && _wallTileAsset != null)
        {
            _wallsTilemap.SetTile(_lastWallTilePosition, null);
        }

        // Default visual and collider states
        _doorRenderer.enabled = true;
        _doorCollider.enabled = true;
        _doorCollider.isTrigger = false; // Default to solid

        switch (_state)
        {
            case DoorState.Open:
                _doorRenderer.sprite = _openSprite;
                _doorCollider.isTrigger = true; // Allow passage and trigger events
                break;

            case DoorState.Locked:
                _doorRenderer.sprite = _lockedSprite;
                // _doorCollider.isTrigger remains false (solid)
                break;

            case DoorState.Closed:
                _doorRenderer.sprite = _closedSprite;
                // _doorCollider.isTrigger remains false (solid)
                break;

            case DoorState.Hidden:
                _doorRenderer.enabled = false;
                _doorCollider.enabled = false; // No visual, no interaction
                break;

            case DoorState.Wall:
                if (_wallsTilemap != null && _wallTileAsset != null)
                {
                    _lastWallTilePosition = _wallsTilemap.WorldToCell(transform.position);
                    _wallsTilemap.SetTile(_lastWallTilePosition, _wallTileAsset);
                    _doorRenderer.enabled = false; // Hide door's own sprite
                    _doorCollider.enabled = false; // Let the tilemap tile handle collision
                }
                else
                {
                    // Fallback to sprite if tilemap/tile asset isn't set up
                    _doorRenderer.sprite = _wallSprite;
                    // _doorRenderer.enabled is true (set by default)
                    // _doorCollider.enabled is true (set by default)
                    // _doorCollider.isTrigger remains false (solid wall)
                    if (_wallsTilemap == null) Debug.LogWarning("[DoorController] Wall state active but no Walls Tilemap assigned. Using fallback sprite.", this);
                    if (_wallTileAsset == null) Debug.LogWarning("[DoorController] Wall state active but no Wall Tile Asset assigned. Using fallback sprite.", this);
                }
                break;
        }
    }

    /// <summary>
    /// Sets the door to a new state.
    /// </summary>
    /// <param name="newState">The desired new state for the door.</param>
    public void SetState(DoorState newState)
    {
        state = newState;
    }

    /// <summary>
    /// Opens the door if it is not locked.
    /// If connected to another door, it attempts to open that door as well.
    /// </summary>
    public void Open()
    {
        if (state == DoorState.Wall) return;

        if (state != DoorState.Locked)
        {
            SetState(DoorState.Open);
            // if (_audioSource != null && _openSound != null)
            //     _audioSource.PlayOneShot(_openSound);

            // If there's a connected door, open it too for consistency
            if (_connectedDoor != null && _connectedDoor.state != DoorState.Open)
                _connectedDoor.Open();
        }
    }

    /// <summary>
    /// Closes the door.
    /// </summary>
    public void Close()
    {
        if (state == DoorState.Wall) return;
        SetState(DoorState.Closed);
    }

    /// <summary>
    /// Locks the door, making it impassable.
    /// </summary>
    public void Lock()
    {
        if (state == DoorState.Wall) return;
        SetState(DoorState.Locked);
    }

    /// <summary>
    /// Unlocks the door, changing its state to Closed.
    /// It can then be opened.
    /// </summary>
    public void Unlock()
    {
        if (state == DoorState.Wall) return;
        SetState(DoorState.Closed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DoorController] OnTriggerEnter2D triggered by: {other.name} with tag: {other.tag}");
        Debug.Log($"[DoorController] Door state: {state}, isConnected: {isConnected}");

        // Check if the player entered the door trigger when it's open
        if (other.CompareTag("Player"))
        {
            Debug.Log("[DoorController] Collider is Player.");
            if (state == DoorState.Open)
            {
                Debug.Log("[DoorController] Door is Open.");
                if (isConnected)
                {
                    Debug.Log("[DoorController] Door is Connected. Invoking OnPlayerEnterDoor.");
                    // Trigger the room transition event
                    OnPlayerEnterDoor?.Invoke(this);
                }
                else
                {
                    Debug.LogWarning("[DoorController] Door is Open but not connected to another room.");
                }
            }
            else if (state == DoorState.Locked)
            {
                Debug.Log("[DoorController] Player collided with a Locked door.");
                // Play locked sound when player tries to enter a locked door
                // if (_audioSource != null && _lockedSound != null)
                //     _audioSource.PlayOneShot(_lockedSound);
            }
            else
            {
                Debug.Log($"[DoorController] Player collided with door in state: {state}");
            }
        }
        else
        {
            Debug.Log($"[DoorController] Collision with non-player object: {other.name}");
        }
    }

    /// <summary>
    /// Connects this door to a target room and, optionally, a specific door in that room.
    /// Updates the door's state based on whether a connection is successfully established.
    /// If a targetDoor is provided, it will attempt to establish a reciprocal connection.
    /// </summary>
    /// <param name="room">The room to connect to. If null, the door is considered unconnected and becomes a wall.</param>
    /// <param name="targetDoor">The specific door in the target room to connect to. Can be null.</param>
    public void ConnectTo(Room room, DoorController targetDoor = null)
    {
        DoorState previousState = _state; // Capture state before ConnectTo potentially changes it

        _connectedRoom = room;
        isConnected = room != null;
        _connectedDoor = targetDoor;

        if (isConnected)
        {
            SetState(DoorState.Closed);

            if (targetDoor != null && targetDoor._connectedRoom != _parentRoom)
            {
                targetDoor.ConnectTo(_parentRoom, this);
            }
        }
        else
        {
            SetState(DoorState.Wall);
        }
    }

    /// <summary>
    /// Gets the opposite cardinal direction.
    /// </summary>
    /// <param name="dir">The input direction.</param>
    /// <returns>The opposite direction (e.g., North returns South).</returns>
    /// <remarks>
    /// If an undefined direction value is passed (which shouldn\'t happen with standard enum usage),
    /// this method currently returns the input direction.
    /// </remarks>
    public static Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => dir  // should not happen with cardinal directions
        };
    }
}