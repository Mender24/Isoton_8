using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyState))]
public class EnemyPerception : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyState _state;
    [SerializeField] private string _mainCameraTag = "MainCamera";
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Vision")]
    [SerializeField] private float _fieldOfViewAngle = 110f;
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionHeight = 0.5f;

    [Header("Hearing")]
    [SerializeField] private float _hearingRange = 10f;
    [SerializeField] private float _noiseInvestigationTime = 8f;

    [Header("Detection")]
    [SerializeField] private float _detectionDelay = 0.9f;
    [SerializeField] private float _forgetTime = 10f;
    [SerializeField] private float _minDistanceToForget = 4f;

    private Transform _playerTransform;
    private Camera _mainCamera;
    private float _noiseTimer;
    private bool _detectionPending;

    public float VisionRange  => _visionRange;
    public float VisionHeight => _visionHeight;
    public float FieldOfView  => _fieldOfViewAngle;
    public float HearingRange => _hearingRange;
    public float ForgetTime   => _forgetTime;

    private void Awake()
    {
        if (_state == null) _state = GetComponent<EnemyState>();
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;

        if (_playerTransform != null)
        {
            foreach (var cam in _playerTransform.GetComponentsInChildren<Camera>())
            {
                if (cam.CompareTag(_mainCameraTag))
                {
                    _mainCamera = cam;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (!_state.IsActivated || _state.IsDead) return;
        TickForget();
        TickNoise();
    }

    private void TickForget()
    {
        if (!_state.IsAlerted || CanSeePlayer()) return;

        float dist = GetDistanceToPlayer();
        if (dist < _minDistanceToForget) return;

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

    public bool CanSeePlayer()
    {
        if (_playerTransform == null || _mainCamera == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * _visionHeight;
        Vector3 dir = (_mainCamera.transform.position - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, _mainCamera.transform.position);

        if (dist > _visionRange) return false;
        if (Vector3.Angle(transform.forward, dir) > _fieldOfViewAngle * 0.5f) return false;

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, _visionRange, _obstacleLayer | _playerLayer))
            return hit.transform == _playerTransform;

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