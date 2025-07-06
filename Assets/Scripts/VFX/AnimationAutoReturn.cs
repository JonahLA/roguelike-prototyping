using UnityEngine;

/// <summary>
/// A component that automatically deactivates its GameObject after the animation has finished playing.
/// This is essential for returning pooled particle effects to the VFXSpawner.
/// </summary>
public class AnimationAutoReturn : MonoBehaviour
{
    /// <summary>
    /// This method is called by the Animation Event at the end of the animation.
    /// It returns the object to the VFX pool.
    /// </summary>
    private void OnAnimationFinished()
    {
        if (VFXSpawner.Instance != null)
            VFXSpawner.Instance.ReturnVFX(gameObject);
        else
            gameObject.SetActive(false);
    }
}