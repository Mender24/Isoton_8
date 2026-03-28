using UnityEngine;

public class BatteryAnim : MonoBehaviour
{
    [SerializeField] private string _nameOpen;
    [SerializeField] private string _nameClose;
    [SerializeField] private WireStateSwitcher _wire;

    private Battery _battery;
    private Animator _animator;

    private void OnDestroy()
    {
        if (_battery == null)
            return;

        _battery.OnStartUpShield -= OnStartUpShield;
        _battery.OnStartDownShield -= OnStartDownShield;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _battery = transform.parent.GetComponent<Battery>();

        _battery.OnStartUpShield += OnStartUpShield;
        _battery.OnStartUpShield += ActivateWire;
        _battery.OnStartDownShield += OnStartDownShield;
        _battery.OnStartDownShield += DeactivateWire;
        _battery.OnDeath.AddListener(DeactivateWire);
    }

    private void OnStartUpShield()
    {
        _animator.SetTrigger(_nameOpen);
    }

    private void OnStartDownShield()
    {
        _animator.SetTrigger(_nameClose);
    }

    private void ActivateWire()
    {
        if(_wire != null)
            _wire.ActivateIsoton();
    }

    private void DeactivateWire()
    {
        if (_wire != null)
            _wire.ActivateRubber();
    }
}
