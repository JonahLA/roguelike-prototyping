# Minimap Implementation Plan

## 1. Overview
This document outlines the implementation details for a dynamic minimap system for the "Flare - Prototype" game. The minimap will display explored rooms, special room types, and the player's current location, enhancing navigation and awareness within procedurally generated stages, inspired by "The Binding of Isaac."

## 2. UI Components & Structure (UGUI)

### 2.1. Main Canvas
*   A root `Canvas` GameObject will host all minimap UI.
    *   Render Mode: Screen Space - Overlay (or Camera if specific sorting is needed later).
    *   `CanvasScaler` for resolution independence.
    *   `GraphicRaycaster` for potential future interactions (though not in initial scope).

### 2.2. Standard Minimap Container
*   A `GameObject` (likely a `Panel` with an `Image` component for the black background and border) anchored to a screen corner (e.g., top-left).
    *   Position and border color will be configurable via script.
    *   Will contain a grid layout (e.g., `GridLayoutGroup` or manual positioning) for the 3x3 room display.
    *   Masking or clipping will be applied to ensure only the 3x3 grid is visible.

### 2.3. Full Map Overlay Container
*   A `GameObject` (likely a `Panel` with an `Image` component for a semi-transparent background) that covers the full screen when active.
    *   Initially inactive.
    *   Will contain a grid layout or manually positioned room UI elements representing all explored rooms of the stage.

### 2.4. Room UI Prefab (`MinimapRoom_Prefab`)
*   A `Prefab` representing a single room on the minimap.
    *   Root `GameObject` with an `Image` component for the room's background rectangle.
        *   Color will be configurable (linked to the minimap border color).
    *   Child `GameObject`s with `Image` components for door indicators (small squares) on each of the four sides (North, East, South, West). These will be enabled/disabled based on actual door presence.
    *   A child `GameObject` with an `Image` component for the special room icon (e.g., treasure, boss). Initially inactive and centered on the room rectangle.
    *   A script (e.g., `MinimapUIRoom.cs`) to manage its visual state (explored, icon type, door visibility).

### 2.5. Player Icon UI
*   A `GameObject` with an `Image` component to represent the player's current location on the minimap.
    *   This will be overlaid on the current room's UI element in both standard and full map views.

## 3. Core Logic (`MinimapController.cs`)

### 3.1. Initialization
*   Attached to a persistent UI manager object or the main Minimap Canvas.
*   Receives `StageGrid` data from `IsaacStageManager` after stage generation.
*   Instantiates `MinimapRoom_Prefab` for all potential room slots in the `StageGrid` for the full map view. These are initially hidden or marked as unexplored.
*   Sets up the standard (3x3 centered) minimap view based on the player's starting room.
*   Configures minimap background and border colors.

### 3.2. Room Discovery & Updates
*   Subscribes to an event from `IsaacStageManager` or `Player` that fires when the player enters a new room.
*   On room change:
    *   Identifies the `MinimapRoom_Prefab` instance corresponding to the new current room and any newly adjacent explored rooms.
    *   Updates the visual state of the `MinimapUIRoom.cs` script:
        *   Sets room color to indicate "explored."
        *   If the room is special (e.g., boss, treasure), enables and sets the appropriate icon sprite.
        *   Enables door indicators based on the `Room`'s actual door connections.
    *   Updates the player icon's position to the new current room.
    *   Refreshes the standard minimap view:
        *   Centers the 3x3 grid on the new current room.
        *   Activates/deactivates `MinimapRoom_Prefab` instances to show only the current room and its 8 immediate neighbors. Other rooms in the standard view are clipped or hidden.

### 3.3. Door Indication Logic
*   The `MinimapUIRoom.cs` script will have references to its four door indicator `Image` components.
*   When a room is updated (e.g., upon discovery), it will check the `Room` data for existing doors in each direction and toggle the visibility of the corresponding door indicator UI elements.

### 3.4. Special Room Icon Management
*   A ScriptableObject or a dictionary within `MinimapController` will map `RoomType` (enum) to `Sprite` assets for icons.
*   When a special room is discovered, `MinimapController` tells the corresponding `MinimapUIRoom.cs` to display the correct icon from this mapping.

### 3.5. Full Map Toggle Logic
*   A public method `OnToggleFullMap(InputAction.CallbackContext context)` will be implemented.
    *   This method will be linked to a Unity Input Action (e.g., bound to the "Tab" key).
    *   When triggered (e.g., `ctx.performed`), it toggles the active state of the Standard Minimap Container and the Full Map Overlay Container.
    *   The Full Map Overlay will display all *explored* `MinimapRoom_Prefab` instances in their correct grid positions.

## 4. Integration Points

### 4.1. `IsaacStageManager.cs`
*   Will hold a reference to or instantiate the `MinimapController`.
*   After generating `_currentStage` (`StageGrid`), it will pass this data to `MinimapController.InitializeMinimap(_currentStage)`.
*   When the player enters a new room (`OnPlayerEnteredDoor`), it will notify `MinimapController.UpdateCurrentRoom(Room newRoom, Room previousRoom)`.

### 4.2. `Room.cs` (Assumed structure)
*   Must expose:
    *   `Vector2Int gridPosition`
    *   `RoomType roomType` (enum: Normal, Boss, Treasure, etc.)
    *   A way to query existing doors (e.g., `Dictionary<Direction, DoorController> doors` or `bool HasDoor(Direction dir)`).

### 4.3. `PlayerInput` Component
*   An Input Action Asset will define a "ToggleMap" action.
*   A `PlayerInput` component (likely on the Player GameObject or a dedicated Input Manager) will have an event for this action, which will be wired to `MinimapController.OnToggleFullMap`.

## 5. Asset Requirements
*   **Sprites:**
    *   Simple square sprite for room background (tintable).
    *   Simple small square sprite for door indicators (tintable).
    *   Placeholder icons for player position, boss room, treasure room, etc.
*   **Prefabs:**
    *   `MinimapRoom_Prefab`
    *   `MinimapCanvas_Prefab` (containing standard view and full map overlay structures)

## 6. Key Script Responsibilities

### 6.1. `MinimapController.cs`
*   Manages overall minimap state (standard vs. full view).
*   Initializes the minimap with stage data.
*   Updates minimap based on player progression and room discovery.
*   Handles input for toggling full map view.
*   Manages the pool of `MinimapRoom_Prefab` instances for the full map.
*   Controls the visibility and content of the 3x3 standard view.
*   Configures minimap appearance (background, border, room colors).

### 6.2. `MinimapUIRoom.cs` (on `MinimapRoom_Prefab`)
*   Controls the visual appearance of a single room on the minimap.
    *   Sets background color.
    *   Shows/hides door indicators.
    *   Shows/hides and sets the special room icon.
    *   Updates its state based on data from `MinimapController`.

This plan provides a detailed breakdown for implementing the minimap feature.
