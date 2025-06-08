using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages spawning and pooling of damage numbers for performance.
/// Handles automatic spawning when damage events occur.
/// </summary>
public class DamageNumberSpawner : MonoBehaviour
{
    [Header("Prefab Reference")]
    [Tooltip("Damage number prefab to spawn")]
    [SerializeField]
    private DamageNumber _damageNumberPrefab;
    
    [Header("Pool Settings")]
    [Tooltip("Initial number of damage numbers to pre-instantiate")]
    [SerializeField, Range(5, 50)]
    private int _poolSize = 20;
    
    [Tooltip("Maximum number of damage numbers that can exist at once")]
    [SerializeField, Range(10, 100)]
    private int _maxPoolSize = 50;
    
    [Header("Spawn Settings")]
    [Tooltip("Vertical offset from entity position")]
    [SerializeField, Range(0f, 2f)]
    private float _spawnHeightOffset = 0.5f;
    
    [Tooltip("Random position variance around spawn point")]
    [SerializeField, Range(0f, 1f)]
    private float _spawnPositionVariance = 0.2f;
    
    // Pool management
    private Queue<DamageNumber> _availableNumbers = new Queue<DamageNumber>();
    private HashSet<DamageNumber> _activeNumbers = new HashSet<DamageNumber>();
    
    // Singleton pattern for easy access
    public static DamageNumberSpawner Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple DamageNumberSpawner instances found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Validate prefab
        if (_damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumber prefab is not assigned!", this);
            return;
        }
        
        // Initialize pool
        InitializePool();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void InitializePool()
    {
        // Pre-instantiate damage numbers
        for (int i = 0; i < _poolSize; i++)
        {
            CreateNewDamageNumber();
        }
    }
    
    private DamageNumber CreateNewDamageNumber()
    {
        GameObject go = Instantiate(_damageNumberPrefab.gameObject, transform);
        DamageNumber damageNumber = go.GetComponent<DamageNumber>();
        damageNumber.ResetForPool();
        _availableNumbers.Enqueue(damageNumber);
        return damageNumber;
    }
    
    /// <summary>
    /// Spawn a damage number at the specified position.
    /// </summary>
    /// <param name="damage">Damage amount to display</param>
    /// <param name="worldPosition">World position to spawn at</param>
    /// <param name="isCritical">Whether this is a critical hit</param>
    /// <param name="isHealing">Whether this is healing instead of damage</param>
    public void SpawnDamageNumber(float damage, Vector3 worldPosition, bool isCritical = false, bool isHealing = false)
    {
        DamageNumber damageNumber = GetPooledDamageNumber();
        if (damageNumber == null)
        {
            Debug.LogWarning("Could not spawn damage number - pool exhausted and max size reached");
            return;
        }
        
        // Calculate spawn position with variance
        Vector3 spawnPosition = worldPosition + Vector3.up * _spawnHeightOffset;
        if (_spawnPositionVariance > 0f)
        {
            spawnPosition += new Vector3(
                Random.Range(-_spawnPositionVariance, _spawnPositionVariance),
                Random.Range(-_spawnPositionVariance * 0.5f, _spawnPositionVariance * 0.5f),
                0f
            );
        }
        
        // Initialize and activate
        damageNumber.gameObject.SetActive(true);
        damageNumber.Initialize(damage, spawnPosition, isCritical, isHealing, this);
        _activeNumbers.Add(damageNumber);
    }
    
    /// <summary>
    /// Get a damage number from the pool or create a new one if needed.
    /// </summary>
    private DamageNumber GetPooledDamageNumber()
    {
        // Try to get from available pool
        if (_availableNumbers.Count > 0)
        {
            return _availableNumbers.Dequeue();
        }
        
        // If pool is empty and we haven't reached max size, create new one
        if (_activeNumbers.Count + _availableNumbers.Count < _maxPoolSize)
        {
            return CreateNewDamageNumber();
        }
        
        // Pool exhausted
        return null;
    }
    
    /// <summary>
    /// Return a damage number to the pool for reuse.
    /// </summary>
    /// <param name="damageNumber">Damage number to return</param>
    public void ReturnToPool(DamageNumber damageNumber)
    {
        if (damageNumber == null) return;
        
        _activeNumbers.Remove(damageNumber);
        damageNumber.ResetForPool();
        _availableNumbers.Enqueue(damageNumber);
    }
    
    /// <summary>
    /// Static convenience method to spawn damage numbers from anywhere.
    /// </summary>
    public static void SpawnDamage(float damage, Vector3 worldPosition, bool isCritical = false, bool isHealing = false)
    {
        if (Instance != null)
        {
            Instance.SpawnDamageNumber(damage, worldPosition, isCritical, isHealing);
        }
        else
        {
            Debug.LogWarning("DamageNumberSpawner instance not found. Cannot spawn damage number.");
        }
    }
    
    /// <summary>
    /// Clear all active damage numbers (useful for scene transitions).
    /// </summary>
    public void ClearAllDamageNumbers()
    {
        // Return all active numbers to pool
        var activeNumbersCopy = new HashSet<DamageNumber>(_activeNumbers);
        foreach (var damageNumber in activeNumbersCopy)
        {
            damageNumber.ReturnToPool();
        }
    }
    
    // Debug information
    public int ActiveCount => _activeNumbers.Count;
    public int AvailableCount => _availableNumbers.Count;
    public int TotalPoolSize => _activeNumbers.Count + _availableNumbers.Count;
    
    #if UNITY_EDITOR
    [ContextMenu("Test Spawn Damage")]
    private void TestSpawnDamage()
    {
        SpawnDamageNumber(10f, transform.position + Vector3.up);
    }
    
    [ContextMenu("Test Spawn Critical")]
    private void TestSpawnCritical()
    {
        SpawnDamageNumber(25f, transform.position + Vector3.up, true);
    }
    
    [ContextMenu("Test Spawn Healing")]
    private void TestSpawnHealing()
    {
        SpawnDamageNumber(5f, transform.position + Vector3.up, false, true);
    }
    
    [ContextMenu("Clear All Numbers")]
    private void TestClearAll()
    {
        ClearAllDamageNumbers();
    }
    #endif
}
