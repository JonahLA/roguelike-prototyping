using UnityEngine;

/// <summary>
/// The main AI controller for an enemy. It manages the enemy's state,
/// executes behaviors based on assigned strategies, and handles transitions between states.
/// </summary>
[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Strategies")]
    [Tooltip("The targeting strategy to use when in the Passive state (e.g., wandering).")]
    [SerializeField] private TargetingStrategySO _passiveTargeting;
    [Tooltip("The targeting strategy to use when pursuing the player.")]
    [SerializeField] private TargetingStrategySO _pursueTargeting;
    [Tooltip("The movement strategy used to move the enemy.")]
    [SerializeField] private MovementStrategySO _movementStrategy;
    [Tooltip("The attack strategy used when in attacking range.")]
    [SerializeField] private AttackStrategySO _attackStrategy;

    [Header("AI Behavior")]
    [Tooltip("The time in seconds the enemy will wait at a wander point before choosing a new one.")]
    [SerializeField] private float _wanderTargetDwellTime = 2f;

    [Header("AI State")]
    [Tooltip("The current state of the AI. Visible for debugging.")]
    [SerializeField] private EnemyAIState _currentState = EnemyAIState.Passive;
    
    // Private fields
    private Vector2 _currentTarget;
    private float _timeSinceLastAttack = 0f;
    private float _timeInCurrentWanderTarget = 0f;
    
    // Constants
    private const float WANDER_TARGET_REACHED_DISTANCE = 0.1f;

    // Component References
    private Enemy _enemy;
    private Rigidbody2D _rb;
    private Transform _playerTransform;

    private void Awake()
    {
        // Cache component references
        _enemy = GetComponent<Enemy>();
        _rb = GetComponent<Rigidbody2D>();

        // Subscribe to the death event
        Health health = _enemy.Health;
        health?.OnDeath.AddListener(HandleDeath);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        Health health = _enemy.Health;
        health?.OnDeath.RemoveListener(HandleDeath);
    }

    private void Start()
    {
        // A more robust system might use a service locator or dependency injection
        // For prototyping, finding the player by tag is acceptable.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player GameObject is tagged with 'Player'.", this);
            // Disable the AI if there's no player to interact with.
            enabled = false;
            return;
        }
        
        // Allow attacking immediately upon seeing the player
        _timeSinceLastAttack = _enemy.Stats.attackCooldown; 
        SetState(EnemyAIState.Passive);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        _timeSinceLastAttack += Time.deltaTime;

        UpdateStateTransitions();
        ExecuteCurrentStateLogic();
    }

    private void FixedUpdate()
    {
        // We only move if we are not in the middle of an attack animation/action.
        if (_currentState != EnemyAIState.Attacking)
        {
            _movementStrategy.Move(_enemy, _rb, _currentTarget);
        }
    }

    /// <summary>
    /// Handles the logic for transitioning between AI states based on player proximity.
    /// </summary>
    private void UpdateStateTransitions()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        // State transition logic
        switch (_currentState)
        {
            case EnemyAIState.Passive:
                if (distanceToPlayer <= _enemy.Stats.detectionRange)
                {
                    SetState(EnemyAIState.Pursuing);
                }
                break;
            case EnemyAIState.Pursuing:
                if (distanceToPlayer <= _enemy.Stats.attackRange)
                {
                    SetState(EnemyAIState.Attacking);
                }
                else if (distanceToPlayer > _enemy.Stats.detectionRange)
                {
                    SetState(EnemyAIState.Passive);
                }
                break;
            case EnemyAIState.Attacking:
                if (distanceToPlayer > _enemy.Stats.attackRange)
                {
                    SetState(EnemyAIState.Pursuing);
                }
                break;
        }
    }

    /// <summary>
    /// Executes the behavior corresponding to the current AI state.
    /// </summary>
    private void ExecuteCurrentStateLogic()
    {
        switch (_currentState)
        {
            case EnemyAIState.Passive:
                HandlePassiveState();
                break;

            case EnemyAIState.Pursuing:
                HandlePursuingState();
                break;

            case EnemyAIState.Attacking:
                HandleAttackingState();
                break;
        }
    }

    /// <summary>
    /// Defines the behavior for the Passive state (wandering).
    /// </summary>
    private void HandlePassiveState()
    {
        _timeInCurrentWanderTarget += Time.deltaTime;
        // Get a new wander target if we've reached the old one or the dwell time is up.
        if (Vector2.Distance(transform.position, _currentTarget) < WANDER_TARGET_REACHED_DISTANCE || _timeInCurrentWanderTarget > _wanderTargetDwellTime)
        {
            _currentTarget = _passiveTargeting.GetTarget(_enemy, _playerTransform);
            _timeInCurrentWanderTarget = 0f;
        }
    }

    /// <summary>
    /// Defines the behavior for the Pursuing state (moving towards the player).
    /// </summary>
    private void HandlePursuingState()
    {
        _currentTarget = _pursueTargeting.GetTarget(_enemy, _playerTransform);
    }

    /// <summary>
    /// Defines the behavior for the Attacking state (stopping and using an attack strategy).
    /// </summary>
    private void HandleAttackingState()
    {
        // Stop moving to attack
        _rb.linearVelocity = Vector2.zero;

        // Attack on cooldown
        if (_timeSinceLastAttack >= _enemy.Stats.attackCooldown)
        {
            _timeSinceLastAttack = 0f;
            _attackStrategy.Attack(_enemy, _playerTransform);
        }
    }

    /// <summary>
    /// Sets the AI to a new state and performs any necessary entry logic.
    /// </summary>
    /// <param name="newState">The state to transition to.</param>
    private void SetState(EnemyAIState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;

        // When entering the passive state, immediately force a new wander target decision.
        if (_currentState == EnemyAIState.Passive)
        {
             _timeInCurrentWanderTarget = _wanderTargetDwellTime;
        }
    }

    /// <summary>
    /// Handles the enemy's death by disabling its AI and collider.
    /// </summary>
    private void HandleDeath()
    {
        // Disable AI and collider on death to stop movement and interaction.
        // The GameObject is not destroyed here to allow for death animations or effects.
        enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }
}
