using UnityEngine;

public abstract class MovementStrategySO : ScriptableObject
{
    public abstract void Move(Transform enemyTransform, Vector2 target, float moveSpeed);
}
