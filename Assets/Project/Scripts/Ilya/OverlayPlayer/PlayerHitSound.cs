using System.Collections.Generic;
using Akila.FPSFramework;
using UnityEngine;

public class PlayerHitSound : MonoBehaviour
{
    [SerializeField] private List<CellAudioClip> _hitClips;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _cooldown = 0.15f;

    private Damageable _damageable;
    private float _lastPlayTime = -999f;

    private void Awake()
    {
        _damageable = GetComponentInParent<Damageable>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (_damageable == null)
        {
            Debug.LogWarning("PlayerHitSound: Damageable not found.", this);
            return;
        }

        _damageable.DamageApplied += OnDamageApplied;
    }

    private void OnDestroy()
    {
        if (_damageable != null)
            _damageable.DamageApplied -= OnDamageApplied;
    }

    private void OnDamageApplied(GameObject damageSource)
    {
        if (_hitClips == null || _hitClips.Count == 0) return;
        if (_audioSource == null) return;
        if (Time.time - _lastPlayTime < _cooldown) return;

        _lastPlayTime = Time.time;
        _hitClips[Random.Range(0, _hitClips.Count)].PlayAudioClipOneShot(_audioSource);
    }
}
