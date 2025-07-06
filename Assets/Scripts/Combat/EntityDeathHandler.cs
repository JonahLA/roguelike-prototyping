using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles entity death logic in a modular, event-driven way. Disables gameplay scripts, hides sprites, spawns VFX, and destroys or deactivates the GameObject.
/// Inspector-driven for flexibility.
/// </summary>
[AddComponentMenu("Combat/Entity Death Handler")]
public class EntityDeathHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Health component to subscribe to. If null, will search on this GameObject.")]
    [SerializeField] private Health _health;

    [Tooltip("Scripts to disable on death (e.g., AI, movement, combat, input, etc.)")]
    [SerializeField] private List<MonoBehaviour> _scriptsToDisable = new();

    [Tooltip("SpriteRenderers to hide on death (optional)")]
    [SerializeField] private List<SpriteRenderer> _spritesToHide = new();

    [Header("VFX & Events")]
    [Tooltip("VFX prefab to spawn on death (optional)")]
    [SerializeField] private GameObject _deathVFXPrefab;

    [Tooltip("Custom UnityEvents to invoke on death (e.g., loot drop, UI)")]
    public UnityEvent OnDeathEvents;

    [Header("Cleanup Options")]
    [Tooltip("Delay (seconds) before destroy/deactivate. Allows VFX/animation to play.")]
    [SerializeField] private readonly float _cleanupDelay = 0.5f;

    [Tooltip("If true, destroys the GameObject. If false, deactivates it (for pooling support).")]
    [SerializeField] private readonly bool _destroyOnDeath = false;

    private bool _hasHandledDeath = false;

    private void Awake()
    {
        _health ??= GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDeath.AddListener(HandleDeath);
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDeath.RemoveListener(HandleDeath);
    }

    private void HandleDeath()
    {
        if (_hasHandledDeath) return;
        _hasHandledDeath = true;

        // Disable gameplay scripts
        foreach (var script in _scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Hide sprites
        foreach (var sr in _spritesToHide)
        {
            if (sr != null)
                sr.enabled = false;
        }

        // Spawn VFX
        if (_deathVFXPrefab != null && VFXSpawner.Instance != null)
        {
            VFXSpawner.Instance.SpawnVFX(_deathVFXPrefab, transform.position, transform.rotation);
        }

        // Invoke custom events
        OnDeathEvents?.Invoke();

        // Cleanup (destroy or deactivate)
        StartCoroutine(CleanupAfterDelay());
    }

    private IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(_cleanupDelay);
        if (_destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
