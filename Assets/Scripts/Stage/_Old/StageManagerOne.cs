using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [Header("Stage Definition")]
    public StageDefinition stageDefinition;

    [Header("Parents")]
    public Transform roomsParent;
    public Transform floorParent;

    private class RoomInstance
    {
        public GameObject go;
        public Vector2Int cell;
        public List<string> freeAnchors;
    }

    // maps grid cell -> placed room
    private Dictionary<Vector2Int, RoomInstance> placed = new Dictionary<Vector2Int, RoomInstance>();

    private void Start()
    {
        GenerateStage();
    }

    /// <summary>
    /// Orchestrates the steps of procedural stage creation.
    /// </summary>
    public void GenerateStage()
    {
        PlaceRooms();
        FillFloor();
        SpawnPlayer();
    }

    /// <summary>
    /// Instantiate room prefabs and snap them together via door anchors.
    /// </summary>
    private void PlaceRooms()
    {
        // 1) Place the entry room at (0, 0)
        var entryPrefab = stageDefinition.roomPrefabs[Random.Range(0, stageDefinition.roomPrefabs.Count)];
        var entryGO = Instantiate(entryPrefab, Vector3.zero, Quaternion.identity, roomsParent);
        var entry = new RoomInstance
        {
            go          = entryGO,
            cell        = Vector2Int.zero,
            freeAnchors = GetAnchors(entryGO)
        };
        placed.Add(entry.cell, entry);

        // 2) Expand until we hit maxRooms
        while (placed.Count < stageDefinition.maxRooms)
        {
            // pick a random room with free anchors
            var candidates = placed.Values.Where(r => r.freeAnchors.Count > 0).ToList();
            if (candidates.Count == 0)
            {
                Debug.Log("PlaceRooms: All rooms full - - finishing placement");
                break;
            }
            var parent = candidates[Random.Range(0, candidates.Count)];

            // pick one of its free anchors
            string dirName = parent.freeAnchors[Random.Range(0, parent.freeAnchors.Count)];
            Vector2Int dir = DirFromAnchor(dirName);
            Vector2Int newCell = parent.cell + dir;
            if (placed.ContainsKey(newCell))
            {
                parent.freeAnchors.Remove(dirName);
                continue;
            }

            // determine the opposite anchor name we need on the child
            string neededOpp = Opposite(dirName);

            // filter prefabs that have Door_{neededOpp}
            var validPrefabs = stageDefinition.roomPrefabs
                .Where(pf => FindAnchor(pf, $"Door_{neededOpp}") != null)
                .ToList();
            if (validPrefabs.Count == 0)
            {
                Debug.LogWarning($"PlaceRooms: No room prefab has a Door_{neededOpp} anchor; skipping parent anchor {dirName}");
                parent.freeAnchors.Remove(dirName);
                continue;
            }

            // pick from the valid set
            var prefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
            var newGO  = Instantiate(prefab, roomsParent);

            // find the Transforms
            var parentAnchor = FindAnchor(parent.go, $"Door_{dirName}");
            var newAnchor    = FindAnchor(newGO,     $"Door_{neededOpp}");
            newGO.transform.position = parentAnchor.position - (newAnchor.position - newGO.transform.position);

            // record it
            var inst = new RoomInstance
            {
                go          = newGO,
                cell        = newCell,
                freeAnchors = GetAnchors(newGO)
            };

            // remove the two anchors we just used
            inst.freeAnchors.Remove(neededOpp);
            parent.freeAnchors.Remove(dirName);
            placed.Add(newCell, inst);
        }
    }

    /// <summary>
    /// Fill any empty grid cells with floor-tile prefabs.
    /// </summary>
    private void FillFloor()
    {
        if (stageDefinition.floorTilePrefab == null)
        {
            Debug.LogError("FillFloor: floorTilePrefab is not set on StageDefinition.");
            return;
        }

        // compute bounds
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        Debug.Log(placed.Keys.ToCommaSeparatedString());

        foreach (var cell in placed.Keys)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
        }

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                if (!placed.ContainsKey(new Vector2Int(x, y)))
                    Instantiate(
                        stageDefinition.floorTilePrefab,
                        new Vector3(x, y, 0f),
                        Quaternion.identity,
                        floorParent
                    );
    }

    /// <summary>
    /// Spawn the player at the designated entry door anchor.
    /// </summary>
    private void SpawnPlayer()
    {
        if (stageDefinition.playerPrefab == null)
        {
            Debug.LogError("SpawnPlayer: playerPrefab is not set on StageDefinition.");
            return;
        }

        if (!placed.TryGetValue(Vector2Int.zero, out var entryInst))
        {
            Debug.LogError("SpawnPlayer: No room found at grid (0, 0). Cannot spawn player.");
            return;
        }

        Transform spawnAnchor = FindAnchor(entryInst.go, "Door_South") ?? entryInst.go.transform;
        Vector2 spawnPos = spawnAnchor.position;
        GameObject player = Instantiate(stageDefinition.playerPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"SpawnPlayer: Instantiated player at {spawnPos}.");

        var mainCam = Camera.main;
        if (mainCam != null)
        {
            var camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.target = player.transform;
            }
            else
            {
                Debug.LogWarning("SpawnPlayer: Main Camera is missing a CameraFollow component.");
            }
        }
        else
        {
            Debug.LogWarning("SpawnPlayer: No Main Camera found in scene.");
        }
    }

    /// <summary>
    /// Finds every child Transform (at any depth) whose name starts with "Door_",
    /// and returns just the anchor suffixes ("North", "South", etc.).
    /// </summary>
    private List<string> GetAnchors(GameObject room)
    {
        return room
            .GetComponentsInChildren<Transform>(true)
            .Where(t => t.name.StartsWith("Door_"))
            .Select(t => t.name.Substring("Door_".Length))
            .ToList();
    }

    /// <summary>
    /// Recursively finds the first transform named exactly anchorName.
    /// </summary>
    private Transform FindAnchor(GameObject room, string anchorName)
    {
        return room
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == anchorName);
    }

    private Vector2Int DirFromAnchor(string d)
    {
        switch (d)
        {
            case "North": return Vector2Int.up;
            case "South": return Vector2Int.down;
            case "East": return Vector2Int.right;
            case "West": return Vector2Int.left;
        }
        return Vector2Int.zero;
    }

    private string Opposite(string d)
    {
        switch (d)
        {
            case "North": return "South";
            case "South": return "North";
            case "East": return "West";
            case "West": return "East";
        }
        return "";
    }
}
