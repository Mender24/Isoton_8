using UnityEngine;
using Akila.FPSFramework;

public class MeleeCombatModule : MonoBehaviour, IMeleeCombat
{
    [Header("Config")]
    [SerializeField] private MeleeEnemyConfig _config;

    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    public bool  CanAttack    => _state != null && _state.MeleeAttackCooldown <= 0f && !_state.IsMeleeAttacking;
    public bool  IsAttacking  => _state != null && _state.IsMeleeAttacking;
    public float AttackRange  => _config != null ? _config.AttackRange  : 2f;
    public float AttackDuration => _config != null ? _config.AttackDuration : 3f;

    private EnemyState     _state;
    private IEnemyAnimator _animator;

    private void Awake()
    {
        _state    = GetComponent<EnemyState>();
        _animator = GetComponent<IEnemyAnimator>();
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public bool IsInRange()
    {
        if (_playerTransform == null) return false;
        return Vector3.Distance(transform.position, _playerTransform.position) <= _config.AttackRange;
    }

    public void StartAttack()
    {
        if (!CanAttack || !_state.PlayerDetected) return;

        _state.IsMeleeAttacking = true;
        _state.MeleeAttackCooldown = _config.AttackCooldown;

        bool inMotion = GetComponent<EnemyNavigation>()?.Agent.velocity.sqrMagnitude > 0.1f;
        _animator?.SetMeleeAttacking(true, _config.AttackDuration, inMotion);
    }

    public void ExecuteHit()
    {
        if (_playerTransform == null) return;

        Vector3 attackPos = transform.position
            + transform.forward * _config.AttackOffset.z
            + Vector3.up        * _config.AttackOffset.y;

        Collider[] hits = Physics.OverlapSphere(attackPos, _config.AttackRadius, _config.HitLayers);
        foreach (var col in hits)
        {
            if (col.transform == _playerTransform)
            {
                if (col.TryGetComponent(out Damageable d))
                    d.Damage(_config.Damage);
                break;
            }
        }
    }

    public void Tick(float deltaTime)
    {
        if (_state.MeleeAttackCooldown > 0f)
            _state.MeleeAttackCooldown -= deltaTime;

        if (_state.IsMeleeAttacking && _state.MeleeAttackCooldown <= 0f)
        {
            _state.IsMeleeAttacking = false;
            _animator?.SetMeleeAttacking(false, 0f, false);
        }
    }
}