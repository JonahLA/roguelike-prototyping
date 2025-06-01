using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;

public class BSPDungeonGenerator : MonoBehaviour
{
    [SerializeField] private BSPDungeonConfig config;

    [Header("Tilemap References")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;

    private BSPNode rootNode;
    private List<BSPNode> leafNodes = new();
    private List<(Vector2Int, Vector2Int)> corridors = new();

    public List<Rect> GetRooms()
    {
        return leafNodes
            .Where(leaf => leaf.room.HasValue)
            .Select(leaf => leaf.room.Value)
            .ToList();
    }

    private Vector2 GetRoomCenter(Rect room) {
        return new Vector2(room.x + room.width / 2, room.y + room.height / 2);
    }

    public Vector2 GetRandomRoomPosition()
    {
        List<Rect> rooms = GetRooms();
        if (rooms.Count == 0) return Vector2.zero;

        int roomIndex = Random.Range(0, rooms.Count);
        return GetRoomCenter(rooms[roomIndex]);
    }

    public Vector2 GetFirstRoomPosition()
    {
        List<Rect> rooms = GetRooms();
        if (rooms.Count == 0) return Vector2.zero;

        return GetRoomCenter(rooms[0]);
    }

    public Vector2 GetFarthestRoomPosition()
    {
        List<Rect> rooms = GetRooms();
        if (rooms.Count == 0) return Vector2.zero;

        Vector2 firstRoomCenter = GetFirstRoomPosition();
        float maxDistance = 0;
        int farthestIndex = 0;

        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2 roomCenter = GetRoomCenter(rooms[i]);
            float distance = Vector2.Distance(firstRoomCenter, roomCenter);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestIndex = i;
            }
        }

