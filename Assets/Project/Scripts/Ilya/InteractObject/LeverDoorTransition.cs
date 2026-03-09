using Akila.FPSFramework;
using UnityEngine;

public class LeverDoorTransition : MonoBehaviour
{
    [SerializeField] private Transform _pivot;
    [SerializeField] private float _speed;

    private bool _isActive = false;
    private Quaternion _targetRotation;

    void Update()
    {
        if (_pivot != null)
            _pivot.localRotation = Quaternion.Lerp(_pivot.localRotation, _targetRotation, Time.deltaTime * _speed);
    }

    public void OpenDoor()
    {
        if (_isActive)
            return;

        _isActive = true;

        ToggleLeaver();

        DoorControllerSceneChanger doorController = SceneLoader.instance.GetDoorControllerNextTransition();

        if (doorController == null)
        {
            Debug.LogError("DoorTransition is null!");
            return;
        }

        doorController.ForceOpenEnterDoor();
    }

    private void ToggleLeaver()
    {
        _targetRotation = _targetRotation == Quaternion.Euler(0, 0, 150) ? Quaternion.identity : Quaternion.Euler(0, 0, 10);
    }
}
