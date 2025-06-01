using UnityEngine;

/// <summary>
/// A node in the BSP tree, tracking its assigned region and any children.
/// </summary>
public class BSPNode
{
    public Rect region;
    public BSPNode left;
    public BSPNode right;

    public Rect? room;  // optional room bounds carved inside this node (only for leaves)

    public BSPNode(Rect region)
    {
        this.region = region;
    }

    /// <summary>
    /// True when this node has not been split further.
    /// </summary>
    public bool IsLeaf => left == null && right == null;

    /// <summary>
    /// Splits the node into two either vertically or horizontally
    /// </summary>
    public bool Split(int minSize, int maxSize)
    {
        // Don't split if this isn't a leaf
        if (!IsLeaf) return false;

        // Don't split if the region is too small for two minimum-sized rooms
        if (region.width < minSize * 2 && region.height < minSize * 2)
            return false;

        // Check if we MUST split (region is larger than what would allow a valid room)
        bool mustSplit = region.width > maxSize * 2 || region.height > maxSize * 2;
        
        // If region is of "reasonable" size and we don't NEED to split, randomly decide
        if (!mustSplit && region.width <= maxSize * 1.25f && region.height <= maxSize * 1.25f)
        {
            // 50% chance to stop splitting when in the "sweet spot" size range
            if (Random.value > 0.5f)
                return false;
        }

        // Determine split orientation
        float ratio = region.width / region.height;
        bool splitHoriz;
        
        if (ratio > 1.25f)
            splitHoriz = false;  // much wider than tall -> vertical split
        else if (ratio < 1f / 1.25f)
            splitHoriz = true;   // much taller than wide -> horizontal split
        else
            splitHoriz = Random.value > 0.5f;  // roughly square -> random

        // Calculate valid range for split
        int minSplitSize = minSize;
        int maxSplitSize = splitHoriz ? (int)region.height - minSize : (int)region.width - minSize;
        
        if (maxSplitSize <= minSplitSize)
            return false;
            
        int split = Random.Range(minSplitSize, maxSplitSize + 1);

        if (splitHoriz)
        {
            left  = new BSPNode(new Rect(region.x, region.y,         region.width, split));
            right = new BSPNode(new Rect(region.x, region.y + split, region.width, region.height - split));
        }
        else
        {
            left  = new BSPNode(new Rect(region.x,         region.y, split,                region.height));
            right = new BSPNode(new Rect(region.x + split, region.y, region.width - split, region.height));
        }
        return true;
    }
}
