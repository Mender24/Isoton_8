using UnityEngine;

/// <summary>
/// Простейшая туррель. Стоит на месте, поворачивается к игроку.
/// Когда видит игрока — ждёт <see cref="_shootDelay"/> секунд, затем открывает огонь.
/// Когда перестаёт видеть — немедленно прекращает стрельбу.
/// </summary>
[RequireComponent(typeof(RangedCombatModule))]
public class TurretEnemy : EnemyBase
{
    [Header("Turret")]
    [Tooltip("Задержка перед открытием огня после обнаружения игрока.")]
    [SerializeField] private float _shootDelay = 0.5f;

    [Tooltip("Скорость поворота к игроку (градусов/сек).")]
    [SerializeField] private float _rotationSpeed = 90f;

    private RangedCombatModule _rangedCombat;
    private float _shootTimer;
    private bool  _waitingToShoot;

    protected override void Awake()
    {
        base.Awake();
        _rangedCombat = GetComponent<RangedCombatModule>();
    }

    protected override void OnInitialized()
    {
        _rangedCombat.Initialize(PlayerTransform);

        Navigation.Stop();
        Navigation.Agent.updateRotation = false;
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;

        _rangedCombat.Tick(Time.deltaTime);

        if (State.PlayerIsSeen)
        {
            RotateTowardsPlayer();

            if (_waitingToShoot)
            {
                _shootTimer -= Time.deltaTime;
                if (_shootTimer <= 0f)
                {
                    _waitingToShoot = false;
                    _rangedCombat.StartFire();
                }
            }
            else if (!_rangedCombat.IsFiring && !State.IsReloading && _rangedCombat.CanShoot)
            {
                _waitingToShoot = true;
                _shootTimer     = _shootDelay;
            }
        }
        else
        {
            if (_rangedCombat.IsFiring || State.IsReloading)
                _rangedCombat.StopFire();

            _waitingToShoot = false;
            _shootTimer     = 0f;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (PlayerTransform == null) return;

        Vector3 dir = PlayerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, _rotationSpeed * Time.deltaTime);
    }

    public override bool CanAttack()   => _rangedCombat.CanShoot;
    public override void StartAttack() => _rangedCombat.StartFire();
}
