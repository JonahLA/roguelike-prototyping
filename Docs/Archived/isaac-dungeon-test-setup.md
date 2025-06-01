# Testing Setup for Isaac-Style Room Generation

## Overview
This guide will help you set up a test scene to verify that the room generation system works properly with the new camera system.

## Scene Setup

### 1. Create a Test Scene
1. Create a new scene (File > New Scene) or use an existing scene
2. Save it as "IsaacDungeonTest"

### 2. Create GameObjects
Create the following hierarchy:
```
- StageManager (Empty GameObject)
  - StageGenerator (Empty GameObject)
  - Player (Empty GameObject)
- Main Camera
```

### 3. Configure Components

#### StageManager GameObject
1. Add the `IsaacStageManager` component
2. Configure the following references:
   - **Stage Generator**: Drag the StageGenerator child object
   - **Player Transform**: Drag the Player child object
   - **Camera Controller**: Drag the Main Camera (after adding RoomCameraController to it)

#### StageGenerator GameObject
1. Add the `IsaacStageGenerator` component
2. Configure the generation settings:
   - **Grid Size**: Start with (8, 8)
   - **Min Main Path Length**: 3
   - **Max Main Path Length**: 7
   - **Special Rooms Count**: 2
   - **Branch Probability**: 0.5
   - **Use Random Seed**: Check this for random generation
3. Add Room Templates (you'll need to create these first, see Room Template Creation below)
4. Add Door Prefab (you'll need to create this first, see Door Prefab Creation below)

#### Player GameObject
1. Add a `SpriteRenderer` component and assign a temporary sprite
2. Add a `Rigidbody2D` component:
   - **Body Type**: Dynamic
   - **Gravity Scale**: 0 (for top-down movement)
3. Add a `BoxCollider2D` component
4. Add a simple player movement script (see Player Movement Script below)

#### Main Camera
1. Set to Orthographic projection
2. Set Position to (0, 0, -10)
3. Add the `RoomCameraController` component
4. Configure:
   - **Target Transform**: Drag the Player GameObject
   - **Smooth Transition**: On
   - **Camera Size**: 10 (adjust to match your room size)

### 4. Create Room Templates

#### Door Prefab Creation
1. Create an empty GameObject named "DoorPrefab"
2. Add a `SpriteRenderer` with a door sprite
3. Add a `BoxCollider2D` marked as "Is Trigger"
4. Add the `DoorController` component
5. Save as a prefab in "Assets/Prefabs/Doors"

#### Basic Room Template Creation
For each room type (Start, Normal, Boss, Treasure, Shop):

1. Create an empty GameObject (e.g., "StartRoomTemplate")
2. Create a child "Floor" with a Tilemap for the floor
3. Create a child "Walls" with a Tilemap for walls (add colliders)
4. Add door positions as empty GameObjects at the center of each wall:
   - Create these at the exact positions where doors should appear
   - Name them descriptively (e.g., "NorthDoorPos")
5. Save as a prefab in "Assets/Prefabs/Rooms"

#### Room Template ScriptableObjects
For each room prefab:

1. Create a `RoomTemplate` ScriptableObject (Right-click > Create > ScriptableObject > RoomTemplate)
2. Assign the room prefab you created
3. Add door configurations for each possible door:
   - **Direction**: Set the direction (North, East, South, West)
   - **Position**: Set the local position relative to the room center
4. Save with descriptive names in "Assets/Scriptable Objects/RoomTemplates"

### 5. Play Mode Testing
1. Enter Play mode
2. The stage should generate with rooms and doors
3. The player should spawn in the start room
4. The camera should be properly positioned on the start room
5. Try moving the player to doors to test room transitions

## Player Movement Script
Here's a simple script for testing player movement:

```csharp
using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    
    private Rigidbody2D _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        _rb.velocity = movement * _speed;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we triggered a door
        DoorController door = other.GetComponent<DoorController>();
        if (door != null && door.isConnected)
        {
            // Find the stage manager
            IsaacStageManager stageManager = FindObjectOfType<IsaacStageManager>();
            if (stageManager != null)
            {
                stageManager.OnPlayerEnteredDoor(door);
            }
        }
    }
}
```

## Troubleshooting
- **Camera doesn't follow room transitions**: Make sure the RoomCameraController is properly assigned in IsaacStageManager
- **Room prefabs not appearing**: Check the room templates and make sure they have properly assigned prefabs
- **Doors not connecting**: Verify door configurations in the RoomTemplate ScriptableObjects
- **Door transitions not working**: Make sure door triggers are properly set up and player collider is interacting with them

## Next Steps
Once this is working, you can:
1. Refine your room templates with more variety
2. Add mechanics for room clearing (enemies, obstacles)
3. Implement a minimap system
4. Add visual effects for room transitions
