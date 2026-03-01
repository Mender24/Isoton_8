using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (SpawnManager.Instance == null)
            return;
           
        if(other != null && other.TryGetComponent(out Actor actor))
        {
            BoxCollider bc = GetComponent<BoxCollider>();
            bc.enabled = false;
            SpawnManager.Instance.WritePlayerWeapon(actor);
            SpawnManager.Instance.SetNewSpawnPoint();
        }
    }
}
