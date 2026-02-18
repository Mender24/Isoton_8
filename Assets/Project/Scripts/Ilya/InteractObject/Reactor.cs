using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Reactor : MonoBehaviour, IDamageable
{
    [SerializeField] private float _healthReactor = 30f;
    [SerializeField] private float _timeBeforeOpenShield = 1f;
    [SerializeField] private float _secondDestroy = 1f;
    [SerializeField] private float _lenPathShield = 4f;
    [SerializeField] private float _lenPathDoorEnd = 4f;
    [SerializeField] private float _speedMoveShield = 3f;
    [SerializeField] private float _cooldownNextBattery = 3f;
    [SerializeField] private List<Battery> _batterys = new();

    [SerializeField] private GameObject _shieldObject;
    [SerializeField] private GameObject _reactorObject;
    [SerializeField] private GameObject _doorEndLocation;

    private int _batteryHealth;
    private float _health;

    private DoorControllerSceneChanger _doorControllerSceneChanger;

    private int _currentIndexLiveBattery = 0;

    public bool isDamagableDisabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool allowDamageableEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float Health { get => _health; set => throw new System.NotImplementedException(); }
    public bool DeadConfirmed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameObject DamageSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private UnityEvent onDeath = new UnityEvent();

    public TextMeshPro showingText;
    public UnityEvent OnDeath => onDeath;

    public void Register()
    {

    }

    private void OnDisable()
    {
        for (int i = 0; i < _batterys.Count; i++)
        {
            _batterys[i].OnDeath.RemoveListener(OnDeathBattery);
            _batterys[i].OnEndCooldown.RemoveListener(StartReactor);
        }
    }
    private void Update()
    {
        showingText.text = _currentIndexLiveBattery.ToString();
    }

    private IEnumerator Start()
    {
        _health = _healthReactor;
        _batteryHealth = _batterys.Count;

        for (int i = 0; i < _batterys.Count; i++)
        {
            _batterys[i].OnDeath.AddListener(OnDeathBattery);
            _batterys[i].OnEndCooldown.AddListener(StartReactor);
        }

        while(SceneLoader.instance.IsLoad)
            yield return null;

        _doorControllerSceneChanger = SceneLoader.instance.GetDoorControllerNextTransition();

        StartReactor();
    }

    public void Damage(float amount, GameObject damageSource)
    {
        if (_batteryHealth > 0)
            return;

        _health -= amount;

        if (_health <= 0)
            StartCoroutine(Death());
    }

    public void OnDeathBattery()
    {
        _batteryHealth -= 1;

        if (_batteryHealth <= 0)
        {
            StartCoroutine(OpenShield());
            return;
        }

        StartCoroutine(CooldownNextBattery());
    }

    public bool IsSwaped()
    {
        throw new System.NotImplementedException();
    }

    public void StartReactor()
    {
        if (_batteryHealth <= 0)
            return;

        while(_batterys[_currentIndexLiveBattery].IsDead)
        {
            _currentIndexLiveBattery++;
            

            if (_currentIndexLiveBattery >= _batterys.Count)
                _currentIndexLiveBattery = 0;
        }

        _batterys[_currentIndexLiveBattery++].OpenShield();

        if(_currentIndexLiveBattery >= _batterys.Count)
            _currentIndexLiveBattery = 0;
    }

    private IEnumerator CooldownNextBattery()
    {
        yield return new WaitForSeconds(_cooldownNextBattery);

        StartReactor();
    }

    private IEnumerator OpenShield()
    {
        yield return new WaitForSeconds(_timeBeforeOpenShield);

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

        _reactorObject.SetActive(false);

        OnDeath?.Invoke();

        if(_doorControllerSceneChanger != null)
            _doorControllerSceneChanger.ForceOpenEnterDoor();
    }
}
