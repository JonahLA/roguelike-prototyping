using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Targeting strategy that selects the healthiest enemy (with the most health).
/// </summary>
[CreateAssetMenu(fileName = "HealthiestEnemyTargeting", menuName = "Flare/Cards/Targeting/HealthiestEnemy")]
public class HealthiestEnemyTargetingSO : CardTargetingStrategySO
{
    /// <summary>
    /// Returns a list containing the healthiest enemy GameObject, or an empty list if none found.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <returns>A list with the healthiest enemy GameObject, or empty if no enemies exist.</returns>
    public override List<GameObject> GetTargets(GameObject user)
    {
        // TODO: Replace with your own enemy/health system
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject healthiest = null;
        float maxHealth = float.MinValue;
        foreach (var enemy in enemies)
        {
            var health = enemy.GetComponent<HealthComponent>();
            if (health != null && health.CurrentHealth > maxHealth)
            {
                maxHealth = health.CurrentHealth;
                healthiest = enemy;
            }
        }
        return healthiest != null ? new List<GameObject> { healthiest } : new List<GameObject>();
    }
}
