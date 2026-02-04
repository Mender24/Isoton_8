using System;
using System.Collections;
using UnityEngine;

namespace Akila.FPSFramework
{
    public class DoorControllerSceneChanger : MonoBehaviour
    {
        public Transform pivot;
        public BunkerDoor enterDoor;
        public BunkerDoor exitDoor;
        private bool _isActivated = false;
        public float roughness = 2;
        private Quaternion targetRotation;

        void Update()
        {
            pivot.localRotation = Quaternion.Lerp(pivot.localRotation, targetRotation, Time.deltaTime * roughness);
        }

        public void ActivatedLeaver(bool isActive = false)
        {
            if (!_isActivated)
            {
                _isActivated = true;
                ToggleLeaver();

                if(isActive)
                    StartCoroutine(StartChangeSceneProcess());
            }
        }

        public void EnterNext()
        {
            ActivatedLeaver();
            enterDoor.CloseDoor();
            exitDoor.OpenDoor();
        }

        public void EnterLastLocation()
        {
            enterDoor.OpenDoor();
            exitDoor.CloseDoor();
        }

        private IEnumerator StartChangeSceneProcess()
        {
            enterDoor.CloseDoor();

            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(SceneLoader.instance.SceneRotationProcess());
            yield return new WaitForSeconds(1f);

            exitDoor.OpenDoor();
        }

        private void ToggleLeaver()
        {
            targetRotation = targetRotation == Quaternion.Euler(0, 0, 150) ? Quaternion.identity : Quaternion.Euler(0, 0, 10);
        }
    }
}
