using System;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public event Action<bool> OnAlertedChanged;
    public event Action<bool> OnPlayerDetectedChanged;
    public event Action<bool> OnIsDeadChanged;
    public event Action<bool> OnIsFiringChanged;
    public event Action<bool> OnIsReloadingChanged;
    public event Action<bool> OnIsMeleeAttackingChanged;

    [HideInInspector] public bool IsActivated;
    public float TimeBeforeDeactivate = 20f;

    private bool _isDead;
    [HideInInspector]
    public bool IsDead
    {
        get => _isDead;
        set { if (_isDead == value) return; _isDead = value; OnIsDeadChanged?.Invoke(value); }
    }

    private bool _isAlerted;
    [HideInInspector]
    public bool IsAlerted
    {
        get => _isAlerted;
        set { if (_isAlerted == value) return; _isAlerted = value; OnAlertedChanged?.Invoke(value); }
    }

    private bool _playerDetected;
    [HideInInspector]
    public bool PlayerDetected
    {
        get => _playerDetected;
        set { if (_playerDetected == value) return; _playerDetected = value; OnPlayerDetectedChanged?.Invoke(value); }
    }

    [HideInInspector] public bool IsAlertAnimationPlaying;

    [HideInInspector] public bool IsSearching;
    [HideInInspector] public float TimeSinceLastSeen;

    [HideInInspector] public Vector3 LastKnownPlayerPosition;
    [HideInInspector] public Vector3 StartPosition;

    [HideInInspector] public bool HeardNoise;
    [HideInInspector] public Vector3 LastHeardNoisePosition;

    private bool _isFiring;
    [HideInInspector]
    public bool IsFiring
    {
        get => _isFiring;
        set { if (_isFiring == value) return; _isFiring = value; OnIsFiringChanged?.Invoke(value); }
    }

    private bool _isReloading;
    [HideInInspector]
    public bool IsReloading
    {
        get => _isReloading;
        set { if (_isReloading == value) return; _isReloading = value; OnIsReloadingChanged?.Invoke(value); }
    }

    [HideInInspector] public int CurrentBullet;
    [HideInInspector] public float ShootCooldown;

    private bool _isMeleeAttacking;
    [HideInInspector]
    public bool IsMeleeAttacking
    {
        get => _isMeleeAttacking;
        set { if (_isMeleeAttacking == value) return; _isMeleeAttacking = value; OnIsMeleeAttackingChanged?.Invoke(value); }
    }

    [HideInInspector] public float MeleeAttackCooldown;

    public void ResetState(bool fireEvents = false)
    {
        IsActivated = false;
        IsAlertAnimationPlaying = false;

        if (fireEvents)
        {
            IsDead           = false;
            IsAlerted        = false;
            PlayerDetected   = false;
            IsFiring         = false;
            IsReloading      = false;
            IsMeleeAttacking = false;
        }
        else
        {
            _isDead           = false;
            _isAlerted        = false;
            _playerDetected   = false;
            _isFiring         = false;
            _isReloading      = false;
            _isMeleeAttacking = false;
        }

        IsSearching       = false;
        TimeSinceLastSeen = 0f;

        LastKnownPlayerPosition = Vector3.zero;
        StartPosition           = Vector3.zero;

        HeardNoise             = false;
        LastHeardNoisePosition = Vector3.zero;

        CurrentBullet       = 0;
        ShootCooldown       = 0f;
        MeleeAttackCooldown = 0f;
    }
}