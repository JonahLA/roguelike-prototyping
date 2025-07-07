using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Targeting strategy that selects all enemies in the scene.
/// </summary>
[CreateAssetMenu(fileName = "AllEnemiesTargeting", menuName = "Flare/Cards/Targeting/AllEnemies")]
public class AllEnemiesTargetingSO : CardTargetingStrategySO
{
    /// <summary>
    /// Returns a list of all enemy GameObjects in the scene.
    /// </summary>
    /// <param name="user">The GameObject using the card.</param>
    /// <returns>A list of all enemy GameObjects.</returns>
    public override List<GameObject> GetTargets(GameObject user)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return new List<GameObject>(enemies);
    }
}
