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
    [SerializeField] private float _visionHeightForNonTurret = 1.75f;
    [Tooltip("Куда метится LOS-луч по вертикали от центра капсулы игрока. 0.2 = чуть выше центра, чтобы перепрыгнуть укрытие по пояс")]
    [SerializeField] private float _losTargetHeightOffset = 1.5f;
    [Tooltip("Куда летит пуля по вертикали от центра капсулы игрока. 0 = центр (грудь/живот)")]
    [SerializeField] private float _playerAimHeight = 0f;

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
    private EnemyPerception    _perception;
    [Tooltip("Стрелять строго по направлению ствола. Вкл как туррель. Выкл для обычных врагов.")]
    [SerializeField] private bool _useForwardDirection = false;

    [Tooltip("Пропустить проверку LoS при стрельбе. Для турели с собственной системой обнаружения (CanSeeTarget уже проверяет LoS).")]
    [SerializeField] private bool _skipLosCheck = false;

    public bool _usePrimaryBarrel = true; // какой ствол в данный момент используется
    private bool _isPaused;
    private bool _hasManualTarget;
    private bool _targetIsEnemy;

    public event System.Action OnFire;

    private void Awake()
    {
        _state         = GetComponent<EnemyState>();
        _navigation    = GetComponent<EnemyNavigation>();
        _animator      = GetComponent<IEnemyAnimator>();
        _audio         = GetComponent<IEnemyAudio>();
        _debugger      = GetComponent<EnemyDebugger>();
        _grenadeModule = GetComponent<GrenadeThrowModule>();
        _perception    = GetComponent<EnemyPerception>();
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

    public void SetTarget(Transform target, bool isEnemy = false)
    {
        _playerTransform = target;
        _hasManualTarget = target != null;
        _targetIsEnemy   = target != null && isEnemy;
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

    private bool HasWeaponClearLineOfSight()
    {
        if (_playerTransform == null) return true;

        Vector3 origin;
        if (_useDoubleBarrelTurret)
        {
            Transform barrel = _usePrimaryBarrel ? _shotOrigin : _shotOriginSecondary;
            if (barrel == null) return true;
            origin = barrel.position;
        }
        else if (_useForwardDirection && _shotOrigin != null)
        {
            origin = _shotOrigin.position;
        }
        else
        {
            origin = transform.position + Vector3.up * _visionHeightForNonTurret;
        }

        Vector3 target = (_perception != null && _perception.UseMultiRay)
            ? _perception.LastVisibleAimPoint
            : _playerTransform.position + Vector3.up * _losTargetHeightOffset;
        Vector3 dir    = (target - origin).normalized;
        float   dist   = Vector3.Distance(origin, target) + 1f;

        LayerMask mask = _obstacleLayer | _playerLayer;
        if (_hasManualTarget)
            mask |= 1 << _playerTransform.gameObject.layer;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, mask))
            return hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform);

        return true;
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

        if (!_skipLosCheck && !HasWeaponClearLineOfSight())
        {
            StopFire();
            return;
        }

        _state.CurrentBullet++;
        _grenadeModule?.OnBulletFired();

        bool usingVisiblePoint = _perception != null && _perception.UseMultiRay;
        Vector3 target = usingVisiblePoint
            ? _perception.LastVisibleAimPoint
            : _playerTransform.position + Vector3.up * _playerAimHeight;
        target.y += Random.Range(-_config.HeightSprayOffset, _config.HeightSprayOffset) + (usingVisiblePoint ? 0f : _config.YOffset);
        target.x += Random.Range(-_config.WidthSprayOffset,  _config.WidthSprayOffset)  + _config.XOffset;

        SpawnBullet(target);
        if (_useDoubleBarrelTurret)
            _usePrimaryBarrel = !_usePrimaryBarrel;
        TryDealDamage(target);
        _audio?.PlayAttackSound();

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

        Transform currentShotOrigin = _usePrimaryBarrel || !_useDoubleBarrelTurret
            ? _shotOrigin
            : _shotOriginSecondary;

        if (currentShotOrigin == null) return;

        AiProjectile bullet;
        if (_config.BulletPrefab.GetComponent<EffectedProjectile>() != null)
            bullet = PoolManager.Instance.GetObject<EffectedProjectile>();
        else
            bullet = PoolManager.Instance.GetObject<AiProjectile>();
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
        if (_playerTransform == null) return;

        Transform originSource;
        float forwardOffset = 0.1f;

        if (_useDoubleBarrelTurret)
        {
            originSource = _usePrimaryBarrel ? _shotOrigin : _shotOriginSecondary;
        }
        else
        {
            originSource = transform;
            forwardOffset = 0.1f;
        }

        Vector3 origin;
        if (_useDoubleBarrelTurret)
        {
            origin = originSource.position + originSource.forward * forwardOffset;
        }
        else
        {
            origin = originSource.position + Vector3.up * _visionHeightForNonTurret;
        }

        Vector3 dir = _useForwardDirection
            ? originSource.forward
            : (target - origin).normalized;

        bool hit = false;

        LayerMask mask = _obstacleLayer | _playerLayer;
        if (_hasManualTarget)
            mask |= 1 << _playerTransform.gameObject.layer;

        float chanceToHit = _targetIsEnemy ? _config.ChanceToHitEnemy : _config.ChanceToHit;
        if (Random.value <= chanceToHit)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit rayHit, _config.AttackRange, mask))
            {
                if (rayHit.collider.TryGetComponent(out Damageable damageable))
                {
                    damageable.Damage(_config.Damage * DifficultyManager.EnemyDamageMult, gameObject);
                    hit = true;
                }
                else
                {
                    var enemyHealth = rayHit.collider.GetComponentInParent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Damage(_config.Damage, gameObject);
                        enemyHealth.OnHitInChildren(new HitInfo(gameObject, rayHit, origin, dir));
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