using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DropOnDeathEffect : MonoBehaviour
{
    [SerializeField] private GameObject _ammoBoxPrefab;
    [SerializeField] private Transform _dropTransform;
    [SerializeField] private float _dropDelay = 1f;

    void Start()
    {
        if (!_ammoBoxPrefab)
            Debug.LogError("No ammo prefab on " + gameObject.name);
    }

    public void DropAmmoBox()
    {
        StartCoroutine(DropAmmoRoutine());
    }
    
    private IEnumerator DropAmmoRoutine()
    {
        yield return new WaitForSeconds(_dropDelay);

        if (_dropTransform)
        {
            GameObject ammoGO = Instantiate(_ammoBoxPrefab, _dropTransform.position, Quaternion.identity, _dropTransform);
            ammoGO.transform.parent = null;
        }
        else
        {
            Vector3 botPos = GetComponentInChildren<Renderer>().GameObject().transform.position;
            Instantiate(_ammoBoxPrefab, botPos, Quaternion.identity); // Fallback but likely will be slightly off real mesh position
        }
    }
}