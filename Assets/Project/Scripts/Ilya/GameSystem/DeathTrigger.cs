using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private float _damage = 200f;
    [SerializeField] private bool _isOnlyPlayer = true;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamageable damageable))
        {
            if (_isOnlyPlayer && !other.TryGetComponent(out Player player))
                return;

            damageable.Damage(_damage, null);
        }
    }
}
