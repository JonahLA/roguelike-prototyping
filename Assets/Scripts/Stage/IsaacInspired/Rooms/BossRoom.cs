using UnityEngine;

public class BossRoom : Room
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (!isCleared)
        {
            // LockDoors();
            Debug.Log($"BossRoom {gameObject.name}: Player entered, room not cleared. Implement boss fight/door locking.");
        }
    }
    // private void LockDoors() { ... }
}
