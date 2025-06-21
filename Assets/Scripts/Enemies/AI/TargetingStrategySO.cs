using UnityEngine;

public abstract class TargetingStrategySO : ScriptableObject
{
    public abstract Vector2 GetTarget(Enemy enemy, Transform playerTransform);
}
