using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine;

public class DamageDirectionView : MonoBehaviour
{
    [SerializeField] private float _speedShowUp = 2f;
    [SerializeField] private float _speedShowDown = 2f;
    [SerializeField] private List<DamageCellView> _listView = new();

    private Transform _cameraRoot;
    private Damageable _damageable;

    private void OnDestroy()
    {
        if (_damageable != null)
            _damageable.DamageApplied -= OnDamageApplied;
    }

    private void Awake()
    {
        _damageable = transform.parent.transform.parent.GetComponent<Damageable>();

        if (_damageable != null)
            _damageable.DamageApplied += OnDamageApplied;
        else
            Debug.LogWarning("Damageable in DamageDirectionView is null!");

        foreach (Transform tr in transform.parent.transform.parent.transform)
        {
            _cameraRoot = tr;
            return;
        }
    }

    private void OnDamageApplied(GameObject gameObject)
    {
        if (gameObject == null)
            return;

        Vector3 dir = -(gameObject.transform.position - _damageable.gameObject.transform.position).normalized;
        dir.y = 0;
        Vector3 forward = _cameraRoot.transform.forward;
        forward.y = 0;


        float angle = Vector3.SignedAngle(dir, -forward, Vector3.down);

        ViewDamageDirection(angle);
    }

    public void ViewDamageDirection(float angle)
    {
        foreach (DamageCellView view in _listView)
        {
            if (view.CheckInRange(angle))
            {
                view.StartViewDirectionDamage(_speedShowUp, _speedShowDown);
                return;
            }
        }
    }
}