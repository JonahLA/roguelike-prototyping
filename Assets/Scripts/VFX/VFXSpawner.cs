using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A singleton manager for spawning and pooling visual effects (VFX) like particle systems.
/// This improves performance by reusing objects instead of instantiating and destroying them.
/// </summary>
public class VFXSpawner : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the VFXSpawner.
    /// </summary>
    public static VFXSpawner Instance { get; private set; }

    // A dictionary to hold a pool (Queue) for each type of VFX prefab.
    private readonly Dictionary<GameObject, Queue<GameObject>> _pool = new();
    // A dictionary to track the original prefab of each pooled instance.
    private readonly Dictionary<GameObject, GameObject> _prefabMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Spawns a VFX prefab from the pool at a specified position and rotation.
    /// </summary>
    /// <param name="prefab">The VFX GameObject to spawn.</param>
    /// <param name="position">The world position to spawn at.</param>
    /// <param name="rotation">The world rotation to spawn with.</param>
    /// <param name="parent">(Optional) The parent object for the effect.</param>
    /// <returns>The spawned VFX GameObject instance, or null if the prefab is invalid.</returns>
    public GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Attempted to spawn a null VFX prefab.");
            return null;
        }

        if (!_pool.ContainsKey(prefab))
        {
            _pool[prefab] = new Queue<GameObject>();
        }

        GameObject vfxInstance;
        if (_pool[prefab].Count > 0)
        {
            vfxInstance = _pool[prefab].Dequeue();
            vfxInstance.transform.SetPositionAndRotation(position, rotation);
            if (parent != null) vfxInstance.transform.SetParent(parent, worldPositionStays: true);
            vfxInstance.SetActive(true);
        }
        else
        {
            vfxInstance = Instantiate(prefab, position, rotation);
            _prefabMap[vfxInstance] = prefab; // Map the new instance to its prefab
        }

        return vfxInstance;
    }

    /// <summary>
    /// Returns a VFX instance to the pool. This is typically called automatically
    /// by the ParticleSystemAutoReturn script when an effect finishes.
    /// </summary>
    /// <param name="vfxInstance">The instance to return.</param>
    public void ReturnVFX(GameObject vfxInstance)
    {
        if (vfxInstance == null)
        {
            return;
        }

        if (_prefabMap.TryGetValue(vfxInstance, out GameObject prefab))
        {
            if (_pool.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue.Enqueue(vfxInstance);
                vfxInstance.SetActive(false);
            }
            else
            {
                // This case is unlikely but could happen if the pool was cleared.
                Destroy(vfxInstance);
            }
        }
        else
        {
            // If the instance wasn't created by this spawner, just destroy it.
            Debug.LogWarning("Returning a VFX object that was not spawned from the pool. Destroying it instead.", vfxInstance);
            Destroy(vfxInstance);
        }
    }
}
