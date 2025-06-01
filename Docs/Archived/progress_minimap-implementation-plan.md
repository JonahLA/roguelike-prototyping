# Progress: Minimap Implementation

**Last Updated:** May 26, 2025

This document tracks the progress of implementing the minimap feature as outlined in the Minimap Implementation Plan and Minimap Charter.

## Key Milestones & Tasks

### Phase 1: UI Setup & Basic Display
- [x] **Create UI Prefabs:**
    - [x] `MinimapCanvas_Prefab`: Root canvas for all minimap elements.
        - [x] Setup for standard (corner) minimap view.
        - [x] Setup for full-screen overlay panel.
    - [x] `MinimapRoom_Prefab`: Represents a single room.
        - [x] Image for room background (configurable color).
        - [x] Child Images for 4 door indicators.
        - [x] Child Image for special room icon.
    - [x] Player Icon UI element.
- [x] **Develop `MinimapUIRoom.cs` script:**
    - [x] Logic to update room background color.
    - [x] Logic to show/hide door indicators.
    - [x] Logic to show/hide and set special room icon sprite.
- [x] **Develop `MinimapController.cs` (Initial Setup):**
    - [x] Ability to receive `StageGrid` data.
    - [x] Basic instantiation of `MinimapRoom_Prefab` for all rooms in the full map view (initially all hidden/unexplored).
    - [x] Logic to set minimap background color (black).
    - [x] Logic to set minimap border color (configurable).
    - [x] Logic to set room color (configurable, linked to border color).

### Phase 2: Core Logic & Player Interaction
- [x] **Implement Room Discovery in `MinimapController.cs`:**
    - [x] Subscribe to `IsaacStageManager` event for player room changes.
    - [x] On room entry, mark corresponding `MinimapUIRoom` as explored.
    - [x] Update `MinimapUIRoom` visuals (color, door indicators, special icon if applicable).
- [x] **Implement Standard Minimap View (3x3):**
    - [x] Logic in `MinimapController.cs` to center the view on the current room.
    - [x] Display only the current room and its 8 immediate neighbors.
    - [x] Clip or hide rooms outside this 3x3 grid in the standard view.
    - [x] Position player icon on the current room in the standard view.
- [x] **Implement Full Map Overlay Toggle:**
    - [x] Create `OnToggleFullMap(InputAction.CallbackContext context)` method in `MinimapController.cs`.
    - [ ] Hook up Unity Input Action (e.g., "Tab" key) to this method.
    - [x] Logic to toggle visibility between standard minimap and full map overlay.
    - [x] Ensure full map overlay correctly displays all *explored* rooms.
    - [x] Position player icon on the current room in the full map view.

### Phase 3: Integration & Refinement
- [x] **Integrate with `IsaacStageManager.cs`:**
    - [x] `IsaacStageManager` instantiates/references `MinimapController`.
    - [x] `IsaacStageManager` passes `StageGrid` data upon new stage generation.
    - [x] `IsaacStageManager` calls `MinimapController` methods on player room change.
- [x] **Special Room Icon System:**
    - [x] Create ScriptableObject or dictionary for `RoomType` to `Sprite` mapping.
    - [x] `MinimapController` uses this mapping to set icons on `MinimapUIRoom` instances.
- [X] **Testing & Debugging:**
    - [X] Test with various stage layouts and sizes.
    - [X] Verify correct room discovery and display.
    - [X] Test standard and full map views thoroughly.
    - [X] Check door indicators and special room icons.
    - [X] Ensure configurable colors (border, room) work as expected.
- [X] **Code Review & Documentation:**
    - [X] Review scripts for clarity, efficiency, and adherence to project standards.
    - [X] Add/update code comments as needed.

## Open Issues / To-Do Later
*   (From Charter) Performance for very large stages in full map mode.
*   (From Charter) Final art assets for icons (using placeholders for now).
*   (From Charter) Minimap scaling for exceptionally large stages in full map view.

---
*This document will be updated as tasks are completed.*
