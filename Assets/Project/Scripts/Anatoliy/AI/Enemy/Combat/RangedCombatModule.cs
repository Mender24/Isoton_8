using UnityEngine;
using Akila.FPSFramework;

public class RangedCombatModule : MonoBehaviour, IRangedCombat
{
    [Header("Config")]
    [SerializeField] private RangedEnemyConfig _config;

    [Header("References")]
    [SerializeField] public bool _useDoubleBarrelTurret = false;
    [SerializeField] private Transform _shotOriginSecondary = null;
    [SerializeField] private Transform _shotOrigin;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private ParticleSystem _muzzleFlashPrimary = null;
    [SerializeField] private ParticleSystem _muzzleFlashSecondary = null;

    public bool  CanShoot    => !IsReloading && _config != null;
    internal bool GetUseDoubleBarrelTurret() => _useDoubleBarrelTurret;
    internal bool GetIsPrimaryBarrelFiring() => _usePrimaryBarrel;
    public bool  IsFiring    => _state != null && _state.IsFiring;
    public bool  IsReloading => _state != null && _state.IsReloading;
    public float AttackRange => _config != null ? _config.AttackRange : 0f;
    public float ReloadTime  => _config != null ? _config.ReloadTime  : 3f;

    private EnemyState         _state;
    private EnemyNavigation    _navigation;
    private IEnemyAnimator     _animator;
    private IEnemyAudio        _audio;
    private EnemyDebugger      _debugger;
    private GrenadeThrowModule _grenadeModule;
    [Tooltip("Стрелять строго по направлению ствола. Вкл как туррель. Выкл для обычных врагов.")]
    [SerializeField] private bool _useForwardDirection = false;

    public bool _usePrimaryBarrel = true; // какой ствол в данный момент используется
    private bool _isPaused;
    private bool _hasManualTarget;

    public event System.Action OnFire;

    private void Awake()
    {
        _state         = GetComponent<EnemyState>();
        _navigation    = GetComponent<EnemyNavigation>();
        _animator      = GetComponent<IEnemyAnimator>();
        _audio         = GetComponent<IEnemyAudio>();
        _debugger      = GetComponent<EnemyDebugger>();
        _grenadeModule = GetComponent<GrenadeThrowModule>();
    }

    private void Start()
    {
        _state.OnIsReloadingChanged += ChangeMoving;
    }

    public void SetPaused(bool paused) => _isPaused = paused;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform  = playerTransform;
        _hasManualTarget  = false;
    }

    public void SetTarget(Transform target)
    {
        _playerTransform = target;
        _hasManualTarget = target != null;
    }

    public void StartFire()
    {
        if (!CanShoot) return;
        _state.IsFiring = true;
    }

    public void StopFire()
    {
        _state.IsFiring      = false;
        _state.IsReloading   = false;
        _state.ShootCooldown = 0f;
        _animator?.SetReloading(false, 0f);
    }

    public void Tick(float deltaTime)
    {
        if (_isPaused) return;

        if (_state.ShootCooldown > 0f)
        {
            _state.ShootCooldown -= deltaTime;

            if (_state.IsReloading && _state.ShootCooldown <= 0f)
            {
                _state.IsReloading = false;
                _animator?.SetReloading(false, 0f);
                if (_state.PlayerIsSeen || _hasManualTarget)
                    _state.IsFiring = true;
            }

            return;
        }

        if (!_state.IsFiring) return;

        Fire();
    }

    private void Fire()
    {
        if (!_hasManualTarget && !_state.PlayerIsSeen)
        {
            StopFire();
            return;
        }

        if (_playerTransform == null)
        {
            StopFire();
            return;
        }

        _state.CurrentBullet++;
        _grenadeModule?.OnBulletFired();

        Vector3 target = _playerTransform.position;
        target.y += Random.Range(-_config.HeightSprayOffset, _config.HeightSprayOffset) + _config.YOffset;
        target.x += Random.Range(-_config.WidthSprayOffset,  _config.WidthSprayOffset)  + _config.XOffset;

        SpawnBullet(target);
        if (_useDoubleBarrelTurret)
            _usePrimaryBarrel = !_usePrimaryBarrel;
        TryDealDamage(target);
        _audio?.PlayAttackSound();

        // Включаем нужный muzzle flash
        ParticleSystem currentMuzzleFlash = null;

        if (_useDoubleBarrelTurret)
        {
            currentMuzzleFlash = _usePrimaryBarrel ? _muzzleFlashPrimary : _muzzleFlashSecondary;
        }
        else
        {
            currentMuzzleFlash = _muzzleFlashPrimary;
        }

        if (currentMuzzleFlash != null)
            currentMuzzleFlash.Play();

        OnFire?.Invoke();

        if (_state.CurrentBullet >= _config.MagazineSize)
        {
            _state.CurrentBullet = 0;
            _state.IsFiring      = false;
            _state.IsReloading   = true;
            _state.ShootCooldown = _config.ReloadTime;
            _animator?.SetReloading(true, _config.ReloadTime);
            _audio?.PlayReloadSound();
        }
        else
        {
            _state.ShootCooldown = _config.FireRate;
        }
    }

    private void SpawnBullet(Vector3 target)
    {
        if (_config.BulletPrefab == null) return;

        // Определяем текущий ствол
        Transform currentShotOrigin = _usePrimaryBarrel || !_useDoubleBarrelTurret
            ? _shotOrigin
            : _shotOriginSecondary;

        if (currentShotOrigin == null) return;

        AiProjectile bullet = PoolManager.Instance.GetObject<AiProjectile>();
        if (bullet == null) return;

        Vector3 dir = _useForwardDirection
            ? currentShotOrigin.forward
            : (currentShotOrigin.position != target ? (target - currentShotOrigin.position).normalized : currentShotOrigin.forward);

        bullet.transform.position = currentShotOrigin.position;
        bullet.ClearTrail();
        bullet.gameObject.SetActive(true);
        bullet.Setup(dir, _config.BulletLifetime, _config.BulletSpeed);
    }

    private void TryDealDamage(Vector3 target)
    {
        if (_shotOrigin == null || _playerTransform == null) return;

        Transform currentShotOrigin = _usePrimaryBarrel || !_useDoubleBarrelTurret 
    ? _shotOrigin
    : _shotOriginSecondary;

        Vector3 origin = currentShotOrigin.position + currentShotOrigin.forward * 0.1f;
        Vector3 dir = _useForwardDirection
            ? currentShotOrigin.forward
            : (target - origin).normalized;
        bool hit = false;

        LayerMask mask = _obstacleLayer | _playerLayer;
        if (_hasManualTarget)
            mask |= 1 << _playerTransform.gameObject.layer;

        float chanceToHit = _hasManualTarget ? _config.ChanceToHitEnemy : _config.ChanceToHit;
        if (Random.value <= chanceToHit)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit rayHit, _config.AttackRange, mask))
            {
                if (rayHit.collider.TryGetComponent(out Damageable damageable))
                {
                    damageable.Damage(_config.Damage, gameObject);
                    hit = true;
                }
                else
                {
                    var enemyHealth = rayHit.collider.GetComponentInParent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Damage(_config.Damage, gameObject);
                        hit = true;
                    }
                }
                if (_debugger != null) _debugger.SetLastShot(origin, rayHit.point, hit);
                return;
            }
        }

        if (_debugger != null) _debugger.SetLastShot(origin, origin + dir * _config.AttackRange, hit);
    }

    private void ChangeMoving(bool isMoving)
    {
        if (isMoving)
            _navigation.Stop();
        else 
            _navigation.Resume();
    }
}