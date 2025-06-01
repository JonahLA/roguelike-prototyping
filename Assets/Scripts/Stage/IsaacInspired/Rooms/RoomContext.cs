using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Holds references to essential components and GameObjects within a Room prefab's hierarchy.
/// This component should be added to the root of your Room prefab, and its fields
/// assigned in the Unity Inspector. The dynamically added Room script will use this
/// to access its required Tilemaps or other parts.
/// </summary>
public class RoomContext : MonoBehaviour
{
    [Header("Essential Tilemaps")]
    [Tooltip("Assign the 'Walls' Tilemap from this Room prefab's hierarchy.")]
    public Tilemap WallsTilemap;

    [Tooltip("Assign the 'Floors' Tilemap from this Room prefab's hierarchy (optional).")]
    public Tilemap FloorsTilemap; // Example if you also need a floor tilemap reference

    // Add other references here as needed, e.g.:
    // public Transform EnemySpawnPointsContainer;
    // public Light RoomLight;
}
