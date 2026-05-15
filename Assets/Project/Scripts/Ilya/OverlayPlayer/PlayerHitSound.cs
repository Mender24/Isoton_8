using System.Collections.Generic;
using Akila.FPSFramework;
using UnityEngine;

public class PlayerHitSound : MonoBehaviour
{
    [Header("Hit Impact")]
    [SerializeField] private List<CellAudioClip> _hitClips;
    [SerializeField] private AudioSource _hitAudioSource;
    [SerializeField] private float _hitCooldown = 0.1f;

    [Header("Pain Grunt")]
    [SerializeField] private List<CellAudioClip> _painClips;
    [SerializeField] private AudioSource _painAudioSource;
    [SerializeField] private float _painCooldown = 0.8f;

    private Damageable _damageable;
    private float _lastHitTime = -999f;
    private float _lastPainTime = -999f;

    private void Awake()
    {
        _damageable = GetComponentInParent<Damageable>();
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
        TryPlayHit();
        TryPlayPain();
    }

    private void TryPlayHit()
    {
        if (_hitClips == null || _hitClips.Count == 0) return;
        if (_hitAudioSource == null) return;
        if (Time.time - _lastHitTime < _hitCooldown) return;

        _lastHitTime = Time.time;
        _hitClips[Random.Range(0, _hitClips.Count)].PlayAudioClipOneShot(_hitAudioSource);
    }

    private void TryPlayPain()
    {
        if (_painClips == null || _painClips.Count == 0) return;
        if (_painAudioSource == null) return;
        if (Time.time - _lastPainTime < _painCooldown) return;

        _lastPainTime = Time.time;
        _painClips[Random.Range(0, _painClips.Count)].PlayAudioClipOneShot(_painAudioSource);
    }
}
