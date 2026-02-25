using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Дебаг-компонент для врагов новой архитектуры.
/// Вешается на тот же GameObject что и EnemyBase/RangedEnemy/MeleeEnemy.
/// Полностью заменяет debug-код из старого EnemyAI.
/// 
/// Добавлено по сравнению с оригиналом:
/// - Дебаг зоны melee-атаки (хитбокс + радиус достижения)
/// - Дебаг выстрела (позиция последнего выстрела + попал/промахнулся)
/// - Дебаг навигации (путь NavMesh агента + конечная точка)
/// - Дебаг патрулирования (waypoint-ы)
/// </summary>
public class EnemyDebugger : MonoBehaviour
{
    [Header("Master")]
    [SerializeField] private bool _showDebug = true;
    [SerializeField] private bool _showDebugInfo = true;   // текстовый оверлей над врагом

    [Header("Vision")]
    [SerializeField] private bool _debugVision = true;
    [SerializeField] private bool _showDetectionRange = true;
    [SerializeField] private bool _showFieldOfView    = true;
    [SerializeField] private bool _showVisionRaycast  = true;
    [SerializeField] private bool _showLastKnownPosition = true;
    [SerializeField] private int  _fovResolution = 10;

    [SerializeField] private Color _detectionRangeColor = new Color(1f, 1f, 0f, 0.08f);
    [SerializeField] private Color _alertColor          = Color.red;
    [SerializeField] private Color _fieldOfViewColor    = new Color(0f, 1f, 0f, 0.15f);
    [SerializeField] private Color _raycastHitColor     = Color.green;
    [SerializeField] private Color _raycastMissColor    = Color.red;

    [Header("Hearing")]
    [SerializeField] private bool  _debugHearing    = true;
    [SerializeField] private bool  _showHearingRange = true;
    [SerializeField] private Color _hearingRangeColor = new Color(0f, 0.5f, 1f, 0.08f);
    [SerializeField] private bool  _showNoiseTarget  = true;
    [SerializeField] private Color _noiseTargetColor = new Color(1f, 0.5f, 0f, 1f);

    [Header("Melee Combat")]
    [SerializeField] private bool  _debugMelee      = true;
    [SerializeField] private bool  _showMeleeRange  = true;   // радиус достижения
    [SerializeField] private bool  _showMeleeHitbox = true;   // сфера хитбокса удара
    [SerializeField] private Color _meleeRangeColor  = new Color(1f, 0f, 0f, 0.15f);
    [SerializeField] private Color _meleeHitboxColor = Color.red;

    [Header("Ranged Combat")]
    [SerializeField] private bool  _debugShooting       = true;
    [SerializeField] private bool  _showShotRaycast     = true;
    [SerializeField] private bool  _showAttackRange     = true;
    [SerializeField] private Color _shotHitColor        = Color.blue;
    [SerializeField] private Color _shotMissColor       = Color.cyan;
    [SerializeField] private Color _attackRangeColor    = new Color(1f, 0f, 0f, 0.05f);

    [Header("Navigation")]
    [SerializeField] private bool  _debugNavigation  = true;
    [SerializeField] private bool  _showNavPath      = true;
    [SerializeField] private bool  _showDestination  = true;
    [SerializeField] private bool  _showPatrolPoints = true;
    [SerializeField] private Color _navPathColor     = Color.yellow;
    [SerializeField] private Color _destinationColor = Color.magenta;
    [SerializeField] private Color _patrolColor      = new Color(0.3f, 1f, 0.3f, 1f);

    private bool    _lastShotHit;
    private Vector3 _lastShotOrigin;
    private Vector3 _lastShotTarget;
    private bool    _hasShotData;

    private EnemyBase        _enemy;
    private EnemyState       _state;
    private EnemyPerception  _perception;
    private EnemyNavigation  _navigation;
    private MeleeCombatModule _meleeCombat;
    private RangedCombatModule _rangedCombat;

    private void Awake()
    {
        _enemy        = GetComponent<EnemyBase>();
        _state        = GetComponent<EnemyState>();
        _perception   = GetComponent<EnemyPerception>();
        _navigation   = GetComponent<EnemyNavigation>();
        _meleeCombat  = GetComponent<MeleeCombatModule>();
        _rangedCombat = GetComponent<RangedCombatModule>();
    }

