using UnityEngine;

public class TreasureRoom : Room
{
    // Example: Item spawning logic
    // public GameObject treasurePrefab;
    // private bool _treasureSpawned = false;

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        // if (!isCleared && !_treasureSpawned)
        // {
        //     // SpawnTreasure();
        //     // OnRoomClear(); // Or clear after item is picked up
        //    _treasureSpawned = true;
        // }
        Debug.Log($"TreasureRoom {gameObject.name}: Player entered. Implement treasure spawning.");
        if(!isCleared) OnRoomClear(); // Treasure rooms are often cleared on entry or after taking item
    }
    // private void SpawnTreasure() { ... }
}
