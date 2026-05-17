using System.Collections;
using UnityEngine;

namespace Akila.FPSFramework
{
    public class DoorControllerSceneChanger : MonoBehaviour
    {
        [SerializeField] private bool _isUseLoad = true;
        [SerializeField] private bool _isUseForceDontOpenDoor = false;
        [SerializeField] private bool _isUseLateLoadedSystem = false;
        [SerializeField] private string _soundName = "Lever_2";
        [Space]
        public Transform pivot;
        public BunkerDoor enterDoor;
        public BunkerDoor exitDoor;
        private bool _isActivated = false;
        public float roughness = 2;
        private Quaternion targetRotation;

        private Interactable _interactable;

        public bool EnterDoorIsOpen => enterDoor.IsOpen;

        private void Start()
        {
            if(TryGetComponent(out Interactable interactable))
                _interactable = interactable;
        }

        void Update()
        {
            if (pivot != null)
                pivot.localRotation = Quaternion.Lerp(pivot.localRotation, targetRotation, Time.deltaTime * roughness);
        }

        public void ActivatedLeaver(bool isActive = false)
        {
            if (!_isActivated)
            {
                _isActivated = true;
                ToggleLeaver();

                if (isActive && _isUseLoad)
                    StartCoroutine(StartChangeSceneProcess());
            }
        }

        public void EnterExitDoor()
        {
            ActivatedLeaver();

            if (_interactable != null)
                _interactable.OffInteraction();

            if(enterDoor != null)
                enterDoor.CloseDoor();

            if (exitDoor != null)
                exitDoor.OpenDoor();
        }

        public void EnterOpenDoor()
        {
            if (_isUseForceDontOpenDoor)
                return;

            if (enterDoor != null)
                enterDoor.OpenDoor();

            if (exitDoor != null)
                exitDoor.CloseDoor();
        }

        public void ForceOpenEnterDoor()
        {
            if (enterDoor != null)
                enterDoor.OpenDoor();
        }

        public void ForceCloseEnterDoor()
        {
            if (enterDoor != null)
                enterDoor.CloseDoor();
        }

        public void ForceOpenExitDoor()
        {
            if (exitDoor != null)
                exitDoor.OpenDoor();
        }

        private IEnumerator StartChangeSceneProcess()
        {
            if (enterDoor != null)
                enterDoor.CloseDoor();

            yield return null;

            if(!_isUseLateLoadedSystem)
                SceneLoader.instance.LoadScenes(isUseSave: true);
            else
                StartCoroutine(SceneLoader.instance.FinishLateLoadScene());
        }

        private void ToggleLeaver()
        {
            targetRotation = targetRotation == Quaternion.Euler(0, 0, 150) ? Quaternion.identity : Quaternion.Euler(0, 0, 10);

            Debug.Log("Start");

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayScriptedOneShotSoundName(_soundName);
        }
    }
}
