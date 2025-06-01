# Setting Up the Camera System for Room Transitions

## Overview
The `RoomCameraController` component manages camera movement between rooms, allowing the camera to smoothly follow the player as they transition through different rooms in your Isaac-style dungeon.

## Setup Instructions

### 1. Camera Setup
1. Select your Main Camera in the Hierarchy
2. Add the `RoomCameraController` component to it (Component > Scripts > Stage > IsaacInspired > RoomCameraController)
3. Set the camera to Orthographic projection (if not already)
4. Configure initial settings in the RoomCameraController:
   - **Main Camera**: Leave empty (it will find the main camera automatically)
   - **Target Transform**: Assign your player GameObject
   - **Smooth Transition**: Enable for smooth camera movement between rooms
   - **Transition Speed**: 8 is a good default value
   - **Camera Size**: Set to match your room size (10 works well for 20x20 rooms)
   - **Camera Offset**: Typically (0, 0, -10) to position the camera behind the scene
   - **Restrict To Bounds**: Enable if you want to limit camera movement
   - **Min/Max Bounds**: Set if you've enabled bounds restriction

### 2. IsaacStageManager Setup
1. Find your `IsaacStageManager` GameObject in the Hierarchy
2. In the Inspector, locate the `Camera Controller` field
3. Drag your camera with the RoomCameraController component into this field

### 3. Testing
1. Enter Play mode
2. The camera should snap to the starting room
3. When the player moves through doors, the camera should smoothly transition to the new room

### Troubleshooting
- If the camera doesn't move to the starting room, check that `_cameraController` is properly assigned
- If transitions between rooms aren't smooth, adjust the `Transition Speed` value
- If the camera view is too close or too far, adjust the `Camera Size` parameter

## Advanced Configuration

### Camera Bounds
- You can limit where the camera can move by enabling `Restrict To Bounds`
- Set appropriate min/max bounds based on your overall stage size
- These can be programmatically updated when a new stage is generated:

```csharp
// Example: Set camera bounds based on stage grid size
Vector2 minBound = new Vector2(0, 0);
Vector2 maxBound = new Vector2(
    _stageGrid.GridSize.x * roomSize, 
    _stageGrid.GridSize.y * roomSize
);
_cameraController.SetCameraBounds(minBound, maxBound);
```

### Custom Transitions
- For special room transitions (boss rooms, etc.), you can use `SnapToRoom()` to immediately move the camera
- This can be combined with effects like screen shake or fade-ins
