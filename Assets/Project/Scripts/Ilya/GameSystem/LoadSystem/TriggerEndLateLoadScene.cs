using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class TriggerEndLateLoadScene : MonoBehaviour
{
    public UnityEvent TriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player _))
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            TriggerEnter?.Invoke();

            if (SceneLoader.instance == null || !SceneLoader.instance.IsLateLoadingSystem)
                return;

            StartCoroutine(SceneLoader.instance.FinishLateLoadScene());
        }
    }
}
