using System.Collections.Generic;
using UnityEngine;

public class BSPStageManager : MonoBehaviour
{
    [SerializeField] private BSPDungeonGenerator dungeonGenerator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Camera playerCamera;

    // [Header("Gameplay Elements")]
    // [SerializeField] private List<GameObject> enemyPrefabs;
    // [SerializeField] private float enemyDensity = 0.1f;  // enemies per area unit

    private void Start()
    {
        GenerateStage();   
    }

    public void GenerateStage()
    {
        dungeonGenerator.Generate();
        Vector2 spawnPoint = GetPlayerSpawnPoint();
        GameObject player = SpawnPlayer(spawnPoint);
        SetupCamera(player);
        // PopulateRooms();
    }

    private Vector2 GetPlayerSpawnPoint()
    {
        return dungeonGenerator.GetRandomRoomPosition();
    }

    private GameObject SpawnPlayer(Vector2 position)
    {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        return player;
    }

    private void SetupCamera(GameObject player)
    {
        if (playerCamera != null)
        {
            var camFollow = playerCamera.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.target = player.transform;
            }
            else
            {
                Debug.LogWarning("SetupCamera: Player Camera is missing a CameraFollow component.");
            }
        }
        else
        {
            Debug.LogWarning("SetupCamera: No Player Camera set in BSPStageManager.");
        }
    }

    // private void PopulateRooms()
    // {
    //     // TODO: this
    // }
}
