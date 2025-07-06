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
    /// <summary>
    /// The type of room this icon represents.
    /// </summary>
    [Tooltip("The type of room this icon represents.")]
    public RoomType roomType;

    /// <summary>
    /// The sprite icon to display for this room type on the minimap.
    /// </summary>
    [Tooltip("The sprite icon to display for this room type on the minimap.")]
    public Sprite icon;
}

/// <summary>
/// ScriptableObject to store mappings from RoomType to Sprite for minimap icons.
/// Create instances of this via Assets > Create > Flare > Minimap Icon Mapping.
/// </summary>
[CreateAssetMenu(fileName = "MinimapIconMapping", menuName = "Flare/Minimap Icon Mapping", order = 1)]
public class MinimapIconMapping : ScriptableObject
{
    /// <summary>
    /// List of room types and their corresponding minimap icons.
    /// </summary>
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
