using Akila.FPSFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Battery : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 100f;
    [SerializeField] private GameObject _modelBattery;
    [SerializeField] private GameObject _shieldObject;
    [SerializeField] private float _lenPathShield = 4f;
    [SerializeField] private float _speedMoveShield = 3f;
    [SerializeField] private float _timeShieldOpen = 10f;
    [SerializeField] private float _cooldownTime = 5f;

    private bool _untargetable = false;

    private UnityEvent onDeath = new UnityEvent();
    private UnityEvent onEndCooldown = new UnityEvent();

    public bool isDamagableDisabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool allowDamageableEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float Health { get => _health; set => throw new System.NotImplementedException(); }
    public bool DeadConfirmed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameObject DamageSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public UnityEvent OnDeath => onDeath;
    public UnityEvent OnEndCooldown => onEndCooldown;

    public void Damage(float amount, GameObject damageSource)
    {
        if (!_untargetable)
            return;

        _health -= amount;

        if(_health <= 0)
        {
            Death();
        }
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
        Vector3 target = _shieldObject.transform.position - new Vector3(0, _lenPathShield, 0);

        _untargetable = true;

        while (_shieldObject.transform.position != target)
        {
            _shieldObject.transform.position = Vector3.MoveTowards(_shieldObject.transform.position, target, _speedMoveShield * Time.deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(_timeShieldOpen);

        target = _shieldObject.transform.position + new Vector3(0, _lenPathShield, 0);

        while (_shieldObject.transform.position != target)
        {
            _shieldObject.transform.position = Vector3.MoveTowards(_shieldObject.transform.position, target, _speedMoveShield * Time.deltaTime);

            yield return null;
        }

        _untargetable = false;

        yield return new WaitForSeconds(_cooldownTime);

        OnEndCooldown?.Invoke();
    }
}
