using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.TryGetComponent(out Actor actor))
        {
            BoxCollider bc = GetComponent<BoxCollider>();
            bc.enabled = false;
            SpawnManager.Instance.SetNewSpawnPoint();
        }
    }
}
