using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Reactor : MonoBehaviour, IDamageable
{
    [SerializeField] private float _healthReactor = 30f;
    [SerializeField] private float _secondDestroy = 1f;
    [SerializeField] private float _lenPathShield = 4f;
    [SerializeField] private float _speedMoveShield = 3f;
    [SerializeField] private float _cooldownNextBattery = 3f;
    [SerializeField] private List<Battery> _batterys = new();

    [SerializeField] private GameObject _shieldObject;

    private int _batteryHealth;
    private float _health;

    private int _currentIndexLiveBattery = 0;

    public bool isDamagableDisabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool allowDamageableEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float Health { get => _health; set => throw new System.NotImplementedException(); }
    public bool DeadConfirmed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameObject DamageSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    UnityEvent IDamageable.OnDeath => throw new System.NotImplementedException();

    private void OnDisable()
    {
        for (int i = 0; i < _batterys.Count; i++)
        {
            _batterys[i].OnDeath.RemoveListener(OnDeath);
            _batterys[i].OnEndCooldown.RemoveListener(StartReactor);
        }
    }

    private void Start()
    {
        _health = _healthReactor;
        _batteryHealth = _batterys.Count;

        for (int i = 0; i < _batterys.Count; i++)
        {
            _batterys[i].OnDeath.AddListener(OnDeath);
            _batterys[i].OnEndCooldown.AddListener(StartReactor);
        }

        StartReactor();
    }

    public void Damage(float amount, GameObject damageSource)
    {
        _health -= amount;

        if (_health <= 0)
        {
            StartCoroutine(Death());
        }
    }

    public void OnDeath()
    {
        _batteryHealth -= 1;

        if (_batteryHealth <= 0)
        {
            StartCoroutine(OpenShield());
            return;
        }

        _currentIndexLiveBattery++;
        StartCoroutine(CooldownNextBattery());
    }

    public bool IsSwaped()
    {
        throw new System.NotImplementedException();
    }

    public void StartReactor()
    {
        _batterys[_currentIndexLiveBattery].OpenShield();
    }

    private IEnumerator CooldownNextBattery()
    {
        yield return new WaitForSeconds(_cooldownNextBattery);

        StartReactor();
    }

    private IEnumerator OpenShield()
    {
        Vector3 target = _shieldObject.transform.position - new Vector3(0, _lenPathShield, 0);

        while (_shieldObject.transform.position != target)
        {
            _shieldObject.transform.position = Vector3.MoveTowards(_shieldObject.transform.position, target, _speedMoveShield * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(_secondDestroy);

        gameObject.SetActive(false);
    }
}
