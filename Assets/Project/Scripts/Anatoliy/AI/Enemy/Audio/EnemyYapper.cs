using UnityEngine;

public class EnemyYapper : MonoBehaviour
{
    [SerializeField] private EnemyAudioController _audio;
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 20f;

    private float _timer;

    private void Awake()
    {
        if (_audio == null)
            _audio = GetComponent<EnemyAudioController>();
        ResetTimer();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _audio.PlayRandomYap();
            ResetTimer();
        }
    }

    public void StartYapping()
    {
        ResetTimer();
        enabled = true;
    }

    public void StopYapping() => enabled = false;

    private void ResetTimer() => _timer = Random.Range(_minInterval, _maxInterval);
}
