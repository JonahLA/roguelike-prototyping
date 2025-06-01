using UnityEngine;

/// <summary>
/// Provides utility methods for working with the <see cref="Direction"/> enum.
/// </summary>
public static class DirectionUtils
{
    /// <summary>
    /// Gets the opposite cardinal direction.
    /// </summary>
    /// <param name="dir">The input direction.</param>
    /// <returns>The opposite direction (e.g., North returns South).</returns>
    /// <remarks>
    /// If an undefined direction value is passed (which shouldn't happen with standard enum usage),
    /// this method currently returns the input direction.
    /// </remarks>
    public static Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => dir,// Should not happen with cardinal directions
        };
    }

    /// <summary>
    /// Determines the cardinal direction from one point to another.
    /// </summary>
    /// <param name="from">The starting coordinate.</param>
    /// <param name="to">The target coordinate.</param>
    /// <returns>The <see cref="Direction"/> that best represents the path from 'from' to 'to'.</returns>
    /// <remarks>
    /// If the absolute difference in x and y coordinates is equal, priority is given to vertical (North/South) movement.
    /// </remarks>
    public static Direction GetDirectionFromTo(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            return diff.x > 0 ? Direction.East : Direction.West;
        }
        else
        {
            return diff.y > 0 ? Direction.North : Direction.South;
        }
    }

    /// <summary>
    /// Converts a <see cref="Direction"/> enum to its corresponding <see cref="Vector2Int"/> offset.
    /// </summary>
    /// <param name="dir">The direction to convert.</param>
    /// <returns>
    /// <see cref="Vector2Int.up"/> for North,
    /// <see cref="Vector2Int.right"/> for East,
    /// <see cref="Vector2Int.down"/> for South,
    /// <see cref="Vector2Int.left"/> for West.
    /// Returns <see cref="Vector2Int.zero"/> for any undefined direction values.
    /// </returns>
    public static Vector2Int ToVector2Int(Direction dir)
    {
        return dir switch
        {
            Direction.North => Vector2Int.up,
            Direction.East => Vector2Int.right,
            Direction.South => Vector2Int.down,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero,// Fallback for undefined enum values
        };
    }
}
