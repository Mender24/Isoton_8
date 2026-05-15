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

    [Tooltip("Максимальный угол (градусы) между forward туррели и направлением на цель для начала стрельбы.")]
    [SerializeField] private float _aimThreshold = 8f;

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

    [Header("Multi-Ray Detection")]
    [Tooltip("Проверять видимость по нескольким точкам тела (ноги, пояс, голова).")]
    [SerializeField] private bool _useMultiRay = false;
    [SerializeField] private Vector3[] _bodyCheckOffsets = { new(0, 0.1f, 0), new(0, 1.0f, 0), new(0, 1.7f, 0) };


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
            else if (!_rangedCombat.IsFiring && !State.IsReloading && _rangedCombat.CanShoot
                     && IsAimedAt(_currentTarget))
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

        // Игрок
        if (_targetMode != TurretTargetMode.MutantsOnly
            && PlayerTransform != null && CanSeeTarget(PlayerTransform))
        {
            float d = Vector3.Distance(transform.position, PlayerTransform.position);
            if (d < bestDist) { bestDist = d; nearest = PlayerTransform; }
        }

        // Мутанты
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

        Vector3 eyePos   = _verticalPivot != null ? _verticalPivot.position : transform.position + Vector3.up * 0.5f;
        Vector3 aimPoint = target.position + Vector3.up * _aimHeightOffset;
        Vector3 aimDir   = aimPoint - eyePos;
        float   dist     = aimDir.magnitude;

        if (dist > _scanRadius) return false;

        float pitchRaw = Mathf.Atan2(aimDir.y, new Vector2(aimDir.x, aimDir.z).magnitude) * Mathf.Rad2Deg;
        if (_upsideDown) pitchRaw = -pitchRaw;
        if (-pitchRaw < _minPitch || -pitchRaw > _maxPitch) return false;

        bool seen;
        if (_useMultiRay)
        {
            seen = CanSeeMultiRay(eyePos, target, aimPoint);
        }
        else
        {
            seen = CanSeeRay(eyePos, aimPoint, dist);
            if (seen) _visibleTargetPoint = aimPoint;
        }

        _haveTarget = seen;
        return seen;
    }

    private bool CanSeeRay(Vector3 from, Vector3 to, float dist)
    {
        return !Physics.Raycast(from, (to - from).normalized, dist - 0.1f, _obstacleLayer);
    }

    private bool CanSeeMultiRay(Vector3 eyePos, Transform target, Vector3 fallback)
    {
        if (_bodyCheckOffsets == null || _bodyCheckOffsets.Length == 0)
        {
            bool fb = CanSeeRay(eyePos, fallback, (fallback - eyePos).magnitude);
            if (fb) _visibleTargetPoint = fallback;
            return fb;
        }

        for (int i = _bodyCheckOffsets.Length - 1; i >= 0; i--)
        {
            Vector3 point = target.position + _bodyCheckOffsets[i];
            Vector3 dir   = point - eyePos;
            if (!Physics.Raycast(eyePos, dir.normalized, dir.magnitude - 0.1f, _obstacleLayer))
            {
                _visibleTargetPoint = point;
                return true;
            }
        }
        return false;
    }

    private bool IsAimedAt(Transform target)
    {
        if (target == null || _muzzle == null) return false;

        Vector3 toTarget = _visibleTargetPoint - _muzzle.position;
        if (toTarget.sqrMagnitude < 0.01f) return true;

        float angle = Vector3.Angle(_muzzle.forward, toTarget);
        return angle <= (_aimThreshold + 2f);
    }



    private void RotateTowards(Transform target)
    {
        if (target == null || _muzzle == null) return;

        Vector3 from = _muzzle.position;
        Vector3 to = target.position;

        Vector3 flatDir = to - from;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(flatDir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, rot, _rotationSpeed * Time.deltaTime);
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