    public void SetLastShot(Vector3 origin, Vector3 target, bool hit)
    {
        _lastShotOrigin = origin;
        _lastShotTarget = target;
        _lastShotHit    = hit;
        _hasShotData    = true;
    }

    private void OnDrawGizmos()
    {
        if (!_showDebug) return;

        if (_state      == null) _state      = GetComponent<EnemyState>();
        if (_perception == null) _perception = GetComponent<EnemyPerception>();
        if (_navigation == null) _navigation = GetComponent<EnemyNavigation>();
        if (_meleeCombat  == null) _meleeCombat  = GetComponent<MeleeCombatModule>();
        if (_rangedCombat == null) _rangedCombat = GetComponent<RangedCombatModule>();

        float visionHeight = _perception != null ? GetVisionHeight() : 0.5f;
        Vector3 eyePos = transform.position + Vector3.up * visionHeight;

        if (_debugHearing)   DrawHearing(eyePos);
        if (_debugVision)    DrawVision(eyePos);
        if (_debugMelee)     DrawMelee();
        if (_debugShooting)  DrawShooting(eyePos);
        if (_debugNavigation) DrawNavigation();
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (!_showDebug || !_showDebugInfo) return;

        if (_state      == null) _state      = GetComponent<EnemyState>();
        if (_perception == null) _perception = GetComponent<EnemyPerception>();

        Handles.Label(transform.position + Vector3.up * 3f, BuildDebugText(),
            new GUIStyle { normal = { textColor = Color.white }, fontSize = 11 });
#endif
    }

    private void DrawHearing(Vector3 eyePos)
    {
        float range = _perception != null ? _perception.HearingRange : 10f;

        if (_showHearingRange)
        {
            Gizmos.color = _hearingRangeColor;
            Gizmos.DrawSphere(eyePos, range);

            Gizmos.color = new Color(_hearingRangeColor.r, _hearingRangeColor.g, _hearingRangeColor.b, 0.6f);
            DrawCircle(eyePos, range, 48);
        }

        if (_showNoiseTarget && _state != null && _state.HeardNoise)
        {
            Gizmos.color = _noiseTargetColor;
            Gizmos.DrawWireSphere(_state.LastHeardNoisePosition, 0.4f);
            Gizmos.DrawLine(eyePos, _state.LastHeardNoisePosition);

#if UNITY_EDITOR
            float pulse = (Mathf.Sin((float)EditorApplication.timeSinceStartup * 4f) * 0.5f + 0.5f) * 0.3f + 0.1f;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(_state.LastHeardNoisePosition, pulse);
#endif
        }
    }

    private void DrawVision(Vector3 eyePos)
    {
        if (_perception == null) return;

        float fov   = _perception.FieldOfView;
        float range = _perception.VisionRange;
        bool alerted   = _state != null && _state.IsAlerted;
        bool seesPlayer = Application.isPlaying && _perception.CanSeePlayer();

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyePos, transform.forward * 2f);

        if (_showDetectionRange)
        {
            Gizmos.color = _detectionRangeColor;
            Gizmos.DrawSphere(eyePos, range);

            Gizmos.color = alerted ? _alertColor : Color.yellow;
            DrawCircle(eyePos, range, 50);
        }

        if (_showFieldOfView)
            DrawFieldOfView(eyePos, fov, range, alerted, seesPlayer);

        if (_showVisionRaycast)
            DrawVisionRaycast(eyePos, fov);

