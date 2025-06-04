using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single room UI element on the minimap.
/// Manages the visual state of the room, including its background color,
/// special icon (e.g., for boss rooms, item rooms), and door indicators.
/// </summary>
public class MinimapUIRoom : MonoBehaviour
{    [Header("UI References")]
    /// <summary>
    /// The Image component for the room's main background.
    /// </summary>
    [Tooltip("The Image component for the room's main background.")]
    [SerializeField] private Image roomBackground;

    /// <summary>
    /// The Image component used to display a special icon for this room (e.g., boss, item room).
    /// </summary>
    [Tooltip("The Image component used to display a special icon for this room (e.g., boss, item room).")]
    [SerializeField] private Image specialRoomIcon;

    [Header("Door Indicators")]
    /// <summary>
    /// GameObject representing the door indicator for the top (North) side of the room.
    /// </summary>
    [Tooltip("GameObject representing the door indicator for the top (North) side of the room.")]
    [SerializeField] private GameObject doorIndicatorUp;

    /// <summary>
    /// GameObject representing the door indicator for the bottom (South) side of the room.
    /// </summary>
    [Tooltip("GameObject representing the door indicator for the bottom (South) side of the room.")]
    [SerializeField] private GameObject doorIndicatorDown;

    /// <summary>
    /// GameObject representing the door indicator for the left (West) side of the room.
    /// </summary>
    [Tooltip("GameObject representing the door indicator for the left (West) side of the room.")]
    [SerializeField] private GameObject doorIndicatorLeft;

    /// <summary>
    /// GameObject representing the door indicator for the right (East) side of the room.
    /// </summary>
    [Tooltip("GameObject representing the door indicator for the right (East) side of the room.")]
    [SerializeField] private GameObject doorIndicatorRight;

    /// <summary>
    /// Sets the background color of the minimap room UI element.
    /// This color is also applied to the door indicators.
    /// </summary>
    /// <param name="color">The color to set for the room background and doors.</param>
    public void SetBackgroundColor(Color color)
    {
        if (roomBackground != null)
        {
            roomBackground.color = color;
        }

        // Also set the color of the door indicators
        SetDoorIndicatorColor(doorIndicatorUp, color);
        SetDoorIndicatorColor(doorIndicatorDown, color);
        SetDoorIndicatorColor(doorIndicatorLeft, color);
        SetDoorIndicatorColor(doorIndicatorRight, color);
    }

    /// <summary>
    /// Sets the color of a specific door indicator's Image component.
    /// </summary>
    /// <param name="doorIndicator">The GameObject of the door indicator.</param>
    /// <param name="color">The color to apply.</param>
    private void SetDoorIndicatorColor(GameObject doorIndicator, Color color)
    {
        if (doorIndicator != null)
        {
            Image doorImage = doorIndicator.GetComponent<Image>();
            if (doorImage != null)
            {
                doorImage.color = color;
            }
        }
    }

    /// <summary>
    /// Sets the visibility of a door indicator for a specific direction.
    /// </summary>
    /// <param name="direction">The direction of the door (North, South, East, West).</param>
    /// <param name="isVisible">True to show the door indicator, false to hide it.</param>
    public void SetDoorVisibility(Direction direction, bool isVisible)
    {
        GameObject doorIndicator = null;
        switch (direction)
        {
            case Direction.North: // Changed from DoorDirection.Up
                doorIndicator = doorIndicatorUp;
                break;
            case Direction.South: // Changed from DoorDirection.Down
                doorIndicator = doorIndicatorDown;
                break;
            case Direction.West:  // Changed from DoorDirection.Left
                doorIndicator = doorIndicatorLeft;
                break;
            case Direction.East:  // Changed from DoorDirection.Right
                doorIndicator = doorIndicatorRight;
                break;
        }

        if (doorIndicator != null)
        {
            doorIndicator.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Sets a special icon for the room (e.g., boss, item room).
    /// If the provided sprite is null, the icon will be hidden.
    /// </summary>
    /// <param name="iconSprite">The sprite to display as the special icon. Can be null.</param>
    public void SetSpecialIcon(Sprite iconSprite)
    {
        if (specialRoomIcon != null)
        {
            if (iconSprite != null)
            {
                specialRoomIcon.sprite = iconSprite;
                specialRoomIcon.gameObject.SetActive(true);
            }
            else
            {
                specialRoomIcon.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Hides the special room icon.
    /// </summary>
    public void HideSpecialIcon()
    {
        if (specialRoomIcon != null)
        {
            specialRoomIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Resets the room UI to a default state: clear background, hidden icon, all doors hidden,
    /// and the GameObject itself is deactivated.
    /// </summary>
    public void ResetRoom()
    {
        SetBackgroundColor(Color.clear); // Or some default unexplored color
        HideSpecialIcon();
        SetDoorVisibility(Direction.North, false); // Changed from DoorDirection.Up
        SetDoorVisibility(Direction.South, false); // Changed from DoorDirection.Down
        SetDoorVisibility(Direction.West, false);  // Changed from DoorDirection.Left
        SetDoorVisibility(Direction.East, false);  // Changed from DoorDirection.Right
        gameObject.SetActive(false); // Initially hide the room UI until discovered or needed
    }
}
