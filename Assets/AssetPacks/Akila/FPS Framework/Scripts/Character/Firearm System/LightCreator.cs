using System.Collections;
using UnityEngine;

namespace Akila.FPSFramework
{
    public class LightCreator : MonoBehaviour
    {
        public GameObject lightObject;  // Перетащите сюда GameObject с Light
        public float activeTime = 0.05f; // Свет горит 0.05 секунды

        void Awake()
        {
            if (lightObject != null)
            {
                lightObject.SetActive(false);
            }
        }

        public void TurnOnLight()
        {
            if (lightObject != null)
            {
                lightObject.SetActive(true);
                StartCoroutine(TurnOffLight(activeTime));
            }
        }

        private IEnumerator TurnOffLight(float time)
        {
            yield return new WaitForSeconds(time);

            if (lightObject != null)
            {
                lightObject.SetActive(false);
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
            lightObject.SetActive(false);
        }
    }
}