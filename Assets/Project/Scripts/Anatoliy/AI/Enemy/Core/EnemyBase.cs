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

    public EnemyState      State       { get; private set; }
    public EnemyHealth     Health      { get; private set; }
    public EnemyPerception Perception  { get; private set; }
    public EnemyNavigation Navigation  { get; private set; }
    public IEnemyAnimator  Animator    { get; private set; }
    public IEnemyAudio     Audio       { get; private set; }

    public Transform PlayerTransform => _playerTransform;

    private bool _cachedIsAlerted = false;

    protected virtual void Awake()
    {
        State      = GetComponent<EnemyState>();
        Health     = GetComponent<EnemyHealth>();
        Perception = GetComponent<EnemyPerception>();
        Navigation = GetComponent<EnemyNavigation>();
        Animator   = GetComponent<IEnemyAnimator>();
        Audio      = GetComponent<IEnemyAudio>();

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
        if (_behaviorAgent != null)
            _behaviorAgent.enabled = false;

        Navigation.DisableAgent();
    }

    protected virtual void OnDamaged(float amount, GameObject source)
    {
        Perception.InvestigateDamageSource(source);
    }

    protected virtual void OnPlayerDeath()
    {
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
        {
            State.IsSearching = false;
            TriggerAlert();
            return Node.Status.Success;
        }

        if (!State.IsAlerted)
        {
            _cachedIsAlerted = State.IsAlerted;

            TriggerAlert();
            return Node.Status.Success;
        }

        return Node.Status.Failure;
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

    public void AlertStarted()  => Navigation.Stop();
    public void AlertCompleted() => Navigation.Resume();

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

    public bool IsEnemyStopped() => Navigation.HasReachedDestination();

    public Vector3 GetNoiseInvestigationTarget() => Perception.GetNoiseTarget();

    public void UpdateLastKnownPosition() => Perception.UpdateLastKnownPosition();

    protected virtual void Register()
    {
        // EnemyCounter.Instance?.Register(this); // get rid of this
    }

    public virtual void FullReset()
    {
        State.ResetState();
        Health.ResetHealth();

        Navigation.EnableAgent();
        Navigation.MoveTo(State.StartPosition, false);

        if (_behaviorAgent != null)
            _behaviorAgent.enabled = true;

        Animator?.ResetAnimator();
    }
}