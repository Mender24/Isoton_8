using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyState))]
public class EnemyPerception : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyState _state;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Vision")]
    [SerializeField] private float _fieldOfViewAngle = 110f;
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionHeight = 0.5f;

    [Header("Vision Meter")]
    [SerializeField] private bool  _useVisionMeter       = false;
    [SerializeField] private float _visionMeterSpeedFar  = 1.5f;
    [SerializeField] private float _visionMeterSpeedNear = 3.0f;
    [SerializeField] private float _visionMeterDecay     = 2.0f;

    [Header("Multi-Ray Detection")]
    [SerializeField] private bool     _useMultiRay = false;
    [SerializeField] private Vector3[] _bodyCheckOffsets = new Vector3[]
    {
        new Vector3(0, 0.1f, 0),
        new Vector3(0, 1.0f, 0),
        new Vector3(0, 1.8f, 0),
    };

    [Header("Hearing")]
    [SerializeField] private float _hearingRange = 10f;
    [SerializeField] private float _noiseInvestigationTime = 8f;

    [Header("Detection")]
    [SerializeField] private float _detectionDelay = 0.9f;
    [SerializeField] private float _forgetTime = 10f;
    [SerializeField] private float _minDistanceToForget = 4f;

    private Transform _playerTransform;
    private float _noiseTimer;
    private bool _detectionPending;
    private float _visionMeter;

    public float VisionRange    => _visionRange;
    public float VisionHeight   => _visionHeight;
    public float FieldOfView    => _fieldOfViewAngle;
    public float HearingRange   => _hearingRange;
    public float ForgetTime     => _forgetTime;
    public float VisionMeter    => _visionMeter;
    public bool  UseMultiRay    => _useMultiRay;
    public Vector3[] BodyCheckOffsets => _bodyCheckOffsets;

    public void MultiplyForgetTime(float multiplier) => _forgetTime *= multiplier;

    private void Awake()
    {
        if (_state == null) _state = GetComponent<EnemyState>();
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    private void Update()
    {
        if (!_state.IsActivated || _state.IsDead) return;
        _state.PlayerIsSeen = CanSeePlayer();
        TickForget();
        TickNoise();
        TickVisionMeter();
    }

    private void TickForget()
    {
        if (!_state.IsAlerted || IsPlayerInSightRaw()) return;

        _state.TimeSinceLastSeen += Time.deltaTime;

        if (_state.TimeSinceLastSeen >= _forgetTime)
            ForgetPlayer();
    }

    private void TickNoise()
    {
        if (!_state.HeardNoise) return;

        _noiseTimer -= Time.deltaTime;

        if (_noiseTimer <= 0 || _state.PlayerDetected)
            _state.HeardNoise = false;
    }

    private void TickVisionMeter()
    {
        if (!_useVisionMeter) return;

        bool inSight = IsPlayerInSightRaw();
        if (inSight)
        {
            float half = _visionRange / 2f;
            float speed = GetDistanceToPlayer() > half ? _visionMeterSpeedFar : _visionMeterSpeedNear;
            _visionMeter = Mathf.Clamp01(_visionMeter + Time.deltaTime * speed);
        }
        else
        {
            _visionMeter = Mathf.Clamp01(_visionMeter - Time.deltaTime * _visionMeterDecay);
        }

        _state.VisionMeterValue = _visionMeter;
    }

    public bool CanSeePlayer()
    {
        if (_useVisionMeter)
            return _visionMeter >= 1f;

        return IsPlayerInSightRaw();
    }

    private bool IsPlayerInSightRaw()
    {
        if (_playerTransform == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * _visionHeight;
        Vector3 playerCenter = _playerTransform.position + Vector3.up * 1.0f;
        float dist = Vector3.Distance(eyePos, _playerTransform.position);

        if (dist > _visionRange) return false;

        if (dist >= _minDistanceToForget)
        {
            Vector3 dir = (playerCenter - eyePos).normalized;
            if (Vector3.Angle(transform.forward, dir) > _fieldOfViewAngle * 0.5f) return false;
        }

        return _useMultiRay ? MultiRayCheck(eyePos) : SingleRayCheck(eyePos, playerCenter);
    }

    private bool SingleRayCheck(Vector3 eyePos, Vector3 targetPoint)
    {
        Vector3 dir = (targetPoint - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetPoint) + 1f;

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist, _obstacleLayer | _playerLayer))
            return hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform);

        return false;
    }

    private bool MultiRayCheck(Vector3 eyePos)
    {
        foreach (var offset in _bodyCheckOffsets)
        {
            Vector3 target = _playerTransform.position + offset;
            Vector3 dir = (target - eyePos).normalized;
            float dist = Vector3.Distance(eyePos, target) + 1f;

            if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist, _obstacleLayer | _playerLayer))
            {
                if (hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform))
                    return true;
            }
        }
        return false;
    }

    public void UpdateLastKnownPosition()
    {
        if (_playerTransform == null) return;
        _state.TimeSinceLastSeen = 0f;
        StartCoroutine(DelayedPositionUpdate());
    }

    private IEnumerator DelayedPositionUpdate()
    {
        yield return new WaitForSeconds(1f);
        if (_playerTransform != null)
            _state.LastKnownPlayerPosition = _playerTransform.position;
    }

    public bool TryHearNoise()
    {
        if (_state.PlayerDetected || _playerTransform == null) return false;

        if (Vector3.Distance(transform.position, _playerTransform.position) > _hearingRange) return false;

        foreach (var src in _playerTransform.GetComponentsInChildren<AudioSource>())
        {
            if (src != null && src.isPlaying)
            {
                _state.LastHeardNoisePosition = src.transform.position;
                _state.HeardNoise = true;
                _noiseTimer = _noiseInvestigationTime;
                return true;
            }
        }

        return false;
    }

    public Vector3 GetNoiseTarget() => _state.LastHeardNoisePosition;

    public void StartDetection(System.Action onDetectionComplete)
    {
        if (_detectionPending) return;
        StartCoroutine(DetectionDelay(onDetectionComplete));
    }

    private IEnumerator DetectionDelay(System.Action onDetectionComplete)
    {
        _detectionPending = true;
        yield return new WaitForSeconds(_detectionDelay);
        _state.PlayerDetected = true;
        _detectionPending = false;
        onDetectionComplete?.Invoke();
    }

    public void ForgetPlayer()
    {
        _state.IsAlerted = false;
        _state.PlayerDetected = false;
        _state.TimeSinceLastSeen = 0f;
    }

    public void InvestigateDamageSource(GameObject damageSource)
    {
        if (!_state.IsActivated) _state.IsActivated = true;

        Vector3 suspiciousPos = damageSource != null
            ? damageSource.transform.position
            : transform.position;

        _state.IsAlerted = true;
        _state.LastKnownPlayerPosition = suspiciousPos;
        _state.HeardNoise = true;
        _state.LastHeardNoisePosition = suspiciousPos;
        _noiseTimer = _noiseInvestigationTime;
    }

    public float GetDistanceToPlayer()
    {
        if (_playerTransform == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, _playerTransform.position);
    }
}
