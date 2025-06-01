using UnityEngine;

public class StartRoom : Room
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        // Start rooms are typically cleared by default or have no enemies
        if (!isCleared)
        {
            OnRoomClear(); 
        }
    }
}
