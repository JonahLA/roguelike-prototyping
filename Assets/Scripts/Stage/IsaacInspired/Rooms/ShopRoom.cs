using UnityEngine;

public class ShopRoom : Room
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        // Logic for shop interaction
        Debug.Log($"ShopRoom {gameObject.name}: Player entered. Implement shop UI/logic.");
        if (!isCleared) OnRoomClear(); // Shop rooms are often cleared on entry
    }
}
