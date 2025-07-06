using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Individual damage number that displays floating damage text with animation.
/// Handles its own lifecycle and returns to pool when animation completes.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How long the damage number is displayed")]
    [SerializeField, Range(0.5f, 3f)]
    private float _lifetime = 1.5f;
    
    [Tooltip("How high the number floats upward")]
    [SerializeField, Range(0.5f, 3f)]
    private float _floatHeight = 1.5f;
    
    [Tooltip("Random horizontal drift range")]
    [SerializeField, Range(0f, 1f)]
    private float _horizontalDrift = 0.3f;
    
    [Tooltip("Animation curve for movement (time vs position)")]
    [SerializeField]
    private AnimationCurve _movementCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, Mathf.PI / 2f), new Keyframe(1f, 1f, 0f, 0f));
    
    [Tooltip("Animation curve for opacity fade from 1 (opaque) to 0 (transparent) with an ease-in sine shape.")]
    [SerializeField]
    private AnimationCurve _fadeCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -Mathf.PI / 2f, 0f));
    
    [Header("Visual Settings")]
    [Tooltip("Color for normal damage")]
    [SerializeField]
    private Color _damageColor = Color.red;
    
    [Tooltip("Color for critical damage")]
    [SerializeField]
    private Color _criticalColor = Color.yellow;
    
    [Tooltip("Color for healing")]
    [SerializeField]
    private Color _healColor = Color.green;
    
    [Tooltip("Scale multiplier for critical hits")]
    [SerializeField, Range(1f, 3f)]
    private float _criticalScale = 1.5f;
    
    // Components
    private TextMeshPro _textMesh;
    private Transform _transform;
    
    // Animation state
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _startTime;
    private Color _startColor;
    private bool _isAnimating;
    
    // Pool reference
    private DamageNumberSpawner _spawner;
    
    private void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
        _transform = transform;
        
        // Ensure the text mesh is set up correctly
        // Access sorting properties through the renderer component
        Renderer textRenderer = _textMesh.renderer;
        if (textRenderer.sortingLayerName == "Default")
        {
            textRenderer.sortingLayerName = "UI"; // Ensure it renders above game objects
        }
        textRenderer.sortingOrder = 100;
    }
    
    /// <summary>
    /// Initialize and start animating the damage number.
    /// </summary>
    /// <param name="damage">Damage amount to display</param>
    /// <param name="worldPosition">World position to spawn at</param>
    /// <param name="isCritical">Whether this is a critical hit</param>
    /// <param name="isHealing">Whether this is healing instead of damage</param>
    /// <param name="spawner">Reference to the spawner for returning to pool</param>
    public void Initialize(float damage, Vector3 worldPosition, bool isCritical = false, bool isHealing = false, DamageNumberSpawner spawner = null)
    {
        _spawner = spawner;
        
        // Set position
        _transform.position = worldPosition;
        _startPosition = worldPosition;
        
        // Calculate target position with random drift
        Vector3 drift = new Vector3(
            Random.Range(-_horizontalDrift, _horizontalDrift),
            _floatHeight,
            0f
        );
        _targetPosition = _startPosition + drift;
        
        // Set text content
        if (isHealing)
        {
            _textMesh.text = $"+{damage:F1}";
            _startColor = _healColor;
        }
        else
        {
            _textMesh.text = $"{damage:F1}";
            _startColor = isCritical ? _criticalColor : _damageColor;
        }
        
        // Set visual properties
        _textMesh.color = _startColor;
        float scale = isCritical ? _criticalScale : 1f;
        _transform.localScale = Vector3.one * scale;
        
        // Start animation
        _startTime = Time.time;
        _isAnimating = true;
        
        // Start the animation coroutine
        StartCoroutine(AnimateLifetime());
    }
    
    private IEnumerator AnimateLifetime()
    {
        while (_isAnimating && Time.time - _startTime < _lifetime)
        {
            float elapsed = Time.time - _startTime;
            float normalizedTime = elapsed / _lifetime;
            
            // Update position using curve
            float positionProgress = _movementCurve.Evaluate(normalizedTime);
            _transform.position = Vector3.Lerp(_startPosition, _targetPosition, positionProgress);
            
            // Update opacity using fade curve
            float alpha = _fadeCurve.Evaluate(normalizedTime);
            Color currentColor = _startColor;
            currentColor.a = alpha;
            _textMesh.color = currentColor;
            
            yield return null;
        }
        
        // Animation complete, return to pool or destroy
        ReturnToPool();
    }
    
    /// <summary>
    /// Stop animation and return to pool.
    /// </summary>
    public void ReturnToPool()
    {
        _isAnimating = false;
        StopAllCoroutines();
        
        if (_spawner != null)
        {
            _spawner.ReturnToPool(this);
        }
        else
        {
            // Fallback: destroy if no pool available
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Reset the damage number for reuse from pool.
    /// </summary>
    public void ResetForPool()
    {
        _isAnimating = false;
        StopAllCoroutines();
        _textMesh.color = Color.white;
        _transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
    
    // Handle cleanup if object is destroyed while animating
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
