using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player-specific death logic. Disables input, triggers death screen, and extends EntityDeathHandler.
/// </summary>
[AddComponentMenu("Combat/Player Death Handler")]
public class PlayerDeathHandler : EntityDeathHandler
{

    [Tooltip("Name of the death screen scene to load additively.")]
    [SerializeField] private string _deathScreenSceneName = "DeathScreen";

    protected override void HandleDeath()
    {
        base.HandleDeath();

        // Load death screen scene additively (placeholder)
        if (!string.IsNullOrEmpty(_deathScreenSceneName))
        {
            SceneManager.LoadSceneAsync(_deathScreenSceneName, LoadSceneMode.Additive);
        }
    }
}
