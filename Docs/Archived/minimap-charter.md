# Minimap Implementation Charter

**Date:** May 26, 2025
**Author:** GitHub Copilot
**Stakeholders:** Development Team

## 1. Project Goal
To implement a dynamic minimap system that displays explored rooms, special room types, and the player's current location. This system will enhance player navigation and awareness within the procedurally generated stages, drawing inspiration from "The Binding of Isaac."

## 2. Scope

### In Scope:
*   **Minimap Display & Positioning:**
    *   The minimap will be displayed in a corner of the screen (initially top-left). Position should be configurable via script parameters rather than hard-coded.
    *   The minimap will have a black background.
    *   The minimap will have a border with a configurable color.
    *   Rooms will be represented by filled rectangular UI elements, with their color configurable (linked to the border color parameter).
    *   Small squares will extend from each side of a room's rectangle to indicate the presence of a door.
*   **Room States & Icons:**
    *   **Explored Rooms:** Displayed as yellow, filled rectangles. If a room is special (e.g., treasure, boss, shop), an icon representing its type will be centered on its rectangle.
    *   **Unexplored Rooms:** Will not be visible on the minimap until the player enters them.
    *   **Current Room:** The room the player is currently in will be clearly indicated, possibly by a unique player icon on its minimap representation.
*   **Minimap View & Behavior:**
    *   **Standard View:** The minimap will center on the current room, displaying a 3x3 grid of rooms (the current room and its immediate cardinal and diagonal neighbors). Rooms beyond this 3x3 grid will be clipped or not rendered in this view.
    *   **Full Map Overlay:** Players can press a designated key (e.g., Tab) to toggle a full-screen overlay showing all explored rooms of the current stage.
*   **Technical Implementation:**
    *   Creation of necessary UI prefabs and components from scratch using Unity's UI system (likely UGUI for ease of overlay).
    *   Logic to dynamically generate and update the minimap based on stage layout and player exploration.

### Out of Scope (for initial prototype):
*   Interactive minimap features like manual zooming or panning (beyond the full map toggle).
*   Animated transitions or effects on the minimap (e.g., room discovery animations).
*   Highly detailed or complex visual styles beyond simple geometric shapes and icons.
*   Displaying individual enemies, items (other than special room indicators), or other dynamic game elements on the minimap.
*   Saving minimap exploration state between game sessions (unless stage progression already handles this).

## 3. High-Level Requirements & Tasks

### 3.1. UI System & Prefabs
*   **Minimap Container:** A root UI GameObject (e.g., a Canvas Panel) to hold all minimap elements.
    *   Configurable screen anchor and offset.
*   **Room UI Prefab:** A prefab for representing a single room on the minimap.
    *   Contains an Image component for the room background (color configurable, as mentioned above).
    *   Contains child GameObjects/Images for door indicators (small squares on each side).
    *   Contains an Image component for special room icons (initially inactive).
*   **Player Icon UI:** A UI element (e.g., an Image) to represent the player's current room on the minimap.
*   **Full Map Container:** A separate UI panel for the full map overlay.

### 3.2. Core Logic (`MinimapController` or similar)
*   **Initialization:**
    *   Receive stage data (`StageGrid`) from `IsaacStageManager` upon stage generation.
    *   Instantiate and arrange Room UI elements based on the stage layout for the full map view (initially all hidden or marked as unexplored).
    *   Set up the standard (3x3 centered) minimap view.
*   **Room Discovery & Update:**
    *   Interface with `IsaacStageManager` or `Room` events to detect when the player enters a new room.
    *   Mark the newly entered room as explored.
    *   Update the corresponding Room UI element:
        *   Change appearance to "explored" (e.g., activate/color the yellow rectangle).
        *   If it's a special room, activate and set the appropriate icon.
    *   Update the player icon to the new current room.
    *   Adjust the standard minimap view to re-center on the new current room, showing its 3x3 neighborhood.
*   **Door Indication:**
    *   When a room UI is created/updated, activate door indicator sub-elements based on the `Room` data.
*   **Special Room Icon Management:**
    *   A system (possibly using a ScriptableObject or enum mapping) to associate room types with specific icons.
    *   Load and assign icons to Room UI prefabs as they are discovered.
*   **Full Map Toggle:**
    *   Input handling for the Tab key (or designated key) using Unity's new Input System. This will involve creating a method that expects an `InputAction.CallbackContext` (similar to `PlayerController2D.OnMove`).
    *   Toggle visibility of the standard minimap and the full map overlay.
    *   The full map overlay should display all *explored* rooms in their correct relative positions.

### 3.3. Integration
*   **`IsaacStageManager`:**
    *   The `IsaacStageManager` will likely instantiate or provide a reference to the `MinimapController`.
    *   It will pass the `StageGrid` data to the `MinimapController` after a new stage is generated.
    *   It will notify the `MinimapController` (or the `MinimapController` will subscribe to events) when the player changes rooms.
*   **`Room` Class:**
    *   The `Room` class should expose its grid position, door configuration, and type (e.g., Normal, Boss, Treasure).

## 4. Key Deliverables
1.  A `MinimapController.cs` script managing all minimap logic.
2.  A `MinimapUIRoom.cs` script (or similar) attached to the Room UI prefab to manage its state and visuals.
3.  UI Prefabs:
    *   `MinimapCanvas_Prefab` (or similar, containing the standard minimap layout area and the full map overlay panel).
    *   `MinimapRoom_Prefab`.
4.  A set of placeholder icons for special room types.
5.  This charter document (`MinimapCharter.md`).

## 5. Assumptions
*   The `StageGrid` object contains all necessary information about rooms, including their positions, connections (doors), and types.
*   The `Room` class can provide its type (e.g., via an enum or string identifier) to determine if it's a special room.
*   A clear event or callback mechanism exists for when the player transitions between rooms.
*   Basic UI assets (simple sprites for squares, placeholder icons) are available or can be quickly created.
*   Unity's UGUI system will be the primary tool for UI implementation.

## 6. Open Questions & Future Considerations
*   **Specific Icon Assets:** What are the final art assets for special room icons? (Placeholders will be used initially).
*   **Performance:** For very large stages, will rendering all explored rooms in the "full map" mode cause performance issues? (Optimization can be addressed later if it becomes a problem).
*   **Styling Details:** Exact colors (beyond "yellow" for rooms), font for any text, and icon styles can be refined iteratively.
*   **Minimap Scaling:** How should the room representations scale in the full map view if the stage is exceptionally large, to ensure it fits on screen? (May require dynamic scaling or a scrollable view for extreme cases).

This charter outlines the plan for the minimap feature. It should serve as a guide during development.
