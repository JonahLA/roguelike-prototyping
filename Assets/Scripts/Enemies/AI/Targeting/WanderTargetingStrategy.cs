using UnityEngine;

[CreateAssetMenu(fileName = "WanderTargetingStrategy", menuName = "Flare/Enemies/Targeting/Wander")]
public class WanderTargetingStrategy : TargetingStrategySO
{
    public override Vector2 GetTarget(Enemy enemy, Transform playerTransform)
    {
        Vector2 randomPoint = (Random.insideUnitCircle * enemy.Stats.wanderRadius) + (Vector2)enemy.transform.position;
        return randomPoint;
    }
}
