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

        public void ActivatedLeaver()
        {
            if (!_isActivated)
            {
                _isActivated = true;
                ToggleLeaver();
                StartCoroutine(StartChangeSceneProcess());
            }
        }

        private IEnumerator StartChangeSceneProcess()
        {
            enterDoor.ToggleDoor();

            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(SceneManagerMy.instance.SceneRotationProcess());
            yield return new WaitForSeconds(1f);

            exitDoor.ToggleDoor();
        }

        private void ToggleLeaver()
        {
            targetRotation = targetRotation == Quaternion.Euler(0, 0, 150) ? Quaternion.identity : Quaternion.Euler(0, 0, 10);
        }
    }
}
