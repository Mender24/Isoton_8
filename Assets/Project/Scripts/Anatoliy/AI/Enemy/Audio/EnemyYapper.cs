using Unity.VisualScripting;
using UnityEngine;

public class EnemyYapper : MonoBehaviour
{
    [SerializeField] private EnemyAudioController _audio;
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 20f;

    private EnemyState _state;
    private float _timer;
    private bool _start = false;

    private void Awake()
    {
        if (_audio == null)
            _audio = GetComponent<EnemyAudioController>();
    }

    private void Start()
    {
        var enemyBase = GetComponent<EnemyBase>();
        if (enemyBase != null)
            _state = enemyBase.State;

        if (_state != null)
            _state.OnAlertedChanged += OnAlertedChanged;

        _start = false;
    }

    private void OnDestroy()
    {
        if (_state != null)
            _state.OnAlertedChanged -= OnAlertedChanged;
    }

    private void Update()
    {
        if (_start)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _audio.PlayRandomYap();
                ResetTimer();
            }
        }

    }

    private void OnAlertedChanged(bool isAlerted)
    {
        if (isAlerted)
        {
            ResetTimer();
            _start = true;
        }
        else
        {
            _start = false;
        }
    }

    private void ResetTimer() => _timer = Random.Range(_minInterval, _maxInterval);
}
