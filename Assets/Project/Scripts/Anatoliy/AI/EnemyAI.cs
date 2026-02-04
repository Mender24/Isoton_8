using Akila.FPSFramework;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour, IDamageable
{
    public enum CombatType { Ranged, Melee }

    [Header("References")]
    public NavMeshAgent agent;
    public BasicEnemyAnimationController animationController;
    public Transform playerTransform;
    [SerializeField] private string mainCameraTag = "MainCamera";
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Vision Settings")]
    [SerializeField] private float _fieldOfViewAngle = 110f;
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionHeight = 0.5f;

    [Header("Hearing Settings")]
    public float hearingRange = 10f;
    [SerializeField] private float _soundDetectionThreshold = 0.5f;
    [SerializeField] private float _noiseInvestigationTime = 8f;

    [Header("Combat Settings")]
    public CombatType combatType = CombatType.Ranged;
    public float forgetTime = 10f;
    public float stoppingDistance = 0.5f;
    [SerializeField] private float _health = 100;
    [SerializeField] private float _detectionDelay = 0.9f;

    [Header("Melee Combat Settings")]
    // public float meleeAttackTime = 2f;
    [SerializeField] private float _meleeAttackRange = 2f;
    public float meleeAttackLength = 3.0f;
    [SerializeField] private float _meleeAttackCooldown = 1.5f;
    [SerializeField] private float _meleeDamage = 20f;
    [SerializeField] private LayerMask _meleeHitLayers;
    [SerializeField] private Vector3 _meleeAttackOffset = new Vector3(0, 1f, 1f);
    [SerializeField] private float _meleeAttackRadius = 1f;

    [Header("Range Combat Settings")]
    public float attackRange = 20f;
    [SerializeField] private GameObject _prefabBullet;
    [SerializeField] private Transform _shotStartTransform;
    [SerializeField] private float _damage = 5f;
    [SerializeField] private float _speedBullet;
    [SerializeField] private int _countBullet = 20;
    [Range(0, 1)]
    [SerializeField] private float _chanceToHit = 0.9f;
    [SerializeField] private float _timeBetweenShot = 0.5f;
    public float timeReload = 3f;
    [SerializeField] private float _bulletLifetime = 2f;

    [Header("Shoot Target Offset")]
    [SerializeField] private float _xShootTargetOffset = 0;
    [SerializeField] private float _yShootTargetOffset = 1;

    [Header("Spray Shoot Offset")]
    [SerializeField] private float _heightSprayOffset = 0.25f;
    [SerializeField] private float _widthSprayOffset = 0.25f;

    [Header("Damage Reaction Settings")]
    [Tooltip("Минимальное время между реакциями на урон (анимация/стан)")]
    [SerializeField] private float _damageReactionCooldown = 3f;
    [Tooltip("Включить кулдаун реакции на урон")]
    [SerializeField] private bool _enableDamageReactionCooldown = true;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Patrol/Idle")]
    public bool shouldPatrol = false;
    public List<GameObject> patrolPoints = new();
    public float waypointWaitTime = 1f;
    public Vector3 startPosition = new();

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _detectionSound;
    [SerializeField] private List<AudioClip> _footstepSounds;

    [Header("Debug Settings")]
    [SerializeField] private bool _showDebug = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _showDebugInfo = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _showDebugLogs = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _debugVision = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showDetectionRange = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showFieldOfView = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showVisionRaycast = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showShootingRaycast = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showLastKnownPosition = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showFieldOfView))]
    [SerializeField] private int _fovResolution = 10; // Количество линий для отрисовки FOV

    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showDetectionRange))]
    [SerializeField] private Color _detectionRangeColor = new Color(1f, 1f, 0f, 0.1f);
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showDetectionRange))]
    [SerializeField] private Color _alertColor = Color.red;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showFieldOfView))]
    [SerializeField] private Color _fieldOfViewColor = new Color(0f, 1f, 0f, 0.2f);
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showVisionRaycast))]
    [SerializeField] private Color _raycastVisionHitColor = Color.green;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showVisionRaycast))]
    [SerializeField] private Color _raycastVisionMissColor = Color.red;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showShootingRaycast))]
    [SerializeField] private Color _raycastShotHitColor = Color.blue;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showShootingRaycast))]
    [SerializeField] private Color _raycastShotMissColor = Color.cyan;

    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _debugHearing = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugHearing))]
    [SerializeField] private bool _showHearingRange = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugHearing), nameof(_showHearingRange))]
    [SerializeField] private Color _hearingRangeColor = new Color(0f, 0.5f, 1f, 0.1f);

    // AI States
    public bool isActivated = false;
    [HideInInspector] public bool playerDetected = false;
    [HideInInspector] public bool isAlerted = false;
    [HideInInspector] public bool isSearching = false;
    [HideInInspector] public Vector3 lastKnownPlayerPosition;
    [HideInInspector] public float timeSinceLastSeen = 0f;
    [HideInInspector] public bool isMeleeAttacking = false;
    [HideInInspector] public float meleeAttackTimer = 0f;

    [HideInInspector] public bool isReload = false;
    [HideInInspector] public bool isFire = false;
    [HideInInspector] public float timeShoot = 0f;
    [HideInInspector] public int currentBullet = 0;
    // [HideInInspector] public int is = 0;
    [HideInInspector] public Vector3 lastHeardNoisePosition;
    [HideInInspector] public bool heardNoise = false;
    [HideInInspector] public bool isDead = false;
    private float _noiseInvestigationTimer = 0f;
    private AudioSource _lastHeardAudioSource;
    private float _lastDamageReactionTime = -999f;

    private bool _detectionDelayActive = false;
    private bool _debugIsPlayerHit = false;
    private Vector3 _debugShotTargetPosition = new();
    private Camera _mainCamera = null;

    public float Health { get => _health; set => _health = value; }
    public bool isDamagableDisabled { get; set; }
    public bool allowDamageableEffects { get; set; }
    public bool DeadConfirmed { get; set; }
    public GameObject DamageSource { get; set; }

    public UnityEvent OnDeath { get; set; }

    void Start()
    {
        if (playerTransform == null) 
        {
            if(SceneLoader.instance == null /*|| SceneLoader.instance.Player == null*/)
            {
                playerTransform = FindFirstObjectByType<CharacterController>().transform;
            }
            // else
            // {
            //     playerTransform = SceneLoader.instance.Player.transform;
            // }
        }
        
        if (_mainCamera == null)
        {
            var cameras = playerTransform.GetComponentsInChildren<Camera>();
            foreach (var cam in cameras)
            {
                if (cam.tag == mainCameraTag)
                {
                    _mainCamera = cam;
                    break;
                }
            }
        }

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animationController == null) animationController = GetComponent<BasicEnemyAnimationController>();
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

        if (playerTransform != null)
        {
            playerTransform.GetComponent<Damageable>().OnDeath.AddListener(OnPlayerDeath);
        }
        agent.speed = walkSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (!isActivated) return;

        if (isAlerted && !CanSeePlayer())
        {
            timeSinceLastSeen += Time.deltaTime;

            if (timeSinceLastSeen >= forgetTime)
            {
                ForgetPlayer();
            }
        }

        if (combatType == CombatType.Melee && isMeleeAttacking)
        {
            meleeAttackTimer -= Time.deltaTime;
            if (meleeAttackTimer <= 0)
            {
                isMeleeAttacking = false;
            }
        }

        UpdateNoiseInvestigation();

        if(combatType == CombatType.Ranged && isFire && timeShoot <= 0)
        {
            Fire();
        }

        timeShoot -= Time.deltaTime;
    }

    public void StartFire()
    {
        isFire = true;
    }

    private void Fire()
    {
        if (isReload)
            isReload = false;

        if (!playerDetected)
        {
            isFire = false;
            return;
        }

        currentBullet++;

        Vector3 targetPosition = playerTransform.position;
        float yRandomSprayOffset = Random.Range(-_heightSprayOffset, _heightSprayOffset);
        float xRandomSprayOffset = Random.Range(-_widthSprayOffset, _widthSprayOffset);

        targetPosition.y += yRandomSprayOffset + _yShootTargetOffset;
        targetPosition.x += xRandomSprayOffset + _xShootTargetOffset;

        Vector3 bulletStartPosition = _shotStartTransform.position;//transform.position + _agentCenterOffset;

        AiProjectile bullet = PoolManager.Instance.GetObgect<AiProjectile>();
        bullet.transform.position = bulletStartPosition;
        bullet.gameObject.SetActive(true);
        bullet.Setup((targetPosition - bulletStartPosition).normalized, _bulletLifetime, _speedBullet);

        targetPosition.y -= yRandomSprayOffset;
        targetPosition.x -= xRandomSprayOffset;

        float hitChance = Random.Range(0, 1f);

        if (hitChance <= _chanceToHit)
        {
            Vector3 shootDirection = (targetPosition - bulletStartPosition).normalized;
            RaycastHit hit;

            if (Physics.Raycast(bulletStartPosition + transform.forward, shootDirection, out hit, attackRange, _playerLayer))
            {
                if (hit.collider.gameObject.TryGetComponent(out Damageable actor))
                {
                    actor.Damage(_damage);

                    if (_showDebug && _showShootingRaycast)
                    {
                        _debugIsPlayerHit = true;
                        _debugShotTargetPosition = hit.point;
                    }
                }
            }
        }
        else if (_showDebug && _showShootingRaycast)
        {
            _debugIsPlayerHit = false;
            _debugShotTargetPosition = targetPosition;
        }

        if (currentBullet >= _countBullet)
        {
            timeShoot = timeReload;
            isReload = true;
            currentBullet = 0;
        }
        else
        {
            timeShoot = _timeBetweenShot;
        }
    }

    public bool IsInMeleeRange()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= _meleeAttackRange;
    }

    public void StartMeleeAttack()
    {
        if (meleeAttackTimer > 0 || !playerDetected) return;
        
        isMeleeAttacking = true;
        meleeAttackTimer = _meleeAttackCooldown;
        animationController.SetMeleeAttackType();
    }

    public void ExecuteMeleeAttack()
    {
        if (playerTransform == null) return;
        
        Vector3 attackPosition = transform.position + transform.forward * _meleeAttackOffset.z + 
                                Vector3.up * _meleeAttackOffset.y;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, _meleeAttackRadius, _meleeHitLayers);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform == playerTransform)
            {
                if (hitCollider.TryGetComponent(out Damageable damageable))
                {
                    damageable.Damage(_meleeDamage);
                    
                    if (_showDebugLogs)
                        Debug.Log($"Melee hit player for {_meleeDamage} damage");
                }
                break;
            }
        }
    }

    public void AlertStarted()
    {
        agent.isStopped = true;
    }

    public void AlertCompleted()
    {
        agent.isStopped = false;
    }

    public bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        if (_mainCamera == null) return false;
        Vector3 rayDirection = (_mainCamera.transform.position - transform.position - new Vector3(0, _visionHeight, 0)).normalized;
        float rayDistance = GetDistanceToMainCamera();

        if (rayDistance > _visionRange) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, rayDirection);

        if (angleToPlayer > _fieldOfViewAngle / 2f)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0, _visionHeight, 0), rayDirection, out hit, rayDistance, _obstacleLayer | _playerLayer))
        {
            
            if (hit.transform == playerTransform)
                return true;
        }
        return false;
    }

    public Node.Status OnPlayerDetected()
    {
        if (isSearching)
        {
            isSearching = false;

            StartDetection();

            return Node.Status.Success;
        }

        if (!isAlerted)
        {
            StartDetection();

            return Node.Status.Success;
        }

        return Node.Status.Failure;
    }

    private void StartDetection()
    {
        isAlerted = true;
        // lastKnownPlayerPosition = playerTransform.position;
        startPosition = transform.position;
        timeSinceLastSeen = 0f;
        agent.speed = runSpeed;
        StartCoroutine(DetectionDelayCoroutine());
    }

    private System.Collections.IEnumerator DetectionDelayCoroutine()
    {
        _detectionDelayActive = true;

        if (_audioSource && _detectionSound)
        {
            _audioSource.PlayOneShot(_detectionSound);
        }

        yield return new WaitForSeconds(_detectionDelay);

        playerDetected = true;

        _detectionDelayActive = false;
    }

    public void UpdateLastKnownPosition()
    {
        if (playerTransform != null)
        {
            timeSinceLastSeen = 0f;
            StartCoroutine(UpdateLastKnownPositionDelay());
        }
    }

    private System.Collections.IEnumerator UpdateLastKnownPositionDelay()
    {
        yield return new WaitForSeconds(1f);
        if (playerTransform != null)
            lastKnownPlayerPosition = playerTransform.position;
    }

    public void ForgetPlayer()
    {
        isAlerted = false;
        playerDetected = false;
        timeSinceLastSeen = 0f;
        agent.speed = walkSpeed;

        _lastDamageReactionTime = -999f;
    }

    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, playerTransform.position);
    }

    public float GetDistanceToMainCamera()
    {
        if (_mainCamera == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position + new Vector3(0, _visionHeight, 0), _mainCamera.transform.position);
    }

    public void Damage(float amount, GameObject damageSource)
    {
        _health -= amount;
        
        DetectAttacker(damageSource);

        bool canReact = !_enableDamageReactionCooldown || 
                        (Time.time - _lastDamageReactionTime >= _damageReactionCooldown);
        
        if (canReact)
        {
            _lastDamageReactionTime = Time.time;
            
            animationController?.PlayHit();
            
            if (_showDebugLogs)
                Debug.Log($"[EnemyAI] Took {amount} damage and played hit reaction");
        }
        else
        {
            if (_showDebugLogs)
            {
                float cooldownRemaining = _damageReactionCooldown - (Time.time - _lastDamageReactionTime);
                Debug.Log($"[EnemyAI] Took {amount} damage but ignored reaction (cooldown: {cooldownRemaining:F1}s)");
            }
        }
        
        if (_health <= 0)
            Die();
    }

    private void DetectAttacker(GameObject damageSource)
    {
        if (isActivated && playerDetected)
        {
            UpdateLastKnownPosition();
            return;
        }
        
        if (!isActivated)
        {
            isActivated = true;
            
            if (_showDebugLogs)
                Debug.Log("[EnemyAI] Activated by taking damage");
        }
        
        if (CanSeePlayer())
        {
            if (!isAlerted)
            {
                StartDetection();
                
                if (_showDebugLogs)
                    Debug.Log("[EnemyAI] Player detected after taking damage (visual)");
            }
            else
            {
                UpdateLastKnownPosition();
            }
        }
        else
        {
            InvestigateDamageSource(damageSource);
        }
    }

    private void InvestigateDamageSource(GameObject damageSource)
    {
        if (damageSource != null)
        {
            Transform sourceTransform = damageSource.transform;
            
            Vector3 suspiciousPosition = sourceTransform.position;
            
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(suspiciousPosition, playerTransform.position);
                
                if (distanceToPlayer < 10f)
                {
                    suspiciousPosition = playerTransform.position;
                    playerDetected = true;
                }
            }
            
            if (!isAlerted)
            {
                isAlerted = true;
                startPosition = transform.position;
                agent.speed = runSpeed;
                
                if (_showDebugLogs)
                    Debug.Log("[EnemyAI] Alerted by damage, investigating source");
            }
            
            lastKnownPlayerPosition = suspiciousPosition;
            heardNoise = true;
            lastHeardNoisePosition = suspiciousPosition;
            _noiseInvestigationTimer = _noiseInvestigationTime;
            
            if (_showDebugLogs)
                Debug.Log($"[EnemyAI] Investigating damage from position: {suspiciousPosition}");
        }
        else
        {
            if (!isAlerted)
            {
                isAlerted = true;
                startPosition = transform.position;
                agent.speed = runSpeed;
                
                if (_showDebugLogs)
                    Debug.Log("[EnemyAI] Alerted by damage from unknown source");
            }
        }
    }

    public bool IsSwaped()
    {
        return true;
    }

    private void Die()
    {
        isDead = true;
        startPosition = transform.position;
        isActivated = false;
        playerDetected = false;
        isFire = false;
        isAlerted = false;
        isSearching = false;
        isMeleeAttacking = false;
        agent.isStopped = true;
        agent.ResetPath();

        var cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            col.enabled = false;
        }

        animationController?.SetDead(true);
    }

    public void OnDeathComplete()
    {
        Debug.Log("Enemy death animation complete");
        // gameObject.SetActive(false);
    }

    public void PlayFootstepSound(int foot)
    {
        if (_audioSource != null && _footstepSounds.Count > 0)
        {
            int rool = Random.Range(0, _footstepSounds.Count - 1);
            _audioSource.PlayOneShot(_footstepSounds[rool]);
        }

        if (_showDebugLogs)
            Debug.Log($"Footstep {foot}");
    }

    public bool CanHearPlayer(float noiseRange)
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= noiseRange;
    }

    public bool CanHearNoise()
    {
        if (playerDetected) return false;
        
        if (GetDistanceToPlayer() < hearingRange)
        {
            AudioSource[] audioSources = playerTransform.GetComponentsInChildren<AudioSource>();
            
            foreach (var audioSource in audioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    if (audioSource.volume >= _soundDetectionThreshold)
                    {
                        lastHeardNoisePosition = audioSource.transform.position;
                        _lastHeardAudioSource = audioSource;
                        return true;
                    }
                }
            }
            
        }

        return false;
    }

    public Node.Status OnNoiseDetected()
    {
        if (heardNoise && playerDetected)
        {
            heardNoise = false;
            return Node.Status.Failure;
        }

        if (!heardNoise && CanHearNoise())
        {
            heardNoise = true;
            _noiseInvestigationTimer = _noiseInvestigationTime;
            agent.speed = runSpeed;
            startPosition = transform.position;

            if (_showDebugLogs)
                Debug.Log($"Enemy heard noise at: {lastHeardNoisePosition}");

            return Node.Status.Success;
        }

        return Node.Status.Failure;
    }

    public void UpdateNoiseInvestigation()
    {
        if (heardNoise)
        {
            _noiseInvestigationTimer -= Time.deltaTime;

            if (_noiseInvestigationTimer <= 0)
            {
                heardNoise = false;
                agent.speed = walkSpeed;
            }
            else if (playerDetected)
            {
                heardNoise = false;
                agent.speed = walkSpeed;
            }
        }
    }

    public Vector3 GetNoiseInvestigationTarget()
    {
        return lastHeardNoisePosition;
    }

    public bool IsEnemyStopped()
    {
        if (agent != null)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
        }
            
        return false;
    }

    private void OnPlayerDeath()
    {
        playerDetected = false;
        isFire = false;
        isAlerted = false;
        isSearching = false;
        isMeleeAttacking = false;
        agent.ResetPath();
        animationController.PlayWinning();
    }

    void OnDrawGizmos()
    {
        // To make only main camera and scene view draw gizmos
        if (Camera.current.tag == "MainCamera" || Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera)
        {
            if (_showDebug)
            {
                Vector3 position = transform.position + new Vector3(0, _visionHeight, 0);

                if (_debugHearing)
                {
                    if (_showHearingRange)
                    {
                        Gizmos.color = _hearingRangeColor;
                        Gizmos.DrawSphere(position, hearingRange);
                    }
                }

                if (_debugVision)
                {
                    if (_showDetectionRange)
                    {
                        Gizmos.color = _detectionRangeColor;
                        Gizmos.DrawSphere(position, _visionRange);

                        Gizmos.color = isAlerted ? _alertColor : Color.yellow;
                        DrawCircle(position, _visionRange, 50);
                    }

                    if (_showFieldOfView)
                    {
                        DrawFieldOfView(position);
                    }

                    if (_showVisionRaycast && playerTransform != null)
                    {
                        DrawVisionRaycast(position);
                    }

                    if (_showShootingRaycast && combatType == CombatType.Ranged)
                    {
                        DrawShootRaycast(position);
                    }

                    if (_showLastKnownPosition && isAlerted)
                    {
                        DrawLastKnownPosition();
                    }

                    if (_debugVision && combatType == CombatType.Melee)
                    {
                        Gizmos.color = Color.red;
                        Vector3 meleePos = transform.position + transform.forward * _meleeAttackOffset.z + 
                                        Vector3.up * _meleeAttackOffset.y;
                        Gizmos.DrawWireSphere(meleePos, _meleeAttackRadius);
                        
                        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                        Gizmos.DrawWireSphere(transform.position + Vector3.up, _meleeAttackRange);
                    }

                    // Направление взгляда
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(position, transform.forward * 2f);
                }
            }
        }
    }

    void DrawFieldOfView(Vector3 position)
    {
        float halfFOV = _fieldOfViewAngle / 2f;

        Color fovColor = _fieldOfViewColor;
        if (isAlerted)
            fovColor = new Color(1f, 0f, 0f, 0.2f);
        else if (CanSeePlayer())
            fovColor = new Color(1f, 0.5f, 0f, 0.3f);

        Gizmos.color = fovColor;

        // Конус обзора
        Vector3 leftBoundary = Quaternion.Euler(0, -halfFOV, 0) * transform.forward * _visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, halfFOV, 0) * transform.forward * _visionRange;

        // Левая граница
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(position, position + leftBoundary);

        // Правая граница
        Gizmos.DrawLine(position, position + rightBoundary);

        // Заполнение конуса линиями
        Vector3 previousPoint = position + leftBoundary;

        for (int i = 0; i <= _fovResolution; i++)
        {
            float angle = -halfFOV + (_fieldOfViewAngle * i / _fovResolution);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 point = position + direction * _visionRange;

            // Линия от центра
            Gizmos.color = new Color(fovColor.r, fovColor.g, fovColor.b, 0.3f);
            Gizmos.DrawLine(position, point);

            // Линия дуги
            Gizmos.color = Color.cyan;
            if (i > 0)
                Gizmos.DrawLine(previousPoint, point);

            previousPoint = point;
        }
    }

    void DrawVisionRaycast(Vector3 position)
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = (Camera.current.transform.position - position).normalized;
        float distanceToPlayer = Vector3.Distance(position, playerTransform.position);

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        bool inFOV = angleToPlayer <= _fieldOfViewAngle / 2f;

        RaycastHit hit;
        if (Physics.Raycast(position, directionToPlayer, out hit, distanceToPlayer,
            _obstacleLayer | _playerLayer))
        {
            bool hitPlayer = hit.transform == playerTransform;

            // Цвет луча
            if (hitPlayer && inFOV)
                Gizmos.color = _raycastVisionHitColor;
            else
                Gizmos.color = _raycastVisionMissColor;

            // Луч до препятствия
            Gizmos.DrawLine(position, hit.point);

            // Точка попадания
            Gizmos.DrawSphere(hit.point, 0.1f);

            // Если попали не в игрока, рисуем что заблокировало
            if (!hitPlayer)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.1f);
            }
        }
        else
        {
            // Луч до игрока если нет препятствий
            Gizmos.color = inFOV ? _raycastVisionHitColor : _raycastVisionMissColor;
            Gizmos.DrawLine(position, playerTransform.position);
        }

        // Индикатор угла обзора
        if (!inFOV)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, 0.5f);
        }
    }

    void DrawShootRaycast(Vector3 position)
    {
        if (_debugIsPlayerHit)
        {
            Gizmos.color = _raycastShotHitColor;
            Gizmos.DrawLine(position, _debugShotTargetPosition);
        }
        else
        {
            Gizmos.color = _raycastShotMissColor;
            Gizmos.DrawLine(position, _debugShotTargetPosition);
        }
    }

    void DrawLastKnownPosition()
    {
        // Маркер последней известной позиции
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        Gizmos.DrawLine(lastKnownPlayerPosition, lastKnownPlayerPosition + Vector3.up * 2f);

        // Крестик
        Gizmos.DrawLine(lastKnownPlayerPosition + Vector3.left * 0.5f, lastKnownPlayerPosition + Vector3.right * 0.5f);
        Gizmos.DrawLine(lastKnownPlayerPosition + Vector3.forward * 0.5f, lastKnownPlayerPosition + Vector3.back * 0.5f);

        // Таймер забывания
        float forgetProgress = timeSinceLastSeen / forgetTime;
        Gizmos.color = Color.Lerp(Color.red, Color.yellow, forgetProgress);
        Gizmos.DrawWireSphere(lastKnownPlayerPosition + Vector3.up * 2f, 0.3f * (1f - forgetProgress));
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

    // Для отображения в Game View
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // To make only main camera and scene view draw gizmos
        if (Camera.current.tag == "MainCamera" || Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera)
        {
            if (_showDebug && _showDebugInfo)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, GetDebugInfo());
        }
