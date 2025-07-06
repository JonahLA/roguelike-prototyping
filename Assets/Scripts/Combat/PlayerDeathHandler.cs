using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player-specific death logic. Disables input, triggers death screen, and extends EntityDeathHandler.
/// </summary>
[AddComponentMenu("Combat/Player Death Handler")]
public class PlayerDeathHandler : EntityDeathHandler
{
    [Header("Player-Specific References")]
    [Tooltip("Input scripts to disable on death (e.g., PlayerInput, custom input handlers)")]
    [SerializeField] private MonoBehaviour[] _inputScriptsToDisable;

    [Tooltip("Name of the death screen scene to load additively.")]
    [SerializeField] private string _deathScreenSceneName = "DeathScreen";

    protected override void HandleDeath()
    {
        base.HandleDeath();

        // Disable input scripts
        foreach (var script in _inputScriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Load death screen scene additively (placeholder)
        if (!string.IsNullOrEmpty(_deathScreenSceneName))
        {
            SceneManager.LoadSceneAsync(_deathScreenSceneName, LoadSceneMode.Additive);
        }
    }
}
