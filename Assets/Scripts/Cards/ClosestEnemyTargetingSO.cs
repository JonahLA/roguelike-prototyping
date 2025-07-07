using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Targeting strategy that selects the closest enemy to the user.
/// </summary>
[CreateAssetMenu(fileName = "ClosestEnemyTargeting", menuName = "Flare/Cards/Targeting/ClosestEnemy")]
public class ClosestEnemyTargetingSO : CardTargetingStrategySO
{
    /// <summary>
    /// Returns a list containing the closest enemy GameObject to the user, or an empty list if none found.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <returns>A list with the closest enemy GameObject, or empty if no enemies exist.</returns>
    public override List<GameObject> GetTargets(GameObject user)
    {
        // TODO: Replace with your own enemy management system
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(user.transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest != null ? new List<GameObject> { closest } : new List<GameObject>();
    }
}
