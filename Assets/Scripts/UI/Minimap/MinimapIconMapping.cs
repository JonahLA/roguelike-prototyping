using UnityEngine;
using System.Collections.Generic;

// Enum defined in Room.cs - ensure it's accessible here or duplicate/reference appropriately
// For simplicity, assuming RoomType enum is globally accessible or in an assembly MinimapController can access.

/// <summary>
/// A serializable class to pair a RoomType with a specific Sprite for the minimap icon.
/// </summary>
[System.Serializable]
public class RoomTypeIconPair
{
    public RoomType roomType;
    public Sprite icon;
}

/// <summary>
/// ScriptableObject to store mappings from RoomType to Sprite for minimap icons.
/// Create instances of this via Assets > Create > Flare > Minimap Icon Mapping.
/// </summary>
[CreateAssetMenu(fileName = "MinimapIconMapping", menuName = "Roguelike/Minimap Icon Mapping", order = 1)]
public class MinimapIconMapping : ScriptableObject
{
    [Tooltip("List of room types and their corresponding minimap icons.")]
    public List<RoomTypeIconPair> iconMappings;

    /// <summary>
    /// Retrieves the icon Sprite for a given RoomType.
    /// </summary>
    /// <param name="roomType">The type of the room.</param>
    /// <returns>The Sprite for the icon, or null if no mapping is found.</returns>
    public Sprite GetIcon(RoomType roomType)
    {
        if (iconMappings == null) return null;

        foreach (var mapping in iconMappings)
        {
            if (mapping.roomType == roomType)
            {
                return mapping.icon;
            }
        }
        return null; // No icon found for this room type
    }
}
