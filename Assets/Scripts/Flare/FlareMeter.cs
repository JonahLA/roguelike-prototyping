using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UnityEvent for flare value changes. Passes (currentFlareInt, maxFlare, currentFlareFloat).
/// </summary>
[Serializable]
public class FlareChangedEvent : UnityEvent<int, int, float> { }

/// <summary>
/// Tracks and manages the player's Flare resource, including regeneration, upgrades, and event notification.
/// </summary>
/// <remarks>
/// - Flare increases at a configurable rate per second (can be modified by items/cards).
/// - Supports instant flare gain (e.g., from card effects or pickups).
/// - Exposes both C# and UnityEvent notifications for UI and other systems.
/// - Flare persists between rooms, but resets to max at the start of each stage.
/// - Attach this MonoBehaviour to the player object.
/// </remarks>
public class FlareMeter : MonoBehaviour
{
    [Header("Flare Settings")]
    [Tooltip("The maximum amount of flare the player can have.")]
    [SerializeField] private int _maxFlare = 5;

    [Tooltip("The current amount of flare (float, for smooth UI fill). Initialized to maxFlare at start.")]
    [SerializeField] private float _currentFlare = 0f;

    [Tooltip("The rate at which flare regenerates per second.")]
    [SerializeField] private float _flareRegenRate = 1f;

    /// <summary>
    /// The maximum amount of flare the player can have.
    /// </summary>
    public int MaxFlare => _maxFlare;

    /// <summary>
    /// The current flare value as an integer (for discrete UI sections).
    /// </summary>
    public int CurrentFlare => Mathf.FloorToInt(_currentFlare);

    /// <summary>
    /// The current flare value as a float (for smooth UI sub-bar).
    /// </summary>
    public float CurrentFlareFloat => _currentFlare;

    /// <summary>
    /// The rate at which flare regenerates per second.
    /// </summary>
    public float FlareRegenRate => _flareRegenRate;

    /// <summary>
    /// C# event for flare value changes. Passes (currentFlareInt, maxFlare, currentFlareFloat).
    /// </summary>
    public event Action<int, int, float> OnFlareChanged;

    /// <summary>
    /// UnityEvent for flare value changes. Passes (currentFlareInt, maxFlare, currentFlareFloat).
    /// </summary>
    public FlareChangedEvent OnFlareChangedUnityEvent;

    private void Awake()
    {
        OnFlareChangedUnityEvent ??= new FlareChangedEvent();
    }

    private void Start()
    {
        _currentFlare = _maxFlare;
        NotifyFlareChanged();
    }

    private void Update()
    {
        _currentFlare = Mathf.Min(_currentFlare + _flareRegenRate * Time.deltaTime, _maxFlare);
        NotifyFlareChanged();
    }

    /// <summary>
    /// Instantly adds the specified amount of flare.
    /// </summary>
    /// <param name="amount">The amount of flare to add (can be negative).</param>
    public void AddFlare(float amount)
    {
        float prev = _currentFlare;
        _currentFlare = Mathf.Clamp(_currentFlare + amount, 0, _maxFlare);
        if (!Mathf.Approximately(_currentFlare, prev))
            NotifyFlareChanged();
    }

    /// <summary>
    /// Attempts to spend the specified amount of flare (integer, for card costs).
    /// </summary>
    /// <param name="amount">The amount of flare to spend.</param>
    /// <returns>True if enough flare was available and spent; false otherwise.</returns>
    public bool SpendFlare(int amount)
    {
        if (CurrentFlare < amount)
            return false;
        _currentFlare -= amount;
        _currentFlare = Mathf.Max(_currentFlare, 0);
        NotifyFlareChanged();
        return true;
    }

    /// <summary>
    /// Sets the current flare value directly.
    /// </summary>
    /// <param name="value">The new flare value.</param>
    public void SetFlare(float value)
    {
        _currentFlare = Mathf.Clamp(value, 0, _maxFlare);
        NotifyFlareChanged();
    }

    /// <summary>
    /// Sets the maximum flare value. Optionally refills current flare to max.
    /// </summary>
    /// <param name="value">The new maximum flare value.</param>
    /// <param name="refill">If true, refill current flare to new max.</param>
    public void SetMaxFlare(int value, bool refill = false)
    {
        _maxFlare = Mathf.Max(1, value);
        if (refill) _currentFlare = _maxFlare;
        else _currentFlare = Mathf.Clamp(_currentFlare, 0, _maxFlare);
        NotifyFlareChanged();
    }

    /// <summary>
    /// Sets the flare regeneration rate (per second).
    /// </summary>
    /// <param name="rate">The new regeneration rate.</param>
    public void SetRegenRate(float rate)
    {
        _flareRegenRate = Mathf.Max(0f, rate);
    }

    /// <summary>
    /// Resets the current flare to the maximum value.
    /// </summary>
    public void ResetFlareToMax()
    {
        _currentFlare = _maxFlare;
        NotifyFlareChanged();
    }

    /// <summary>
    /// Notifies listeners of a flare value change.
    /// </summary>
    private void NotifyFlareChanged()
    {
        OnFlareChanged?.Invoke(CurrentFlare, _maxFlare, _currentFlare);
        OnFlareChangedUnityEvent?.Invoke(CurrentFlare, _maxFlare, _currentFlare);
    }
}
