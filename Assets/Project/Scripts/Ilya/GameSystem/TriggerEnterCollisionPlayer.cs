using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class TriggerEnterCollisionPlayer : MonoBehaviour
{
    public UnityEvent CollisionEntered;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player _))
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            CollisionEntered?.Invoke();
        }
    }
}
