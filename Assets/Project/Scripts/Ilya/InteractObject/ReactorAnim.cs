using UnityEngine;

public class ReactorAnim : MonoBehaviour
{
    [SerializeField] private string _nameOpen;

    private Reactor _reactor;
    private Animator _animator;

    private void OnDestroy()
    {
        if (_reactor == null)
            return;

        _reactor.OnBatteryDestroy -= OnBatteryDestroy;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _reactor = transform.parent.GetComponent<Reactor>();

        _reactor.OnBatteryDestroy += OnBatteryDestroy;
    }

    private void OnBatteryDestroy()
    {
        _animator.SetTrigger(_nameOpen);
    }
}
