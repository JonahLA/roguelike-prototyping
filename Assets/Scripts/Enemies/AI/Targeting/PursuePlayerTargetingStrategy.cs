using UnityEngine;

[CreateAssetMenu(fileName = "PursuePlayerTargetingStrategy", menuName = "Flare/Enemies/Targeting/Pursue Player")]
public class PursuePlayerTargetingStrategy : TargetingStrategySO
{
    public override Vector2 GetTarget(Enemy enemy, Transform playerTransform)
    {
        if (playerTransform != null)
            return playerTransform.position;

        return enemy.transform.position; // Default to staying put if no player
    }
}
