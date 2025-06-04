using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the minimap UI, including both the standard (3x3) view and the full map overlay.
/// Handles room discovery, player position updates, and toggling between minimap views.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [Header("UI Prefabs & References")]
    [Tooltip("The prefab for a single room UI element on the minimap.")]
    [SerializeField] private GameObject minimapRoomPrefab;
    
    [Tooltip("The parent transform for all room UI elements in the full map view.")]
    [SerializeField] private Transform fullMapRoomsContainer;
    
    [Tooltip("The Image component used to display the player's icon on the minimap.")]
    [SerializeField] private Image playerIcon;
    
    [Tooltip("The Panel Image component for the standard minimap's background.")]
    [SerializeField] private Image standardMinimapBackground;
    
    [Tooltip("Optional: The Image component for the border of the standard minimap.")]
    [SerializeField] private Image standardMinimapBorder;
    
    [Tooltip("The parent GameObject that holds the 3x3 grid of rooms for the standard minimap view.")]
    [SerializeField] private GameObject standardMinimapContainer;
    
    [Tooltip("The parent GameObject for the full map overlay, shown when the player toggles the map.")]
    [SerializeField] private GameObject fullMapOverlayContainer;

    [Header("Minimap Configuration")]
    [Tooltip("Default color for unexplored rooms in the full map view.")]
    [SerializeField] private Color defaultRoomColor = Color.gray;
    
    [Tooltip("Color for rooms that have been explored by the player.")]
    [SerializeField] private Color exploredRoomColor = Color.white;
    
    [Tooltip("Color for the room the player is currently in.")]
    [SerializeField] private Color currentRoomColor = Color.green;
    
    [Tooltip("Background color for the standard minimap.")]
    [SerializeField] private Color minimapBackgroundColor = Color.black;
    
    [Tooltip("Border color for the standard minimap.")]
    [SerializeField] private Color minimapBorderColor = Color.white;
    
    [Tooltip("ScriptableObject asset that maps RoomType enums to their corresponding UI sprites.")]
    [SerializeField] private MinimapIconMapping iconMapping;

    [Header("Room Layout Settings")]
    [Tooltip("The size (width and height) of each individual room UI element on the minimap.")]
    [SerializeField] private Vector2 roomSize = new(30, 30);
    
    [Tooltip("The spacing (horizontal and vertical) between room UI elements on the minimap.")]
    [SerializeField] private Vector2 roomSpacing = new(5, 5);

    /// <summary>
    /// Dictionary mapping grid coordinates to their corresponding minimap UI room representations.
    /// </summary>
    private Dictionary<Vector2Int, MinimapUIRoom> minimapRooms = new();
    
    /// <summary>
    /// Reference to the current stage's grid data structure.
    /// </summary>
    private StageGrid currentStageGrid;
    
    /// <summary>
    /// The grid coordinates of the room the player is currently in.
    /// </summary>
    private Vector2Int currentPlayerRoomCoordinates = Vector2Int.zero;
    
    /// <summary>
    /// Whether the full map overlay is currently active/visible.
    /// </summary>
    private bool isFullMapActive = false;

    /// <summary>
    /// Size of the standard minimap view (3x3 grid).
    /// </summary>
    private const int StandardViewSize = 3;
    
    /// <summary>
    /// Array representing the 3x3 standard minimap view room slots.
    /// </summary>
    private MinimapUIRoom[,] standardViewRooms = new MinimapUIRoom[StandardViewSize, StandardViewSize];
    
    /// <summary>
    /// Dictionary tracking all rooms that have been explored by the player.
    /// </summary>
    private Dictionary<Vector2Int, Room> exploredRooms = new();

    /// <summary>
    /// Unity lifecycle method called before Start().
    /// Initializes the standard view slots for the 3x3 minimap.
    /// </summary>
    void Awake()
    {
        InitializeStandardViewSlots();
    }

    /// <summary>
    /// Unity lifecycle method called at the start of the first frame.
    /// Sets up initial minimap visual properties and container states.
    /// </summary>
    void Start()
    {
        if (standardMinimapBackground != null)
        {
            standardMinimapBackground.color = minimapBackgroundColor;
        }
        if (standardMinimapBorder != null)
        {
            standardMinimapBorder.color = minimapBorderColor;
        }

        if (playerIcon != null)
        {
            playerIcon.gameObject.SetActive(false); // Hide until map is initialized
        }

        // It's good practice to ensure containers are active/inactive as expected initially
        // StandardMinimapContainer should be active for InitializeStandardViewSlots in Awake to correctly setup UI.
        if (standardMinimapContainer != null) standardMinimapContainer.SetActive(true);
        if (fullMapOverlayContainer != null) fullMapOverlayContainer.SetActive(false);
    }

    /// <summary>
    /// Initializes the 3x3 grid of UI room slots for the standard minimap view.
    /// These slots are pre-instantiated and then populated with room data as the player moves.
    /// </summary>
    private void InitializeStandardViewSlots()
    {
        if (minimapRoomPrefab == null || standardMinimapContainer == null)
        {
            Debug.LogError("[MinimapController] MinimapRoom Prefab or StandardMinimapContainer not assigned!");
            return;
        }

        // Create the 3x3 grid for the standard minimap view
        // These will be placeholders, updated when the player moves
        for (int y = 0; y < StandardViewSize; y++)
        {
            for (int x = 0; x < StandardViewSize; x++)
            {
                GameObject roomGO = Instantiate(minimapRoomPrefab, standardMinimapContainer.transform);
                MinimapUIRoom uiRoom = roomGO.GetComponent<MinimapUIRoom>();
                if (uiRoom != null)
                {
                    roomGO.name = $"StandardMinimapRoom_{x}_{y}";
                    RectTransform rt = roomGO.GetComponent<RectTransform>();
                    
                    // Set anchors and pivot to center for proper relative positioning
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);

                    // Position them in a 3x3 grid. 
                    // The container (StandardMinimapContainer) should be sized and positioned 
                    // where the 3x3 grid should appear.
                    // Calculate offset from the center of the container.
                    float xPos = (x - (StandardViewSize - 1) / 2f) * (roomSize.x + roomSpacing.x);
                    float yPos = ((StandardViewSize - 1) / 2f - y) * (roomSize.y + roomSpacing.y); // Invert y for UI coordinates

                    rt.anchoredPosition = new Vector2(xPos, yPos);
                    
                    uiRoom.ResetRoom();
                    roomGO.SetActive(false); // Initially hide
                    standardViewRooms[x, y] = uiRoom;
                }
                else
                {
                    Destroy(roomGO);
                }
            }
        }
    }

    /// <summary>
    /// Initializes the minimap with data from the provided StageGrid.
    /// Clears any existing minimap elements and creates new UI rooms for the full map view.
    /// Subscribes to player room change events to update the minimap.
    /// </summary>
    /// <param name="stageGrid">The StageGrid data for the current stage.</param>
    public void InitializeMinimap(StageGrid stageGrid)
    {
        currentStageGrid = stageGrid;
        ClearExistingMinimap(); // Clear old rooms if any
        exploredRooms.Clear();

        if (minimapRoomPrefab == null || fullMapRoomsContainer == null)
        {
            Debug.LogError("MinimapRoom Prefab or FullMapRoomsContainer not assigned in MinimapController!");
            return;
        }

        if (stageGrid == null)
        {
            Debug.LogError("StageGridData is null. Cannot initialize minimap.");
            return;
        }

        // Calculate the total size of the grid to help with centering
        // Use stageGrid.GridSize for dimensions
        float gridDisplayWidth = stageGrid.GridSize.x * (roomSize.x + roomSpacing.x) - roomSpacing.x;
        float gridDisplayHeight = stageGrid.GridSize.y * (roomSize.y + roomSpacing.y) - roomSpacing.y;
        
        // Calculate origin offsets for centering the grid
        // X-axis origin (for the leftmost column's center, relative to container center)
        float gridOriginOffsetX = -gridDisplayWidth / 2f + roomSize.x / 2f;
        // Y-axis origin (for the bottom-most row's center, relative to container center, assuming grid Y increases upwards)
        float gridOriginOffsetY_corrected = -(gridDisplayHeight / 2f) + roomSize.y / 2f;

        // Iterate based on StageGrid.GridSize (assuming y=0 is bottom row, y increases upwards in StageGrid)
        for (int y = 0; y < stageGrid.GridSize.y; y++)
        {
            for (int x = 0; x < stageGrid.GridSize.x; x++)
            {
                Vector2Int coords = new(x, y);
                if (stageGrid.HasRoom(coords)) // Use HasRoom to check if a room exists
                {
                    GameObject roomGO = Instantiate(minimapRoomPrefab, fullMapRoomsContainer);
                    MinimapUIRoom uiRoom = roomGO.GetComponent<MinimapUIRoom>();

                    if (uiRoom != null)
                    {
                        roomGO.name = $"MinimapRoom_{x}_{y}";
                        RectTransform rt = roomGO.GetComponent<RectTransform>();
                        
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        
                        rt.anchoredPosition = new Vector2(
                            x * (roomSize.x + roomSpacing.x) + gridOriginOffsetX,
                            y * (roomSize.y + roomSpacing.y) + gridOriginOffsetY_corrected // Corrected Y calculation
                        );
                        
                        uiRoom.ResetRoom(); // Sets to default (hidden, default color)
                        uiRoom.SetBackgroundColor(defaultRoomColor); // Or a specific "unexplored" color
                        roomGO.SetActive(false); // Initially hide all rooms until discovered
                        minimapRooms[coords] = uiRoom;
                    }
                    else
                    {
                        Debug.LogError($"MinimapRoom_Prefab does not have a MinimapUIRoom component attached at {x},{y}");
                        Destroy(roomGO); // Cleanup
                    }
                }
            }
        }
        // After initializing, you might want to reveal the starting room or update player position
        // For now, player icon remains hidden.
        // Subscribe to player movement event from IsaacStageManager
        // This assumes you have a way to get a reference to IsaacStageManager
        IsaacStageManager stageManager = FindFirstObjectByType<IsaacStageManager>(); // Example: Find it in scene
        if (stageManager != null)
        {
            stageManager.OnPlayerChangedRoom -= OnPlayerRoomChanged; // Unsubscribe first to prevent duplicates
            stageManager.OnPlayerChangedRoom += OnPlayerRoomChanged;
            
            // Manually trigger an update for the starting room
            Vector2Int startRoomPosition = stageGrid.GetStartRoomPosition();
            Room startRoom = stageGrid.GetRoom(startRoomPosition);
            if(startRoom != null)
            {
                OnPlayerRoomChanged(null, startRoom); // previousRoom is null for the first room
            }
            else
            {
                Debug.LogWarning("[MinimapController] Start room not found in StageGrid at expected position: " + startRoomPosition);
            }
        }
        else
        {
            Debug.LogError("[MinimapController] IsaacStageManager not found in scene!");
        }
    }

    /// <summary>
    /// Clears all existing minimap room UI elements from the full map container
    /// and resets the internal dictionary tracking them.
    /// </summary>
    private void ClearExistingMinimap()
    {
        foreach (var pair in minimapRooms)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }
        minimapRooms.Clear();
    }

    /// <summary>
    /// Updates the player's current position on the minimap.
    /// This method is typically called when the player enters a new room.
    /// </summary>
    /// <param name="newCoordinates">The grid coordinates of the new room the player has entered.</param>
    public void UpdatePlayerPosition(Vector2Int newCoordinates)
    {
        // Logic to update current room visuals and player icon position
        // This method might become obsolete or be refactored into OnPlayerRoomChanged
        Room newPlayerRoom = currentStageGrid.GetRoom(newCoordinates);
        Room oldPlayerRoom = currentStageGrid.GetRoom(currentPlayerRoomCoordinates); // Might be null if coords were default
        OnPlayerRoomChanged(oldPlayerRoom, newPlayerRoom);
    }
    
    /// <summary>
    /// Handles the event triggered when the player changes rooms.
    /// Updates the explored status of the new room, refreshes room visuals,
    /// and updates both the standard and full minimap views.
    /// </summary>
    /// <param name="previousRoom">The room the player just left. Can be null if it's the first room.</param>
    /// <param name="newRoom">The room the player just entered.</param>
    public void OnPlayerRoomChanged(Room previousRoom, Room newRoom)
    {
        if (newRoom == null) return;

        currentPlayerRoomCoordinates = newRoom.gridPosition;
        exploredRooms[newRoom.gridPosition] = newRoom; // Mark as explored

    // Update the main dictionary of full-map UI rooms
        if (minimapRooms.TryGetValue(newRoom.gridPosition, out MinimapUIRoom fullMapUIRoom))
        {
            fullMapUIRoom.gameObject.SetActive(true);
            fullMapUIRoom.SetBackgroundColor(currentRoomColor);
            UpdateMinimapUIRoomVisuals(fullMapUIRoom, newRoom);
        }

        if (previousRoom != null && minimapRooms.TryGetValue(previousRoom.gridPosition, out MinimapUIRoom prevFullMapUIRoom))
        {
            prevFullMapUIRoom.SetBackgroundColor(exploredRoomColor); // Set previous room to explored color
        }

        UpdateStandardMinimapView();
        UpdatePlayerIconPosition();
    }

    /// <summary>
    /// Updates the visual representation of a specific minimap room UI element (doors, special icons).
    /// </summary>
    /// <param name="uiRoom">The MinimapUIRoom component to update.</param>
    /// <param name="roomData">The Room data associated with this UI element.</param>
    private void UpdateMinimapUIRoomVisuals(MinimapUIRoom uiRoom, Room roomData)
    {
        if (uiRoom == null || roomData == null || roomData.template == null) return; // Added null check for template

        // Update door indicators
        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            uiRoom.SetDoorVisibility(dir, roomData.doors.ContainsKey(dir) && roomData.doors[dir].isConnected);
        }

        // Update special room icon
        if (iconMapping != null)
        {
            Sprite icon = iconMapping.GetIcon(roomData.template.roomType);
            if (icon != null)
            {
                uiRoom.SetSpecialIcon(icon);
            }
            else
            {
                // Optionally hide icon if room type is Normal/Start or no specific icon found
                if (roomData.template.roomType == RoomType.Normal || roomData.template.roomType == RoomType.Start)
                {
                    uiRoom.HideSpecialIcon();
                }
                // else, if an icon was expected but not found, you might log a warning or leave the current icon
            }
        }
        else
        {
            // Fallback if no iconMapping is assigned - hide icon for non-special rooms
            if (roomData.template.roomType == RoomType.Normal || roomData.template.roomType == RoomType.Start)
            {
                uiRoom.HideSpecialIcon();
            }
            // else, you might want to log a warning that iconMapping is missing
        }
    }

    /// <summary>
    /// Updates the standard 3x3 minimap view centered on the player's current room.
    /// Shows explored rooms within the 3x3 grid and hides others.
    /// </summary>
    private void UpdateStandardMinimapView()
    {
        if (standardMinimapContainer == null || !standardMinimapContainer.activeSelf || currentStageGrid == null) return;

        for (int yOffset = -1; yOffset <= 1; yOffset++) // yOffset here represents game world offset relative to player (+1 UP, -1 DOWN)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                Vector2Int roomCoords = new(currentPlayerRoomCoordinates.x + xOffset, currentPlayerRoomCoordinates.y + yOffset);
                
                // Map game world yOffset to UI array yIndex:
                // game UP (yOffset = +1) should map to UI top row (array index 0)
                // game DOWN (yOffset = -1) should map to UI bottom row (array index 2)
                // yArrayIndex = -yOffset + 1
                int yArrayIndex = -yOffset + 1;
                MinimapUIRoom standardViewSlot = standardViewRooms[xOffset + 1, yArrayIndex];

                if (standardViewSlot == null) 
                {
                    Debug.LogWarning($"StandardMinimapView slot at [{xOffset + 1},{yArrayIndex}] is null. Skipping update for this slot.");
                    continue;
                }

                if (currentStageGrid.HasRoom(roomCoords) && exploredRooms.ContainsKey(roomCoords))
                {
                    Room roomData = exploredRooms[roomCoords];
                    standardViewSlot.gameObject.SetActive(true);
                    standardViewSlot.SetBackgroundColor(roomCoords == currentPlayerRoomCoordinates ? currentRoomColor : exploredRoomColor);
                    UpdateMinimapUIRoomVisuals(standardViewSlot, roomData);
                }
                else
                {
                    standardViewSlot.gameObject.SetActive(false); // Hide if no room or not explored
                }
            }
        }
    }

    /// <summary>
    /// Updates the position and visibility of the player icon on the minimap.
    /// The icon is parented to either the standard minimap container or the full map overlay,
    /// depending on which view is active.
    /// </summary>
    private void UpdatePlayerIconPosition()
    {
        if (playerIcon == null) return;

        playerIcon.gameObject.SetActive(true);

        if (isFullMapActive && fullMapOverlayContainer.activeSelf)
        {
            if (minimapRooms.TryGetValue(currentPlayerRoomCoordinates, out MinimapUIRoom currentRoomUIFull))
            {
                playerIcon.transform.SetParent(fullMapOverlayContainer.transform, false);
                playerIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                playerIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                playerIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                playerIcon.rectTransform.position = currentRoomUIFull.transform.position; 
            }
        }
        else if (standardMinimapContainer.activeSelf) 
        { 
            MinimapUIRoom currentRoomUIStandard = standardViewRooms[1,1]; // Center of the 3x3 grid
            playerIcon.transform.SetParent(standardMinimapContainer.transform, false);
            playerIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            playerIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playerIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            playerIcon.rectTransform.position = currentRoomUIStandard.transform.position;
        }
        else
        {
            playerIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles between the standard minimap view and the full map overlay.
    /// Called when the associated input action (e.g., Tab key) is performed.
    /// </summary>
    /// <param name="context">The callback context from the Unity Input System.</param>
    public void OnToggleFullMap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isFullMapActive = !isFullMapActive;
            standardMinimapContainer.SetActive(!isFullMapActive);
            fullMapOverlayContainer.SetActive(isFullMapActive);

            if (isFullMapActive)
            {
                // Refresh full map explored rooms
                foreach (var pair in minimapRooms)
                {
                    if (exploredRooms.ContainsKey(pair.Key))
                    {
                        pair.Value.gameObject.SetActive(true);
                        pair.Value.SetBackgroundColor(pair.Key == currentPlayerRoomCoordinates ? currentRoomColor : exploredRoomColor);
                        UpdateMinimapUIRoomVisuals(pair.Value, exploredRooms[pair.Key]);
                    }
                    else
                    {
                        pair.Value.gameObject.SetActive(false);
                    }
                }
            }
            UpdatePlayerIconPosition(); // Update icon parent and position
        }
    }

    /// <summary>
    /// Unity lifecycle method called when the GameObject is destroyed.
    /// Unsubscribes from events to prevent memory leaks and potential errors.
    /// </summary>
    void OnDestroy()
    {
        // It's good practice to unsubscribe from events when the object is destroyed
        // to prevent potential errors if the event source outlives this object.
        IsaacStageManager stageManager = FindFirstObjectByType<IsaacStageManager>();
        if (stageManager != null)
        {
            stageManager.OnPlayerChangedRoom -= OnPlayerRoomChanged;
        }
    }
}