#endif
    }

    string GetDebugInfo()
    {
        string info = $"Enemy AI Debug\n";
    info += $"Activated: {isActivated}\n";
    info += $"Alerted: {isAlerted}\n";
    info += $"Searching: {isSearching}\n";
    info += $"Detected player: {playerDetected}\n";
    info += $"Started position: {startPosition}\n";
    info += $"Last Known Position: {lastKnownPlayerPosition}\n";

    if (playerTransform != null)
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        info += $"Distance to player: {distance:F1}m\n";
        info += $"Angle to player: {angle:F1}°\n";
    }

    if (isAlerted)
    {
        info += $"Time Since Seen: {timeSinceLastSeen:F1}s / {forgetTime}s\n";
        
        if (combatType == CombatType.Ranged)
        {
            info += $"Firing: {isFire}\n";
            info += $"Reloading: {isReload}\n";
            info += $"Bullets in magazine: {_countBullet - currentBullet}\n";
        }
        else if (combatType == CombatType.Melee)
        {
            info += $"Melee Attacking: {isMeleeAttacking}\n";
            info += $"Attack Cooldown: {meleeAttackTimer:F1}s\n";
        }
    }

    if (_enableDamageReactionCooldown)
    {
        float timeSinceReaction = Time.time - _lastDamageReactionTime;
        bool canReact = timeSinceReaction >= _damageReactionCooldown;
        
        info += $"\nDamage Reaction: {(canReact ? "READY" : "ON COOLDOWN")}\n";
        if (!canReact)
        {
            float cooldownRemaining = _damageReactionCooldown - timeSinceReaction;
            info += $"Cooldown: {cooldownRemaining:F1}s\n";
        }
    }

    return info;
    }
}