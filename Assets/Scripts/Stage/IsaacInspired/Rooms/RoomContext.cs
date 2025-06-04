using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Holds references to essential components and GameObjects within a Room prefab's hierarchy.
/// This component should be added to the root of your Room prefab, and its fields
/// assigned in the Unity Inspector. The dynamically added <see cref="Room"/> script (or its derivatives)
/// will use this context to access its required Tilemaps or other important parts of the prefab.
/// </summary>
/// <remarks>
/// This approach helps decouple the <see cref="Room"/> logic from the specific structure of its prefab,
/// allowing for more flexibility in prefab design as long as the context provides the necessary references.
/// </remarks>
public class RoomContext : MonoBehaviour
{
    [Header("Essential Tilemaps")]
    [Tooltip("Assign the 'Walls' Tilemap from this Room prefab's hierarchy. This is typically required for room boundary calculations and visuals.")]
    public Tilemap WallsTilemap;

    [Tooltip("Assign the 'Floors' Tilemap from this Room prefab's hierarchy. Used for floor visuals and potentially pathfinding.")]
    public Tilemap FloorsTilemap; // Example if you also need a floor tilemap reference

    // Add other references here as needed, e.g.:
    // [Header("Gameplay Elements")]
    // [Tooltip("Parent transform for enemy spawn points within this room.")]
    // public Transform EnemySpawnPointsContainer;
    //
    // [Tooltip("Main light source for this room, if any.")]
    // public Light RoomLight;

    private void Awake()
    {
        // It's good practice to validate essential references in Awake
        // to catch configuration errors early.
        if (WallsTilemap == null)
        {
            Debug.LogError($"[RoomContext] '{gameObject.name}': WallsTilemap is not assigned. Please assign it in the inspector.", this);
        }

        // FloorsTilemap is optional, so a warning might be more appropriate if it's missing
        // depending on your game's requirements.
        if (FloorsTilemap == null)
        {
            Debug.LogWarning($"[RoomContext] '{gameObject.name}': FloorsTilemap is not assigned. This might be intentional.", this);
        }
    }
}
