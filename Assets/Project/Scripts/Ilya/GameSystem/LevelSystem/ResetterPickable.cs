using Akila.FPSFramework;
using UnityEngine;

public class ResetterPickable : MonoBehaviour
{
    private GameObject _copyObject;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Start()
    {
        SaveObject();

        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(RespawnObject);
    }

    public void RespawnObject(string name)
    {
        _copyObject.transform.position = _startPosition;
        _copyObject.transform.rotation = _startRotation;
        _copyObject.SetActive(true);

        SaveObject();
    }

    private void SaveObject()
    {
        GameObject saveObject = null;

        foreach (Transform t in transform)
        {
            saveObject = t.gameObject;
            break;
        }

        _copyObject = Instantiate(saveObject, transform);
        _startPosition = saveObject.transform.position;
        _startRotation = saveObject.transform.rotation;
        _copyObject.SetActive(false);
    }
}
