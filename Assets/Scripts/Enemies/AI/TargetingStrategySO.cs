using UnityEngine;

public abstract class TargetingStrategySO : ScriptableObject
{
    public abstract Vector2 GetTarget(Transform enemyTransform, Transform playerTransform);
}
