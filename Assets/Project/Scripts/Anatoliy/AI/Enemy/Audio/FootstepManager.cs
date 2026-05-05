using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public static FootstepManager Instance { get; private set; }

    [SerializeField] private int _maxActiveBots = 3;
    [SerializeField] private float _updateInterval = 0.2f;

    private readonly List<Transform> _bots = new();
    private readonly HashSet<Transform> _allowedBots = new();
    private float _timer;
    private Transform _listener;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = _updateInterval;

        if (_listener == null)
        {
            var al = FindFirstObjectByType<AudioListener>();
            if (al != null) _listener = al.transform;
        }
        if (_listener == null) return;

        _bots.RemoveAll(b => b == null);

        Vector3 listenerPos = _listener.position;
        _bots.Sort((a, b) =>
            Vector3.SqrMagnitude(a.position - listenerPos)
                .CompareTo(Vector3.SqrMagnitude(b.position - listenerPos)));

        _allowedBots.Clear();
        int limit = Mathf.Min(_maxActiveBots, _bots.Count);
        for (int i = 0; i < limit; i++)
            _allowedBots.Add(_bots[i]);
    }

    public void Register(Transform bot)
    {
        if (!_bots.Contains(bot)) _bots.Add(bot);
    }

    public void Unregister(Transform bot)
    {
        _bots.Remove(bot);
        _allowedBots.Remove(bot);
    }

    public bool CanPlayFootstep(Transform bot) => _allowedBots.Contains(bot);
}
