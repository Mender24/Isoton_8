using Akila.FPSFramework;
using UnityEngine;

public class ShotImpactBroadcaster : MonoBehaviour
{
    [SerializeField] private float _alertRadius = 12f;

    private static readonly Collider[] _buffer = new Collider[32];

    private void OnEnable()
    {
        Firearm.OnShotImpact += HandleShotImpact;
    }

    private void OnDisable()
    {
        Firearm.OnShotImpact -= HandleShotImpact;
    }

    private void HandleShotImpact(Vector3 hitPoint, Vector3 shooterPosition)
    {
        int count = Physics.OverlapSphereNonAlloc(hitPoint, _alertRadius, _buffer);

        for (int i = 0; i < count; i++)
        {
            if (_buffer[i] == null) continue;

            var enemy = _buffer[i].GetComponentInParent<EnemyBase>();
            if (enemy != null)
                enemy.HearNearbyShot(shooterPosition);
        }
    }
}
