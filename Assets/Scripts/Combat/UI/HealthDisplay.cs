using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// UI component that displays health as hearts with support for half-hearts.
/// Automatically updates when connected Health component changes.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    [Header("Heart UI References")]
    [Tooltip("Prefab for individual heart UI elements")]
    [SerializeField]
    private Image _heartPrefab;

    [Tooltip("Container where heart UI elements will be spawned")]
    [SerializeField]
    private Transform _heartsContainer;

    [Header("Heart Sprites")]
    [Tooltip("Sprite for full heart")]
    [SerializeField]
    private Sprite _fullHeartSprite;

    [Tooltip("Sprite for half heart")]
    [SerializeField]
    private Sprite _halfHeartSprite;

    [Tooltip("Sprite for empty heart")]
    [SerializeField]
    private Sprite _emptyHeartSprite;

    [Tooltip("Color of each heart")]
    [SerializeField]
    private Color _heartColor = Color.white;

    [Header("Animation Settings")]
    [Tooltip("Enable heart animations when health changes")]
    [SerializeField]
    private bool _enableAnimations = true;

    [Tooltip("Duration of heart scale animation")]
    [SerializeField, Range(0.1f, 1f)]
    private float _animationDuration = 0.3f;

    [Tooltip("Scale multiplier for heart damage animation")]
    [SerializeField, Range(1.1f, 2f)]
    private float _damageAnimationScale = 1.5f;

    [Tooltip("Scale multiplier for heart heal animation")]
    [SerializeField, Range(1.1f, 2f)]
    private float _healAnimationScale = 1.2f;

    [Header("Heart Layout")]
    [Tooltip("Size of each heart in the UI, in pixels")]
    [SerializeField, Range(0, 100)]
    private int _heartSize = 24;
    
    [Tooltip("Horizontal spacing between heart columns, as a percentage of the heart size")]
    [SerializeField, Range(0.0f, 5.0f)]
    private float _columnSpacing = 1.0f;
    
    [Tooltip("Vertical spacing between heart rows, as a percentage of the heart size")]
    [SerializeField, Range(0.0f, 5.0f)]
    private float _rowSpacing = 1.0f;
    
    [Tooltip("Vertical offset for right column hearts_heartSize")]
    [SerializeField, Range(0.0f, 5.0f)]
    private float _halfRowOffset = 0.5f;

    [Header("Health Component")]
    [Tooltip("Health component to monitor. If null, will search on same GameObject.")]
    [SerializeField]
    private Health _healthComponent;

    // UI state
    private List<Image> _heartImages = new();
    private float _currentDisplayedHealth;
    private float _maxDisplayedHealth;
    private void Awake()
    {
        // Validate required components
        ValidateComponents();
    }

    private void Start()
    {
        // Subscribe to centralized player health events
        HealthManager.PlayerHealthChanged += OnHealthChanged;

        // Request initial health state from any existing player
        RequestInitialHealthState();
    }
    private void OnDestroy()
    {
        // Unsubscribe from centralized health events
        HealthManager.PlayerHealthChanged -= OnHealthChanged;

        // Clean up any remaining heart objects safely
        if (_heartImages != null)
        {
            foreach (var heart in _heartImages)
            {
                if (heart != null && heart.gameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(heart.gameObject);
                    }
                }
            }
            _heartImages.Clear();
        }
    }
    private void ValidateComponents()
    {
        if (_heartPrefab == null)
        {
            Debug.LogError("Heart prefab is not assigned!", this);
        }

        if (_heartsContainer == null)
        {
            Debug.LogError("Hearts container is not assigned!", this);
        }

        if (_fullHeartSprite == null || _halfHeartSprite == null || _emptyHeartSprite == null)
        {
            Debug.LogError("Heart sprites are not assigned!", this);
        }
    }

    /// <summary>
    /// Request initial health state from any existing player in the scene.
    /// This ensures the UI displays correctly even if the player was already in the scene.
    /// </summary>
    private void RequestInitialHealthState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Initialize display with current player health
                _currentDisplayedHealth = playerHealth.CurrentHealth;
                _maxDisplayedHealth = playerHealth.MaxHealth;
                CreateHeartUI();
                UpdateHeartVisuals();
            }
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        // Check if max health changed (need to recreate UI)
        if (!Mathf.Approximately(maxHealth, _maxDisplayedHealth))
        {
            _maxDisplayedHealth = maxHealth;
            CreateHeartUI();
        }

        // Determine if this was damage or healing for animation
        bool wasHealing = currentHealth > _currentDisplayedHealth;
        bool wasDamage = currentHealth < _currentDisplayedHealth;

        _currentDisplayedHealth = currentHealth;
        UpdateHeartVisuals();

        // Trigger animations
        if (_enableAnimations)
        {
            if (wasDamage)
            {
                StartCoroutine(AnimateHeartChange(_damageAnimationScale));
            }
            else if (wasHealing)
            {
                StartCoroutine(AnimateHeartChange(_healAnimationScale));
            }
        }
    }
    private void CreateHeartUI()
    {
        // Clear existing hearts safely
        foreach (var heart in _heartImages)
        {
            if (heart != null && heart.gameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(heart.gameObject);
                }
                else
                {
                    DestroyImmediate(heart.gameObject);
                }
            }
        }
        _heartImages.Clear();

        // Validate required components before creating UI
        if (_heartPrefab == null || _heartsContainer == null)
        {
            return;
        }

        // Create new hearts based on max health
        int totalHearts = Mathf.CeilToInt(_maxDisplayedHealth);

        for (int i = 0; i < totalHearts; i++)
        {
            GameObject heartObj = Instantiate(_heartPrefab.gameObject, _heartsContainer);
            Image heartImage = heartObj.GetComponent<Image>();

            if (heartImage != null)
            {
                // Set up the heart positioning in zig-zag pattern
                SetupHeartPosition(heartImage, i);
                heartImage.color = _heartColor;
                _heartImages.Add(heartImage);
            }
        }
    }

    private void UpdateHeartVisuals()
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            float heartValue = _currentDisplayedHealth - i;

            if (heartValue >= 1f)
            {
                // Full heart
                _heartImages[i].sprite = _fullHeartSprite;
            }
            else if (heartValue >= 0.5f)
            {
                // Half heart
                _heartImages[i].sprite = _halfHeartSprite;
            }
            else
            {
                // Empty heart
                _heartImages[i].sprite = _emptyHeartSprite;
            }
        }
    }

    private IEnumerator AnimateHeartChange(float targetScale)
    {
        // Animate all hearts that are affected
        List<Vector3> originalScales = new List<Vector3>();

        // Store original scales
        foreach (var heart in _heartImages)
        {
            originalScales.Add(heart.transform.localScale);
        }

        // Scale up
        float elapsed = 0f;
        while (elapsed < _animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (_animationDuration * 0.5f);
            float currentScale = Mathf.Lerp(1f, targetScale, progress);

            for (int i = 0; i < _heartImages.Count; i++)
            {
                _heartImages[i].transform.localScale = originalScales[i] * currentScale;
            }

            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < _animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (_animationDuration * 0.5f);
            float currentScale = Mathf.Lerp(targetScale, 1f, progress);

            for (int i = 0; i < _heartImages.Count; i++)
            {
                _heartImages[i].transform.localScale = originalScales[i] * currentScale;
            }

            yield return null;
        }

        // Ensure final scale is correct
        for (int i = 0; i < _heartImages.Count; i++)
        {
            _heartImages[i].transform.localScale = originalScales[i];
        }
    }

    /// <summary>
    /// Set up heart positioning in a vertical zig-zag pattern.
    /// Hearts alternate between left and right columns, offset vertically.
    /// </summary>
    /// <param name="heartImage">The heart image to position</param>
    /// <param name="index">Index of this heart (0-based)</param>
    private void SetupHeartPosition(Image heartImage, int index)
    {
        RectTransform heartRect = heartImage.GetComponent<RectTransform>();
        if (heartRect == null) return;

        // Determine which column (left = 0, right = 1)
        bool isRightColumn = (index % 2) == 1;
        int rowIndex = index / 2; // Which row this heart is in

        // Calculate position
        float xPos = isRightColumn ? _columnSpacing * _heartSize : 0f;
        float yPos = -(rowIndex * _rowSpacing * _heartSize + (isRightColumn ? _halfRowOffset * _heartSize: 0f));

        // Apply position
        heartRect.anchoredPosition = new Vector2(xPos, yPos);
        heartRect.sizeDelta = new Vector2(_heartSize, _heartSize);

        // Set anchoring to top-left for consistent positioning
        heartRect.anchorMin = new Vector2(0f, 1f);
        heartRect.anchorMax = new Vector2(0f, 1f);
        heartRect.pivot = new Vector2(0.5f, 0.5f);
    }
    
    /// <summary>
    /// Force refresh the display (useful for editor preview).
    /// </summary>
    [ContextMenu("Refresh Display")]
    public void RefreshDisplay()
    {
        RequestInitialHealthState();
    }
    
    // Editor preview functionality
    #if UNITY_EDITOR
    [Header("Editor Preview")]
    [SerializeField, Range(0f, 10f)]
    private float _previewCurrentHealth = 5f;
    
    [SerializeField, Range(0.5f, 10f)]
    private float _previewMaxHealth = 5f;
    
    [ContextMenu("Preview Health")]
    private void PreviewHealth()
    {
        if (!Application.isPlaying)
        {
            _currentDisplayedHealth = _previewCurrentHealth;
            _maxDisplayedHealth = _previewMaxHealth;
            CreateHeartUI();
            UpdateHeartVisuals();
        }
    }
    #endif
}
