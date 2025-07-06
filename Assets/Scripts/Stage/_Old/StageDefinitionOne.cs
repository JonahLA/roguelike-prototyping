using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flare/Stage Definition", fileName = "NewStageDefinition")]
public class StageDefinition : ScriptableObject
{
    [Tooltip("List of room prefabs used for stage generation.")]
    public List<GameObject> roomPrefabs = new List<GameObject>();

    [Tooltip("Maximum number of rooms to spawn in a stage.")]
    public int maxRooms = 5;

    [Tooltip("Maximum number of branching connections per room.")]
    public int maxBranching = 2;

    [Tooltip("Placeholder floor tile prefab for filling gaps.")]
    public GameObject floorTilePrefab;

    [Tooltip("Prefab used for the player.")]
    public GameObject playerPrefab;
}
