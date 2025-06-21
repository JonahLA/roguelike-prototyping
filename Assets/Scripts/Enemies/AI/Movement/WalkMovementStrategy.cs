using UnityEngine;

[CreateAssetMenu(fileName = "WalkMovementStrategy", menuName = "Flare/Enemies/Movement/Walk")]
public class WalkMovementStrategy : MovementStrategySO
{
    public override void Move(Enemy enemy, Rigidbody2D rb, Vector2 target)
    {
        Vector2 direction = (target - (Vector2)enemy.transform.position).normalized;
        rb.MovePosition(rb.position + direction * enemy.Stats.moveSpeed * Time.fixedDeltaTime);
    }
}
