using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.Events;

public class Battery : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 100f;

    private UnityEvent onDeath = new UnityEvent();

    public bool isDamagableDisabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool allowDamageableEffects { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float Health { get => _health; set => throw new System.NotImplementedException(); }
    public bool DeadConfirmed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameObject DamageSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public UnityEvent OnDeath => onDeath;

    public void Damage(float amount, GameObject damageSource)
    {
        _health -= amount;

        if(_health <= 0)
        {
            Death();
        }
    }

    public bool IsSwaped()
    {
        throw new System.NotImplementedException();
    }

    private void Death()
    {
        _health = 0;
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }
}
