using UnityEngine;

/// <summary>
/// A component that automatically deactivates its GameObject after the attached ParticleSystem has finished playing.
/// This is essential for returning pooled particle effects to the VFXSpawner.
/// </summary>
/// <remarks>
/// For this script to work, the ParticleSystem's "Stop Action" in the Main module must be set to "Callback".
/// </remarks>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemAutoReturn : MonoBehaviour
{
    /// <summary>
    /// This method is automatically called by Unity when the ParticleSystem finishes.
    /// It deactivates the GameObject, making it available again in the object pool.
    /// </summary>
    private void OnParticleSystemStopped()
    {
        gameObject.SetActive(false);
    }
}
