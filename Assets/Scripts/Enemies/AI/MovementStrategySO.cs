using UnityEngine;

public abstract class MovementStrategySO : ScriptableObject
{
    public abstract void Move(Enemy enemy, Rigidbody2D rb, Vector2 target);
}
