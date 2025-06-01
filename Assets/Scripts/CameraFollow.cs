using UnityEngine;

/// <summary>
/// Makes the camera follow a target Transform smoothly.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// The Transform that the camera will follow.
    /// </summary>
    [Tooltip("Transform of the Player to follow.")]
    public Transform target;

    /// <summary>
    /// The approximate time it will take to reach the target. A smaller value will reach the target faster.
    /// </summary>
    [Tooltip("How quickly the camera moves to catch up.")]
    public float smoothTime = 0.2f;

    private Vector3 _velocity; // Current velocity, this value is modified by SmoothDamp every time you call it.

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPos = new(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    }
}
