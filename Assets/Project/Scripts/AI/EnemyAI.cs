using Akila.FPSFramework;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform playerTransform;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Vision Settings")]
    [SerializeField] private float _fieldOfViewAngle = 110f;
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionHeight = 0.5f;

    [Header("Hearing Settings")]
    public float hearingRange = 10f;

    [Header("Combat Settings")]
    [SerializeField] private GameObject _prefabBullet;
    [SerializeField] private float _speedBullet;
    [SerializeField] private int _countBullet = 20;
    [SerializeField] private float _percentage;
    [SerializeField] private float _timeBetweenShoot = 0.5f;
    [SerializeField] private float _timeReload = 10f;
    public float attackRange = 20f;
    public float forgetTime = 10f;
    [SerializeField] private float _detectionDelay = 0.25f;
    [Header("OffSet")]
    [SerializeField] private float _xValue; // const offset
    [SerializeField] private float _yValue = 1;
    [Header("SizeColliderTarget")]
    [SerializeField] private float _height = 0.5f; // random offset
    [SerializeField] private float _width = 2;
    [SerializeField] private GameObject _testPoint;
    [SerializeField] private float _value;


    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Patrol/Idle")]
    public bool shouldPatrol = false;
    public List<GameObject> patrolPoints = new();
    public float waypointWaitTime = 1f;
    [SerializeField] private float _idleWaitTime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _detectionSound;

    [Header("Debug Settings")]
    [SerializeField] private bool _showDebug = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _showDebugInfo = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _debugVision = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showDetectionRange = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showFieldOfView = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision))]
    [SerializeField] private bool _showRaycast = false;
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
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showRaycast))]
    [SerializeField] private Color _raycastHitColor = Color.green;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugVision), nameof(_showRaycast))]
    [SerializeField] private Color _raycastMissColor = Color.red;


    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug))]
    [SerializeField] private bool _debugHearing = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugHearing))]
    [SerializeField] private bool _showHearingRange = false;
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.And, nameof(_showDebug), nameof(_debugHearing), nameof(_showHearingRange))]
    [SerializeField] private Color _hearingRangeColor = new Color(0f, 0.5f, 1f, 0.1f);

    // AI States
    [HideInInspector] public bool isActivated = false;
    [HideInInspector] public bool playerDetected = false;
    [HideInInspector] public bool isAlerted = false;
    [HideInInspector] public bool isSearching = false;
    [HideInInspector] public Vector3 lastKnownPlayerPosition;
    [HideInInspector] public float timeSinceLastSeen = 0f;

    [HideInInspector] public bool _isReload = false; // Reload Status
    [HideInInspector] public bool _isFire = false;
    [HideInInspector] public float timeShoot = 0f;
    [HideInInspector] public int currentBullet = 0;

    private bool _detectionDelayActive = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (!isActivated) return;

        // Обновление таймера забывания
        if (isAlerted && !CanSeePlayer())
        {
            timeSinceLastSeen += Time.deltaTime;

            if (timeSinceLastSeen >= forgetTime)
            {
                ForgetPlayer();
            }
        }

        if(_isFire && timeShoot <= 0)
        {
            Fire();
        }

        timeShoot -= Time.deltaTime;
    }

    public void StartFire()
    {
        _isFire = true;
    }

    private void Fire()
    {
        if (!playerDetected)
            _isFire = false;

        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit _, 100, LayerMask.GetMask("Player")))
            return;

        //Visualisation

        currentBullet++;

        Vector3 targetPosition = playerTransform.position;
        float yValue = Random.Range(-_height, _height);
        float xValue = Random.Range(-_width, _width);

        targetPosition.y += yValue + _yValue;
        targetPosition.x += xValue + _xValue;

        GameObject newBullet = Instantiate(_prefabBullet, transform);
        Bullet bullet = newBullet.GetComponent<Bullet>();
        _testPoint.transform.position = transform.position + transform.forward * _value;
        bullet.transform.position = _testPoint.transform.position;
        bullet.Init(2, (targetPosition - transform.position).normalized, _speedBullet);

        //Shoot

        targetPosition.y -= yValue;
        targetPosition.x -= xValue;

        float isShoot = Random.Range(0, 1f);

        if (isShoot <= _percentage)
        {
            RaycastHit hit;


            Debug.Log(Physics.Raycast(transform.position, (targetPosition - transform.position).normalized, out hit, 100) && hit.collider.gameObject.TryGetComponent(out Actor _));


            if (Physics.Raycast(transform.position, (targetPosition - transform.position).normalized, out hit, 100) && hit.collider.gameObject.TryGetComponent(out Actor actor))
            {
                Debug.DrawRay(transform.position, (targetPosition - transform.position).normalized * 100, Color.blue, 0.5f);
                Debug.Log("Target: " + hit.collider.gameObject.name + " Hit: " + (isShoot));
            }
        }

        if (currentBullet >= _countBullet)
        {
            timeShoot = _timeReload;
            currentBullet = 0;
        }
        else
        {
            timeShoot = _timeBetweenShoot;
        }
    }

    public bool CanSeePlayer()
    {
        Vector3 rayDirection = (playerTransform.position - transform.position).normalized;
        float rayDistance = GetDistanceToPlayer();

        if (rayDistance > _visionRange) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, rayDirection);

        if (angleToPlayer > _fieldOfViewAngle / 2f)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, rayDirection, out hit, rayDistance, _obstacleLayer | _playerLayer))
        {
            if (hit.transform == playerTransform)
                return true;
        }
        return false;
    }

    public bool CanHearPlayer(float noiseRange)
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= noiseRange;
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
        lastKnownPlayerPosition = playerTransform.position;
        timeSinceLastSeen = 0f;
        agent.speed = runSpeed;
        StartCoroutine(DetectionDelayCoroutine());
    }

    private System.Collections.IEnumerator DetectionDelayCoroutine()
    {
        _detectionDelayActive = true;

        // Звуковое оповещение
        if (_audioSource && _detectionSound)
        {
            _audioSource.PlayOneShot(_detectionSound);
        }

        // Анимация тревоги
        if (animator != null) animator?.SetTrigger("Alert");

        // Задержка
        yield return new WaitForSeconds(_detectionDelay);

        // Активация боевого режима
        playerDetected = true;

        _detectionDelayActive = false;
    }

    public void UpdateLastKnownPosition()
    {
        if (playerTransform != null)
        {
            lastKnownPlayerPosition = playerTransform.position;
            timeSinceLastSeen = 0f;
        }
    }

    public void ForgetPlayer()
    {
        isAlerted = false;
        playerDetected = false;
        timeSinceLastSeen = 0f;
        agent.speed = walkSpeed;
    }

    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, playerTransform.position);
    }

    void OnDrawGizmos()
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

                if (_showRaycast && playerTransform != null)
                {
                    DrawVisionRaycast(position);
                }

                if (_showLastKnownPosition && isAlerted)
                {
                    DrawLastKnownPosition();
                }

                // Направление взгляда
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(position, transform.forward * 2f);
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

        Vector3 directionToPlayer = (playerTransform.position - position).normalized;
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
                Gizmos.color = _raycastHitColor;
            else
                Gizmos.color = _raycastMissColor;

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
            Gizmos.color = inFOV ? _raycastHitColor : _raycastMissColor;
            Gizmos.DrawLine(position, playerTransform.position);
        }

        // Индикатор угла обзора
        if (!inFOV)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, 0.5f);
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
        // Текстовая информация (только в Scene View)
#if UNITY_EDITOR
        if (_showDebug && _showDebugInfo)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, GetDebugInfo());
#endif
    }

    string GetDebugInfo()
    {
        string info = $"Enemy AI Debug\n";
        info += $"Activated: {isActivated}\n";
        info += $"Alerted: {isAlerted}\n";
        info += $"Searching: {isSearching}\n";
        info += $"Can See Player: {CanSeePlayer()}\n";

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
        }

        return info;
    }
}