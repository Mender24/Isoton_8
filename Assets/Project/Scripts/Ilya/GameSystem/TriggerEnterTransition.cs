using Akila.FPSFramework;
using System.Collections;
using UnityEngine;

public class TriggerEnterTransition : MonoBehaviour
{
    [SerializeField] private GameObject _dontExitObject;
    [Space]
    [SerializeField] private float _timeBeforeLoadNextLevel = 2f;
    [SerializeField] private DoorControllerSceneChanger _doorControllerSceneChanger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _))
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;
            StartCoroutine(StartChangeScene());
        }
    }

    private IEnumerator StartChangeScene()
    {
        _dontExitObject.SetActive(true);

        _doorControllerSceneChanger.ForceCloseEnterDoor();

        yield return new WaitForSeconds(_timeBeforeLoadNextLevel);
        yield return StartCoroutine(SceneLoader.instance.SceneRotationProcess());
        yield return new WaitForSeconds(1f);

        _doorControllerSceneChanger.ForceOpenExitDoor();
    }
}
