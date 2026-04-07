using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(EnemyState))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyPerception))]
[RequireComponent(typeof(EnemyNavigation))]

public abstract class EnemyBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform _playerTransform;
    [SerializeField] private BehaviorGraphAgent _behaviorAgent;

    [Header("Patrol / Idle")]
    public bool ShouldPatrol;
    public System.Collections.Generic.List<GameObject> PatrolPoints = new();
    public float WaypointWaitTime = 1f;

    public EnemyState        State       { get; private set; }
    public EnemyHealth       Health      { get; private set; }
    public EnemyPerception   Perception  { get; private set; }
    public EnemyNavigation   Navigation  { get; private set; }
    public IEnemyAnimator    Animator    { get; private set; }
    public IEnemyAudio       Audio       { get; private set; }
    public EnemyCoverModule  CoverModule { get; private set; }

    public Transform PlayerTransform => _playerTransform;

    public bool IsSpawnedBySpawner { get; set; }

    private Vector3 _initialPosition;

    protected virtual void Awake()
    {
        _initialPosition = transform.position;
        State       = GetComponent<EnemyState>();
        Health      = GetComponent<EnemyHealth>();
        Perception  = GetComponent<EnemyPerception>();
        Navigation  = GetComponent<EnemyNavigation>();
        Animator    = GetComponent<IEnemyAnimator>();
        Audio       = GetComponent<IEnemyAudio>();
        CoverModule = GetComponent<EnemyCoverModule>(); // null у MeleeEnemy

        if (_behaviorAgent == null)
            _behaviorAgent = GetComponent<BehaviorGraphAgent>();

        if (_playerTransform == null)
        {
            if(SceneLoader.instance == null || SceneLoader.instance.Player == null)
            {
                _playerTransform = FindFirstObjectByType<CharacterController>().transform;
            }
            else
            {
                _playerTransform = SceneLoader.instance.Player.transform;
            }
        }
    }

    protected virtual void Start()
    {
        ResolvePlayerTransform();

        Perception.Initialize(_playerTransform);
        CoverModule?.Initialize(_playerTransform);

        State.StartPosition = transform.position;

        Health.OnDeathInternal.AddListener(OnDeath);
        Health.OnDamaged.AddListener(OnDamaged);

        if (_playerTransform != null)
        {
            var playerDamageable = _playerTransform.GetComponent<Akila.FPSFramework.Damageable>();
            if (playerDamageable != null)
                playerDamageable.OnDeath.AddListener(OnPlayerDeath);
        }

        Navigation.Agent.speed = Navigation.WalkSpeed;

        Register();
        OnInitialized();
    }

    private void ResolvePlayerTransform()
    {
        if (_playerTransform != null) return;

        if (SceneLoader.instance?.Player != null)
            _playerTransform = SceneLoader.instance.Player.transform;
        else
        {
            var cc = FindFirstObjectByType<CharacterController>();
            if (cc != null) _playerTransform = cc.transform;
        }
    }

    protected virtual void OnInitialized() { }

    public abstract bool CanAttack();
    public abstract void StartAttack();

    protected virtual void OnDeath()
    {
        CoverModule?.ReleaseCover();

        if (_behaviorAgent != null)
            _behaviorAgent.enabled = false;

        Navigation.DisableAgent();
    }

    protected virtual void OnDamaged(float amount, GameObject source)
    {
        State.LastDamageTime = Time.time;
        Perception.InvestigateDamageSource(source);
    }

    protected virtual void OnPlayerDeath()
    {
        CoverModule?.ReleaseCover();
        State.PlayerDetected = false;
        State.IsFiring = false;
        State.IsAlerted = false;
        State.IsSearching = false;
        State.IsMeleeAttacking = false;
        Navigation.ResetPath();
        Animator?.PlayWinning();
    }

    public Node.Status OnPlayerDetected()
    {
        if (State.IsSearching)
            State.IsSearching = false;

        if (!State.IsAlerted)
            TriggerAlert();

        if (!State.PlayerDetected)
            Perception.StartDetection(() => { });

        return Node.Status.Success;
    }

    private void TriggerAlert()
    {
        State.IsAlerted = true;
        State.StartPosition = transform.position;
        State.TimeSinceLastSeen = 0f;
        Navigation.SetSpeed(Navigation.RunSpeed);

        Audio?.PlayDetectionSound();
        Animator?.SetAlerted(State.IsAlerted);

        Perception.StartDetection(() => {
            // Вызывается когда задержка обнаружения прошла
        });
    }

    public void Activate()
    {
        State.IsActivated = true;
    }

    public void ActivateWithBehavior()
    {
        if (_behaviorAgent != null)
            _behaviorAgent.enabled = true;
        Activate();
    }

    public void ActivateAlerted()
    {
        ActivateWithBehavior();
        if (_playerTransform != null)
            State.LastKnownPlayerPosition = _playerTransform.position;
        TriggerAlert();
        State.PlayerDetected = true;
    }

    public void AlertByGroup(Vector3 knownPlayerPos)
    {
        if (State.IsAlerted || State.IsDead || !State.IsActivated) return;

        State.LastKnownPlayerPosition = knownPlayerPos;
        State.TimeSinceLastSeen = 0f;
        TriggerAlert();
        State.PlayerDetected = true;
    }

    /// <summary>
    /// Немедленно агрит врага на игрока, минуя задержку обнаружения и визомер.
    /// Работает даже если враг ещё не активирован.
    /// </summary>
    public void AggroOnPlayer()
    {
        if (State.IsDead) return;

        if (!State.IsActivated)
            ActivateWithBehavior();

        if (_playerTransform != null)
            State.LastKnownPlayerPosition = _playerTransform.position;

        State.TimeSinceLastSeen = 0f;
        State.PlayerDetected    = true;
        State.PlayerIsSeen      = true;

        if (!State.IsAlerted)
            TriggerAlert();

        StartCoroutine(TrackPlayerUntilSeen());
    }

    /// <summary>
    /// Обновляет LastKnownPlayerPosition каждый кадр пока враг не видит игрока.
    /// Это нужно чтобы враг бежал к актуальной позиции игрока, а не к снимку момента агра.
    /// </summary>
    private System.Collections.IEnumerator TrackPlayerUntilSeen()
    {
        while (!State.IsDead && State.IsAlerted && !State.PlayerIsSeen)
        {
            if (_playerTransform != null)
                State.LastKnownPlayerPosition = _playerTransform.position;
            yield return null;
        }
    }

    public void AlertStarted()  => Navigation.Stop();
    public void AlertCompleted()
    {
        Navigation.Resume();
        Audio?.PlayAlertSound();
    }

    public Node.Status OnNoiseDetected()
    {
        if (State.HeardNoise && State.PlayerDetected)
        {
            State.HeardNoise = false;
            return Node.Status.Failure;
        }

        if (!State.HeardNoise && Perception.TryHearNoise())
        {
            Navigation.SetSpeed(Navigation.RunSpeed);
            State.StartPosition = transform.position;
            return Node.Status.Success;
        }

        return Node.Status.Failure;
    }

    public void HearNearbyShot(Vector3 shooterPosition)
    {
        if (State.IsDead || !State.IsActivated || State.PlayerDetected) return;

        Perception.HearNearbyShot(shooterPosition);
    }

    public bool IsEnemyStopped() => Navigation.HasReachedDestination();

    public Vector3 GetNoiseInvestigationTarget() => Perception.GetNoiseTarget();

    public void UpdateLastKnownPosition() => Perception.UpdateLastKnownPosition();

    protected virtual void Register()
    {
        if (EnemyCounter.Instance != null)
            EnemyCounter.Instance.Register(Health);
    }

    public virtual void FullReset()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        CoverModule?.ReleaseCover();

        State.ResetState(fireEvents: true);
        State.StartPosition = _initialPosition;

        Health.ResetHealth();

        ResolvePlayerTransform();
        Perception.Reset(_playerTransform);
        if (CoverModule != null)
            CoverModule.Initialize(_playerTransform);

        transform.position = _initialPosition;
        Navigation.Unlock();
        Navigation.EnableAgent();
        Navigation.MoveTo(_initialPosition, false);

        if (_behaviorAgent != null)
        {
            _behaviorAgent.enabled = false;
            _behaviorAgent.enabled = true;
        }

        Animator?.ResetAnimator();
    }
}