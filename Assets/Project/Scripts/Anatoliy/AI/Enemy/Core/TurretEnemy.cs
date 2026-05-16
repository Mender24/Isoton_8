using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Простейшая туррель. Стоит на месте, поворачивается к цели.
/// Режим цели задаётся через <see cref="TargetMode"/> или <see cref="SetTargetMode"/>.
/// Когда видит цель — ждёт <see cref="_shootDelay"/> секунд, затем открывает огонь.
/// Когда теряет все цели — немедленно прекращает стрельбу.
/// </summary>
[RequireComponent(typeof(RangedCombatModule))]
public class TurretEnemy : EnemyBase
{
    public enum TurretTargetMode
    {
        PlayerOnly,
        MutantsOnly,
        Any
    }
    public event System.Action OnTargetModeChanged;

    [Header("Turret")]
    [Tooltip("Задержка перед открытием огня после обнаружения цели.")]
    [SerializeField] private float _shootDelay = 0.5f;

    [Tooltip("Скорость поворота к цели (градусов/сек).")]
    [SerializeField] private float _rotationSpeed = 90f;

    [Header("Target Scan")]
    [Tooltip("Тег мутантов (MeleeEnemy, CrawlerEnemy, ScriptedCeilingCrawler).")]
    [SerializeField] private string _mutantTag = "Mutant";

    [Tooltip("Радиус поиска целей.")]
    [SerializeField] private float _scanRadius = 20f;

    [Tooltip("Как часто пересканировать (сек).")]
    [SerializeField] private float _scanInterval = 0.3f;

    [Tooltip("Слой препятствий для проверки видимости цели.")]
    [SerializeField] private LayerMask _obstacleLayer;

    [Tooltip("Слой(и) на которых находятся мутанты (для OverlapSphere).")]
    [SerializeField] private LayerMask _mutantLayer = ~0;

    [Tooltip("Горизонтальный угол обзора в градусах относительно начального направления турели. 360 = полный круговой обзор.")]
    [SerializeField] private float _fieldOfViewAngle = 180f;

    [Tooltip("Кого атакует туррель по умолчанию.")]
    [SerializeField] private TurretTargetMode _targetMode = TurretTargetMode.Any;

    [Header("Vertical Aim")]
    [SerializeField] private Transform _verticalPivot;
    [SerializeField] private float _minPitch = -30f;
    [SerializeField] private float _maxPitch = 30f;
    [SerializeField] private bool _upsideDown = false;
    [Tooltip("Смещение точки прицеливания по вертикали")]
    [SerializeField] private float _aimHeightOffset = 1f;

    [SerializeField] private Transform _muzzle;


    public bool _haveTarget;

    private Vector3 _visibleTargetPoint;


    public TurretTargetMode TargetMode => _targetMode;


    public void SetTargetModeByIndex(int index)
    {
        if (index >= 0 && index < System.Enum.GetValues(typeof(TurretTargetMode)).Length)
        {
            _targetMode = (TurretTargetMode)index;
            _currentTarget = null;
            _scanTimer = 0f;

            OnTargetModeChanged?.Invoke();
        }
    }

    private RangedCombatModule _rangedCombat;
    private float _shootTimer;
    private bool  _waitingToShoot;

    private Transform _currentTarget;
    private float     _scanTimer;

