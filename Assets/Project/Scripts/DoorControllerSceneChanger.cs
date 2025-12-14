using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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
        private SceneManagerMy sceneManager;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            pivot.localRotation = Quaternion.Lerp(pivot.localRotation, targetRotation, Time.deltaTime * roughness);
        }

        public void ActivatedLeaver()
        {

            sceneManager = FindFirstObjectByType<SceneManagerMy>();
            if (sceneManager == null)
            {
                Debug.LogWarning("SceneManagerMy not found in the scene.");
            }
            if (!_isActivated)
            {
                _isActivated = true;
                ToggleLeaver();
                StartCoroutine(StartChangeSceneProcess());
            }
            else
            {

            }
        }

        private IEnumerator StartChangeSceneProcess()
        {
            enterDoor.ToggleDoor();
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(sceneManager.SceneRotationProcess());
            yield return new WaitForSeconds(1f);
            exitDoor.ToggleDoor();

        }

        private void ToggleLeaver()
        {
            targetRotation = targetRotation == Quaternion.Euler(0, 0, 150) ? Quaternion.identity : Quaternion.Euler(0, 0, 10);
        }
    }
}
