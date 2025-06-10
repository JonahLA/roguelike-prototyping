using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles 2D player movement based on user input using Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("Gameplay/Player Controller 2D")]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second")]
    [SerializeField, Range(0f, 10f)]
    private float _moveSpeed = 2f;

    private Vector2 _moveInput;
    private Rigidbody2D _rb;

    /// <summary>
    /// The current direction the player is facing.
    /// </summary>
    public Vector2 FacingDirection { get; private set; } = Vector2.down; // Default to down or a common starting direction

    /// <summary>
    /// Indicates whether the player can currently perform actions like attacking.
    /// Set to false during actions like dashing or being stunned.
    /// </summary>
    public bool CanPerformActions { get; private set; } = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + delta);

        // Update facing direction if there is movement input
        if (_moveInput.sqrMagnitude > 0.01f) // Use a small threshold to avoid jitter from near-zero input
        {
            FacingDirection = _moveInput.normalized;
        }
    }

    /// <summary>
    /// Called by the PlayerInput (Invoke Unity Events).
    /// </summary>
    /// <param name="ctx"></param>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            _moveInput = ctx.ReadValue<Vector2>();
            // Update facing direction immediately on performed input if significant
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                FacingDirection = _moveInput.normalized;
            }
        }
        else if (ctx.phase == InputActionPhase.Canceled)
            _moveInput = Vector2.zero;
    }

    // Example methods to control CanPerformActions (add more as needed for dash, stun, etc.)
    public void StartPlayerAction()
    {
        CanPerformActions = false;
        // e.g., called at the beginning of a dash
    }

    public void EndPlayerAction()
    {
        CanPerformActions = true;
        // e.g., called at the end of a dash
    }
}
