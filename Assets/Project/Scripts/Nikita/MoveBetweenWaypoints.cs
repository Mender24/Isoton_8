using UnityEngine;

public class MoveBetweenWaypoints : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;

    [Header("Settings")]
    public bool isMoving = true;
    public float speed = 3f;
    [Range(0f, 0.5f)]
    public float pauseDuration = 0.5f;

    private float _t = 0f;
    private float _direction = 1f;
    private bool _isPaused = false;
    private float _pauseTimer = 0f;

    void Update()
    {
        if (!isMoving || _pointA == null || _pointB == null) return;

        if (_isPaused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f) _isPaused = false;
            return;
        }

        float distance = Vector3.Distance(_pointA.position, _pointB.position);
        _t += _direction * (speed / distance) * Time.deltaTime;
        _t = Mathf.Clamp01(_t);

        // ease in/out
        float easedT = _t * _t * (3f - 2f * _t);
        transform.position = Vector3.LerpUnclamped(_pointA.position, _pointB.position, easedT);

        if (_t >= 1f)
        {
            _direction = -1f;
            _isPaused = true;
            _pauseTimer = pauseDuration;
        }
        else if (_t <= 0f)
        {
            _direction = 1f;
            _isPaused = true;
            _pauseTimer = pauseDuration;
        }
    }
}
