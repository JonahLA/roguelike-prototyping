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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + delta);
    }

    /// <summary>
    /// Called by the PlayerInput (Invoke Unity Events).
    /// </summary>
    /// <param name="ctx"></param>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
            _moveInput = ctx.ReadValue<Vector2>();
        else if (ctx.phase == InputActionPhase.Canceled)
            _moveInput = Vector2.zero;
    }
}
