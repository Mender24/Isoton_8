using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private float _damage = 200f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamageable damageable))
        {
            damageable.Damage(_damage, null);
        }
    }
}
