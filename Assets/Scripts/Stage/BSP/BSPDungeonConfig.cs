using UnityEngine;

[CreateAssetMenu(fileName = "BSPDungeonConfig", menuName = "Roguelike/Dungeon Config/BSP")]
public class BSPDungeonConfig : ScriptableObject
{
    [Header("BSP Parameters")]
    [Min(20)] public int dungeonWidth = 100;
    [Min(20)] public int dungeonHeight = 100;

    [Tooltip("Minimum size of any room")]
    [Range(4, 30)] public int minRoomSize = 8;

    [Tooltip("Maximum size of any room")]
    [Range(10, 50)] public int maxRoomSize = 20;

    [Tooltip("Number of recursive splits to perform")]
    [Range(1, 10)] public int splitIterations = 4;

    [Tooltip("Width of corridors connecting rooms")]
    [Range(1, 5)] public int corridorWidth = 3;

    [Tooltip("Padding between room edges and region boundaries")]
    [Range(0, 10)] public int roomPadding = 1;

    [Tooltip("Fixed seed for reproducible generation (0 = random)")]
    public int randomSeed = 0;

    [Tooltip("Whether to use a new random seed each time")]
    public bool useRandomSeed = true;

    private void OnValidate()
    {
        if (maxRoomSize <= minRoomSize)
            maxRoomSize = minRoomSize + 4;

        int minDungeonSize = maxRoomSize * 3;
        dungeonWidth  = Mathf.Max(dungeonWidth, minDungeonSize);
        dungeonHeight = Mathf.Max(dungeonHeight, minDungeonSize);
    }
}
