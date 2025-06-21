using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Strategies")]
    [SerializeField] private TargetingStrategySO _passiveTargeting;
    [SerializeField] private TargetingStrategySO _pursueTargeting;
    [SerializeField] private MovementStrategySO _movementStrategy;
    [SerializeField] private AttackStrategySO _attackStrategy;

    [Header("AI State")]
    [SerializeField] private EnemyAIState _currentState = EnemyAIState.Passive;
    private Vector2 _currentTarget;
    private float _timeSinceLastAttack = 0f;
    private float _timeInCurrentWanderTarget = 0f;
    private float _wanderTargetDwellTime = 2f; // Time to stay at a wander point

    // Component References
    private Enemy _enemy;
    private Rigidbody2D _rb;
    private Transform _playerTransform;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // A more robust system might use a service locator or dependency injection
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _timeSinceLastAttack = _enemy.Stats.attackCooldown; // Allow attacking immediately
        SetState(EnemyAIState.Passive);
    }

    private void Update()
    {
        _timeSinceLastAttack += Time.deltaTime;

        UpdateStateTransitions();
        ExecuteCurrentStateLogic();
    }

    private void FixedUpdate()
    {
        if (_currentState != EnemyAIState.Attacking)
        {
            _movementStrategy.Move(_enemy, _rb, _currentTarget);
        }
    }

    private void UpdateStateTransitions()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        if (_currentState == EnemyAIState.Passive)
        {
            if (distanceToPlayer <= _enemy.Stats.detectionRange)
            {
                SetState(EnemyAIState.Pursuing);
            }
        }
        else if (_currentState == EnemyAIState.Pursuing)
        {
            if (distanceToPlayer <= _enemy.Stats.attackRange)
            {
                SetState(EnemyAIState.Attacking);
            }
            else if (distanceToPlayer > _enemy.Stats.detectionRange)
            {
                SetState(EnemyAIState.Passive);
            }
        }
        else if (_currentState == EnemyAIState.Attacking)
        {
            if (distanceToPlayer > _enemy.Stats.attackRange)
            {
                SetState(EnemyAIState.Pursuing);
            }
        }
    }

    private void ExecuteCurrentStateLogic()
    {
        switch (_currentState)
        {
            case EnemyAIState.Passive:
                // Get a new wander target if we've reached the old one or time is up
                _timeInCurrentWanderTarget += Time.deltaTime;
                if (Vector2.Distance(transform.position, _currentTarget) < 0.1f || _timeInCurrentWanderTarget > _wanderTargetDwellTime)
                {
                    _currentTarget = _passiveTargeting.GetTarget(_enemy, _playerTransform);
                    _timeInCurrentWanderTarget = 0f;
                }
                break;

            case EnemyAIState.Pursuing:
                _currentTarget = _pursueTargeting.GetTarget(_enemy, _playerTransform);
                break;

            case EnemyAIState.Attacking:
                // Stop moving
                _rb.velocity = Vector2.zero;

                // Attack on cooldown
                if (_timeSinceLastAttack >= _enemy.Stats.attackCooldown)
                {
                    _timeSinceLastAttack = 0f;
                    _attackStrategy.Attack(_enemy, _playerTransform);
                }
                break;
        }
    }

    private void SetState(EnemyAIState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        _timeInCurrentWanderTarget = _wanderTargetDwellTime; // Force new wander target when entering passive state
    }
}
