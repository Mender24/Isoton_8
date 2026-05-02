using Akila.FPSFramework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Battery : MonoBehaviour, IDamageable
{
    [SerializeField] private bool _isOpenShieldStart = false;
    [Space]
    [SerializeField] private float _health = 100f;
    [SerializeField] private GameObject _modelBattery;
    [SerializeField] private GameObject _shieldObject;
    [SerializeField] private float _lenPathShield = 4f;
    [SerializeField] private float _speedOpenShield = 3f;
    [SerializeField] private float _speedCloseShield = 3f;
    [SerializeField] private float _timeShieldOpen = 10f;
    [SerializeField] private float _cooldownTime = 5f;
    [SerializeField] private Explosive _explosive;


    [SerializeField] private AnimationClip _animationClip;

    private bool _untargetable = false;
    private bool _isDead = false;

    private UnityEvent onDeath = new UnityEvent();
    private UnityEvent onEndCooldown = new UnityEvent();

    public bool isDamagableDisabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool allowDamageableEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float Health { get => _health; set => throw new System.NotImplementedException(); }
    public bool DeadConfirmed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameObject DamageSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool IsDead => _isDead;

    public UnityEvent EndInteraction;
    public UnityEvent OnDeath => onDeath;
    public UnityEvent OnEndCooldown => onEndCooldown;
    public event Action OnStartUpShield;
    public event Action OnStartDownShield;

    private void Start()
    {
        if(_explosive != null)
        {
            _explosive.enabled = false;
        }

        if(_isOpenShieldStart)
        {
            OpenShield();
        }
    }

    public void Damage(float amount, GameObject damageSource)
    {
        if (!_untargetable)
            return;

        _health--;

        _untargetable = false;

        if (_health <= 0)
        {
            _isDead = true;

            if(_explosive != null)
            {
                _explosive.enabled = true;
                _explosive.Explode();
            }

            Death();
            EndInteraction?.Invoke();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(CloseShieldObject());
        }
    }

    public void Register()
    {

    }

    public bool IsSphereCollision(Vector3 sphereCenter, float sphereRadius)
    {
        return false;
    }

    public void OpenShield()
    {
        StartCoroutine(OpenShieldInteraction());
    }

    public bool IsSwaped()
    {
        throw new System.NotImplementedException();
    }

    private void Death()
    {
        StopAllCoroutines();
        _health = 0;
        OnDeath?.Invoke();
        _modelBattery.SetActive(false);
    }

    private IEnumerator OpenShieldInteraction()
    {
        OnStartUpShield?.Invoke();

        yield return StartCoroutine(OpenShieldObject());

        yield return new WaitForSeconds(_timeShieldOpen);

        OnStartDownShield?.Invoke();

        yield return StartCoroutine(CloseShieldObject());
    }

    private IEnumerator OpenShieldObject()
    {
        _untargetable = true;

        if (_shieldObject != null)
        {
            Vector3 target = _shieldObject.transform.position - new Vector3(0, _lenPathShield, 0);

            while (Mathf.Abs((_shieldObject.transform.position - target).magnitude) >= 0.1f)
            {
                _shieldObject.transform.position = Vector3.MoveTowards(_shieldObject.transform.position, target, _speedOpenShield * Time.deltaTime);

                yield return null;
            }
        }
    }

    private IEnumerator CloseShieldObject()
    {
        if(_shieldObject != null)
        {
            Vector3 target = _shieldObject.transform.position + new Vector3(0, _lenPathShield, 0);

            while (Mathf.Abs((_shieldObject.transform.position - target).magnitude) >= 0.1f)
            {
                _shieldObject.transform.position = Vector3.MoveTowards(_shieldObject.transform.position, target, _speedCloseShield * Time.deltaTime);

                yield return null;
            }
        }

        _untargetable = false;

        yield return new WaitForSeconds(_cooldownTime);

        OnEndCooldown?.Invoke();
    }
}