    private readonly Collider[] _scanBuffer = new Collider[64];

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

        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = _scanInterval;
            ScanForTarget();
        }

        if (_currentTarget != null)
        {
            RotateTowards(_currentTarget);
            RotateVerticalPivot(_currentTarget);

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

    private void ScanForTarget()
    {
        Transform nearest  = null;
        float     bestDist = Mathf.Infinity;

        if (_targetMode != TurretTargetMode.MutantsOnly
            && PlayerTransform != null && CanSeeTarget(PlayerTransform))
        {
            float d = Vector3.Distance(transform.position, PlayerTransform.position);
            if (d < bestDist) { bestDist = d; nearest = PlayerTransform; }
        }

        if (_targetMode != TurretTargetMode.PlayerOnly)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, _scanRadius, _scanBuffer, _mutantLayer);
            var seen  = new HashSet<EnemyBase>();

            for (int i = 0; i < count; i++)
            {
                var enemy = _scanBuffer[i].GetComponentInParent<EnemyBase>();
                if (enemy == null || enemy == this) continue;
                if (!seen.Add(enemy)) continue;
                if (!enemy.CompareTag(_mutantTag)) continue;
                if (enemy.State.IsDead) continue;
                if (!CanSeeTarget(enemy.transform)) continue;

                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < bestDist) { bestDist = d; nearest = enemy.transform; }
            }
        }

        _currentTarget = nearest;
        _haveTarget    = nearest != null;
        bool targetIsEnemy = nearest != null && nearest != PlayerTransform;
        _rangedCombat.SetTarget(_currentTarget, targetIsEnemy);
    }

    private bool CanSeeTarget(Transform target)
    {
        if (target == null)
        {
            _haveTarget = false; //Mender Вывожу состояние цели для вращения в анимацию, так что сделал публичный bool.
            return false;
        }

        _haveTarget = false;

        Vector3 eyePos   = _muzzle != null ? _muzzle.position
                         : _verticalPivot != null ? _verticalPivot.position
                         : transform.position + Vector3.up * 0.5f;
        Vector3 aimPoint = target.position + Vector3.up * _aimHeightOffset;
        Vector3 aimDir   = aimPoint - eyePos;
        float   dist     = aimDir.magnitude;

        if (dist > _scanRadius) return false;

        if (_fieldOfViewAngle < 360f)
        {
            Vector3 flatForward  = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 flatToTarget = new Vector3(aimDir.x, 0f, aimDir.z).normalized;
            if (Vector3.Angle(flatForward, flatToTarget) > _fieldOfViewAngle * 0.5f) return false;
        }

        float pitchRaw = Mathf.Atan2(aimDir.y, new Vector2(aimDir.x, aimDir.z).magnitude) * Mathf.Rad2Deg;
        if (_upsideDown) pitchRaw = -pitchRaw;
        if (-pitchRaw < _minPitch || -pitchRaw > _maxPitch) return false;

        if (Physics.Raycast(eyePos, aimDir.normalized, dist - 0.1f, _obstacleLayer))
            return false;

        _visibleTargetPoint = aimPoint;
        _haveTarget = true;
        return true;
    }


    private void RotateTowards(Transform target)
    {
        if (target == null || _muzzle == null) return;

        Vector3 worldDir = target.position - _muzzle.position;
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 0.01f) return;

        Vector3 localDir = transform.parent != null
            ? transform.parent.InverseTransformDirection(worldDir)
            : worldDir;
        localDir.y = 0f;
        if (localDir.sqrMagnitude < 0.01f) return;

        Quaternion localTarget = Quaternion.LookRotation(localDir);
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, localTarget, _rotationSpeed * Time.deltaTime);
    }

    private void RotateVerticalPivot(Transform target)
    {
        if (_verticalPivot == null || target == null) return;

        Vector3 dir = _visibleTargetPoint - _verticalPivot.position;
        float pitch = Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;

        if (_upsideDown) pitch = -pitch;

        pitch = Mathf.Clamp(-pitch, _minPitch, _maxPitch);

        Vector3 angles = _verticalPivot.localEulerAngles;
        angles.z = pitch;
        _verticalPivot.localEulerAngles = angles;
    }

    public override bool CanAttack()   => _rangedCombat.CanShoot;
    public override void StartAttack() => _rangedCombat.StartFire();

    public override void FullReset()
    {
        base.FullReset();
        _shootTimer     = 0f;
        _waitingToShoot = false;
        _currentTarget  = null;
        _scanTimer      = 0f;
        _rangedCombat.StopFire();
        _rangedCombat.Initialize(PlayerTransform);
        Navigation.Stop();
        Navigation.Agent.updateRotation = false;
    }
}