        if (_showLastKnownPosition && _state != null && _state.IsAlerted &&
            _state.LastKnownPlayerPosition != Vector3.zero)
            DrawLastKnownPosition();
    }

    private void DrawFieldOfView(Vector3 pos, float fov, float range, bool alerted, bool seesPlayer)
    {
        Color color = _fieldOfViewColor;
        if (alerted)     color = new Color(1f, 0f, 0f, 0.2f);
        else if (seesPlayer) color = new Color(1f, 0.5f, 0f, 0.25f);

        float halfFov = fov * 0.5f;
        Vector3 left  = Quaternion.Euler(0, -halfFov, 0) * transform.forward * range;
        Vector3 right = Quaternion.Euler(0,  halfFov, 0) * transform.forward * range;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos, pos + left);
        Gizmos.DrawLine(pos, pos + right);

        Vector3 prev = pos + left;
        for (int i = 0; i <= _fovResolution; i++)
        {
            float angle = -halfFov + fov * i / _fovResolution;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 point = pos + dir * range;

            Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
            Gizmos.DrawLine(pos, point);

            Gizmos.color = Color.cyan;
            if (i > 0) Gizmos.DrawLine(prev, point);

            prev = point;
        }
    }

    private void DrawVisionRaycast(Vector3 eyePos, float fov)
    {
        if (_enemy == null || _enemy.PlayerTransform == null) return;

        Transform targetTransform = _enemy.PlayerTransform;
        Vector3 targetPos = targetTransform.position;

#if UNITY_EDITOR
        if (Camera.current != null)
            targetPos = Camera.current.transform.position;
#endif

        Vector3 dir  = (targetPos - eyePos).normalized;
        float   dist = Vector3.Distance(eyePos, _enemy.PlayerTransform.position);
        float   angle = Vector3.Angle(transform.forward, dir);
        bool    inFov = angle <= fov * 0.5f;

        EnemyPerception p = _perception;

        RaycastHit hit;

        if (Physics.Raycast(eyePos, dir, out hit, dist))
        {
            bool hitPlayer = hit.transform == _enemy.PlayerTransform;
            Gizmos.color = (hitPlayer && inFov) ? _raycastHitColor : _raycastMissColor;
            Gizmos.DrawLine(eyePos, hit.point);
            // Gizmos.DrawSphere(hit.point, 0.1f);

            if (!hitPlayer)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.12f);
            }
        }
        else
        {
            Gizmos.color = inFov ? _raycastHitColor : _raycastMissColor;
            Gizmos.DrawLine(eyePos, _enemy.PlayerTransform.position);
        }

        if (!inFov)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_enemy.PlayerTransform.position, 0.5f);
        }
    }

    private void DrawLastKnownPosition()
    {
        Vector3 pos = _state.LastKnownPlayerPosition;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, 0.5f);
        Gizmos.DrawLine(pos, pos + Vector3.up * 2f);

        Gizmos.DrawLine(pos + Vector3.left    * 0.5f, pos + Vector3.right   * 0.5f);
        Gizmos.DrawLine(pos + Vector3.forward * 0.5f, pos + Vector3.back    * 0.5f);

        if (_perception != null && _perception.ForgetTime > 0f)
        {
            float progress = _state.TimeSinceLastSeen / _perception.ForgetTime;
            Gizmos.color = Color.Lerp(Color.red, Color.yellow, progress);
            Gizmos.DrawWireSphere(pos + Vector3.up * 2f, 0.3f * (1f - progress));
        }
    }

    private void DrawMelee()
    {
        if (_meleeCombat == null) return;

        // Радиус достижения (дистанция при которой начинается Chase → Attack)
        if (_showMeleeRange)
        {
            Gizmos.color = _meleeRangeColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, _meleeCombat.AttackRange);
        }

        // Хитбокс удара — нужен доступ к конфигу
        // Рисуем только если атака активна
        if (_showMeleeHitbox && _state != null && _state.IsMeleeAttacking)
        {
            // Примерное положение хитбокса (вперёд + вверх)
            Vector3 hitPos = transform.position + transform.forward * 1f + Vector3.up * 1f;
            Gizmos.color = _meleeHitboxColor;
            Gizmos.DrawWireSphere(hitPos, 1f);
        }
    }

    private void DrawShooting(Vector3 eyePos)
    {
        if (_rangedCombat == null) return;

        // Радиус атаки
        if (_showAttackRange)
        {
            Gizmos.color = _attackRangeColor;
            Gizmos.DrawWireSphere(transform.position, _rangedCombat.AttackRange);
            Gizmos.color = new Color(_attackRangeColor.r, _attackRangeColor.g, _attackRangeColor.b, 0.4f);
            DrawCircle(transform.position + Vector3.up * 0.1f, _rangedCombat.AttackRange, 48);
        }

        // Последний выстрел
        if (_showShotRaycast && _hasShotData)
        {
            Gizmos.color = _lastShotHit ? _shotHitColor : _shotMissColor;
            Gizmos.DrawLine(_lastShotOrigin, _lastShotTarget);
            Gizmos.DrawWireSphere(_lastShotTarget, 0.15f);
        }
    }

    private void DrawNavigation()
    {
        if (_navigation == null || _navigation.Agent == null) return;

        var agent = _navigation.Agent;
        if (!agent.isOnNavMesh) return;

        // Путь
        if (_showNavPath && agent.hasPath)
        {
            var corners = agent.path.corners;
            Gizmos.color = _navPathColor;
            for (int i = 0; i < corners.Length - 1; i++)
                Gizmos.DrawLine(corners[i], corners[i + 1]);

            // Точки пути
            foreach (var c in corners)
                Gizmos.DrawWireSphere(c, 0.1f);
        }

        // Конечная точка
        if (_showDestination && agent.hasPath)
        {
            Gizmos.color = _destinationColor;
            Vector3 dest = agent.pathEndPosition;
            Gizmos.DrawWireSphere(dest, 0.25f);
            Gizmos.DrawLine(dest + Vector3.up * 0.5f, dest + Vector3.up * 1.5f);
        }

        // Patrol waypoints
        if (_showPatrolPoints && _enemy != null && _enemy.PatrolPoints != null)
        {
            Gizmos.color = _patrolColor;
            for (int i = 0; i < _enemy.PatrolPoints.Count; i++)
            {
                if (_enemy.PatrolPoints[i] == null) continue;
                Vector3 wp = _enemy.PatrolPoints[i].transform.position;

                Gizmos.DrawWireSphere(wp, 0.3f);

                // Стрелка к следующей точке
                int next = (i + 1) % _enemy.PatrolPoints.Count;
                if (_enemy.PatrolPoints[next] != null)
                    DrawArrow(wp, _enemy.PatrolPoints[next].transform.position);
            }
        }
    }

    private string BuildDebugText()
    {
        if (_state == null) return "No EnemyState";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Enemy Debug</b>");
        sb.AppendLine($"Activated:  {_state.IsActivated}");
        sb.AppendLine($"Alerted:    {_state.IsAlerted}");
        sb.AppendLine($"Searching:  {_state.IsSearching}");
        sb.AppendLine($"Detected:   {_state.PlayerDetected}");
        sb.AppendLine($"Dead:       {_state.IsDead}");

        if (_enemy != null && _enemy.PlayerTransform != null)
        {
            float dist  = Vector3.Distance(transform.position, _enemy.PlayerTransform.position);
            Vector3 dir = (_enemy.PlayerTransform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            sb.AppendLine($"Dist/Angle: {dist:F1}m  {angle:F1}°");
        }

        if (_state.IsAlerted)
        {
            sb.AppendLine($"Forget:  {_state.TimeSinceLastSeen:F1}s / {(_perception != null ? _perception.ForgetTime : 0):F1}s");
        }

        if (_rangedCombat != null)
        {
            sb.AppendLine($"Firing:    {_rangedCombat.IsFiring}");
            sb.AppendLine($"Reloading: {_rangedCombat.IsReloading}");
            if (_state != null)
                sb.AppendLine($"Bullets:   {_state.CurrentBullet}");
        }

        if (_meleeCombat != null)
        {
            sb.AppendLine($"Attacking: {_meleeCombat.IsAttacking}");
            sb.AppendLine($"Atk cd:    {_state.MeleeAttackCooldown:F1}s");
            sb.AppendLine($"In Range:  {_meleeCombat.IsInRange()}");
        }

        if (_state.HeardNoise)
            sb.AppendLine($"HEARD NOISE");

        if (_navigation != null)
            sb.AppendLine($"Speed:  {_navigation.CurrentSpeed:F1} m/s");

        return sb.ToString();
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float step = 360f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float rad = step * i * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Gizmos.DrawLine(from, to);

        Vector3 dir = (to - from).normalized;
        Vector3 right = Quaternion.Euler(0, 30f, 0) * (-dir) * 0.4f;
        Vector3 left  = Quaternion.Euler(0, -30f, 0) * (-dir) * 0.4f;

        Gizmos.DrawLine(to, to + right);
        Gizmos.DrawLine(to, to + left);
    }

    private float GetVisionHeight()
    {
        return _perception != null ? _perception.VisionHeight : 0.5f;
    }
}