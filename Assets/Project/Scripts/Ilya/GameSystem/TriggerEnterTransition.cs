using Akila.FPSFramework;
using System.Collections;
using UnityEngine;

public class TriggerEnterTransition : MonoBehaviour
{
    [SerializeField] private GameObject _dontExitObject;
    [Space]
    [SerializeField] private float _timeBeforeLoadNextLevel = 0;
    [SerializeField] private DoorControllerSceneChanger _doorControllerSceneChanger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _))
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            if(_doorControllerSceneChanger.EnterDoorIsOpen)
                StartCoroutine(StartChangeScene());
        }
    }

    private IEnumerator StartChangeScene()
    {
        if(_dontExitObject != null)
            _dontExitObject.SetActive(true);

        if (_doorControllerSceneChanger != null)
            _doorControllerSceneChanger.ForceCloseEnterDoor();

        yield return new WaitForSeconds(_timeBeforeLoadNextLevel);
        SceneLoader.instance.LoadScenes(isUseSave: true);
    }
}
