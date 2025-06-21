using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemyStatsSO _stats;
    public EnemyStatsSO Stats => _stats;

    private Health _health;
    public Health Health => _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        if (_stats != null)
        {
            _health.SetMaxHealth(_stats.maxHealth);
        }
    }
}
