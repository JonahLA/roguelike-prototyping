using System;
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

    [Header("Animation & Fade-Out")]
    [Tooltip("Animator to trigger death animation (optional)")]
    [SerializeField] private Animator _animator;

    [Tooltip("Trigger name for death animation (if Animator is assigned)")]
    [SerializeField] private string _deathTrigger = "Death";

    [Tooltip("If true, fade out sprites if no animation is present")]
    [SerializeField] private readonly bool _fadeOutIfNoAnimation = false;

    [Tooltip("Fade out duration (seconds)")]
    [SerializeField] private readonly float _fadeOutDuration = 0.5f;

    [Header("Loot Drop Events")]
    [Tooltip("Invoked when this entity should drop loot (for enemies). Designers can hook up loot logic here.")]
    public UnityEvent OnLootDrop;

    /// <summary>
    /// C# event for loot drop. Subscribe in code to handle loot drops (e.g., LootManager).
    /// </summary>
    public static event Action<EntityDeathHandler> OnLootDropEvent;

    private bool _hasHandledDeath = false;

    private void Awake()
    {
        _health = _health != null ? _health : GetComponent<Health>();
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

    protected virtual void HandleDeath()
    {
        if (_hasHandledDeath) return;
        _hasHandledDeath = true;

        // Disable gameplay scripts
        foreach (var script in _scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Animation or fade-out logic
        if (_animator != null && !string.IsNullOrEmpty(_deathTrigger))
        {
            _animator.SetTrigger(_deathTrigger);
            float animLength = GetDeathAnimationLength();
            StartCoroutine(WaitAndCleanup(animLength > 0 ? animLength : _cleanupDelay));
        }
        else if (_fadeOutIfNoAnimation && _spritesToHide.Count > 0)
        {
            StartCoroutine(FadeOutAndCleanup());
        }
        else
        {
            // Hide sprites immediately
            foreach (var sr in _spritesToHide)
            {
                if (sr != null)
                    sr.enabled = false;
            }
            StartCoroutine(WaitAndCleanup(_cleanupDelay));
        }

        // Spawn VFX
        if (_deathVFXPrefab != null && VFXSpawner.Instance != null)
        {
            VFXSpawner.Instance.SpawnVFX(_deathVFXPrefab, transform.position, transform.rotation);
        }

        // Invoke custom events
        OnDeathEvents?.Invoke();

        // Loot drop events (only for enemies, not player)
        if (this is not PlayerDeathHandler)
        {
            OnLootDrop?.Invoke();
            OnLootDropEvent?.Invoke(this);
        }
    }

    private float GetDeathAnimationLength()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return 0f;
        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == _deathTrigger)
                return clip.length;
        }
        return 0f;
    }

    private IEnumerator WaitAndCleanup(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndCleanup()
    {
        float elapsed = 0f;
        List<Color> originalColors = new List<Color>();
        foreach (var sr in _spritesToHide)
            originalColors.Add(sr != null ? sr.color : Color.white);

        while (elapsed < _fadeOutDuration)
        {
            float t = 1f - (elapsed / _fadeOutDuration);
            for (int i = 0; i < _spritesToHide.Count; i++)
            {
                if (_spritesToHide[i] != null)
                {
                    Color c = originalColors[i];
                    c.a = t;
                    _spritesToHide[i].color = c;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Hide sprites at end
        foreach (var sr in _spritesToHide)
        {
            if (sr != null)
                sr.enabled = false;
        }
        yield return new WaitForSeconds(_cleanupDelay);
        if (_destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