        return GetRoomCenter(rooms[farthestIndex]);
    }

    public void SetConfiguration(BSPDungeonConfig newConfig)
    {
        config = newConfig;
    }

    public void Generate()
    {
        if (config.useRandomSeed)
            Random.InitState(System.Environment.TickCount);
        else
            Random.InitState(config.randomSeed);

        leafNodes.Clear();
        corridors.Clear();
        rootNode = new BSPNode(new Rect(0, 0, config.dungeonWidth, config.dungeonHeight));

        SplitNodeRecursive(rootNode, 0);
        CreateRooms();
        ConnectRooms();
        DebugDrawDungeon();
    }

    private void SplitNodeRecursive(BSPNode node, int depth)
    {
        // Base case: we've reached the maximum split depth
        if (depth >= config.splitIterations)
        {
            leafNodes.Add(node);
            return;
        }

        // Otherwise, try to split the node
        if (node.Split(config.minRoomSize, config.maxRoomSize))
        {
            // Continue recursing on children if split successful
            SplitNodeRecursive(node.left, depth + 1);
            SplitNodeRecursive(node.right, depth + 1);
        }
        else
            leafNodes.Add(node);  // node couldn't be split further -> it's a leaf
    }

    private void CreateRooms()
    {
        foreach (BSPNode leaf in leafNodes)
        {
            int roomWidth = Random.Range(config.minRoomSize,
                Mathf.Min((int)leaf.region.width - config.roomPadding * 2, config.maxRoomSize + 1));
            int roomHeight = Random.Range(config.minRoomSize,
                Mathf.Min((int)leaf.region.height - config.roomPadding * 2, config.maxRoomSize + 1));

            int roomX = (int)leaf.region.x + config.roomPadding +
                Random.Range(0, (int)leaf.region.width - roomWidth - config.roomPadding * 2 + 1);
            int roomY = (int)leaf.region.y + config.roomPadding +
                Random.Range(0, (int)leaf.region.height - roomHeight - config.roomPadding * 2 + 1);

            leaf.room = new Rect(roomX, roomY, roomWidth, roomHeight);
        }
    }

    private void ConnectRooms()
    {
        ConnectNodesRooms(rootNode); // start the recursive connection process from the root node
    }

    private void ConnectNodesRooms(BSPNode node)
    {
        // Base case: we've reached a leaf node
        if (node.IsLeaf) return;

        ConnectNodesRooms(node.left);
        ConnectNodesRooms(node.right);

        CreateHallway(GetRoomFromSubtree(node.left), GetRoomFromSubtree(node.right));
    }

    private Rect GetRoomFromSubtree(BSPNode node)
    {
        if (node.IsLeaf && node.room.HasValue) return node.room.Value;

        if (!node.IsLeaf)
        {
            if (node.left != null)
            {
                Rect leftRoom = GetRoomFromSubtree(node.left);
                if (leftRoom.width > 0) return leftRoom;
            }

            if (node.right != null) return GetRoomFromSubtree(node.right);
        }

        return new Rect(0, 0, 0, 0);  // no room found, return an invalid rect
    }

    private void CreateHallway(Rect roomA, Rect roomB)
    {
        Vector2 centerA = new(roomA.x + roomA.width / 2, roomA.y + roomA.height / 2);
        Vector2 centerB = new(roomB.x + roomB.width / 2, roomB.y + roomB.height / 2);

        bool useLShaped = Random.value > 0.3f;  // 70% L-shaped, 30% straight

        if (useLShaped)
        {
            Vector2 corner = Random.value > 0.5f
                ? new Vector2(centerA.x, centerB.y)
                : new Vector2(centerB.x, centerA.y);

            corridors.Add((Vector2Int.FloorToInt(centerA), Vector2Int.FloorToInt(corner)));
            corridors.Add((Vector2Int.FloorToInt(corner), Vector2Int.FloorToInt(centerB)));
        }
        else
        {
            corridors.Add((Vector2Int.FloorToInt(centerA), Vector2Int.FloorToInt(centerB)));
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || leafNodes == null || leafNodes.Count == 0) return;

        Gizmos.color = Color.green;
        foreach (BSPNode leaf in leafNodes)
        {
            if (leaf.room.HasValue)
            {
                Rect room = leaf.room.Value;
                Gizmos.DrawWireCube(
                    new UnityEngine.Vector3(room.x + room.width / 2, room.y + room.height / 2, 0),
                    new UnityEngine.Vector3(room.width, room.height, 0)
                );
            }
        }

        Gizmos.color = Color.yellow;
        foreach (var corridor in corridors)
        {
            Gizmos.DrawLine(
                new UnityEngine.Vector3(corridor.Item1.x, corridor.Item1.y, 0),
                new UnityEngine.Vector3(corridor.Item2.x, corridor.Item2.y, 0)
            );
        }
    }

    private void DebugDrawDungeon()
    {
        if (floorTilemap == null || wallTilemap == null) return;

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        HashSet<Vector2Int> floorPositions = new();

        foreach (BSPNode leaf in leafNodes)
        {
            if (leaf.room.HasValue)
            {
                Rect room = leaf.room.Value;
                for (int x = (int)room.x; x < (int)(room.x + room.width); x++)
                {
                    for (int y = (int)room.y; y < (int)(room.y + room.height); y++)
                    {
                        floorPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        foreach (var corridor in corridors)
        {
            CreateCorridorTiles(corridor.Item1, corridor.Item2, floorPositions);
        }

        foreach (Vector2Int pos in floorPositions)
        {
            floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), floorTile);
        }

        PlaceWalls(floorPositions);
    }

    private void CreateCorridorTiles(Vector2Int start, Vector2Int end, HashSet<Vector2Int> floorPositions)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        
        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        if (steps == 0) return;
        
        float xStep = dx / (float)steps;
        float yStep = dy / (float)steps;
        
        float x = start.x;
        float y = start.y;
        
        for (int i = 0; i <= steps; i++)
        {
            int corridorHalfWidth = config.corridorWidth / 2;
            
            for (int w = -corridorHalfWidth; w <= corridorHalfWidth; w++)
            {
                if (Mathf.Abs(dx) > Mathf.Abs(dy))
                    floorPositions.Add(new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y) + w));
                else
                    floorPositions.Add(new Vector2Int(Mathf.RoundToInt(x) + w, Mathf.RoundToInt(y)));
            }
            
            x += xStep;
            y += yStep;
        }
    }

    private void PlaceWalls(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        
        foreach (Vector2Int pos in floorPositions)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip center
                    
                    Vector2Int checkPos = new Vector2Int(pos.x + x, pos.y + y);
                    if (!floorPositions.Contains(checkPos))
                    {
                        wallPositions.Add(checkPos);
                    }
                }
            }
        }
        
        foreach (Vector2Int pos in wallPositions)
        {
            wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), wallTile);
        }
    }
}
