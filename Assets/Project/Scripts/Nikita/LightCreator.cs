using UnityEngine;

namespace LightCreator
{
    public class LightCreator : MonoBehaviour
    {
        public GameObject lightObject;  // Перетащите сюда GameObject с Light
        public float activeTime = 0.05f; // Свет горит 0.05 секунды

        [SerializeField] private ParticleSystem particleSystem;
        private float timer;
        private bool lightOn;

        void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();

            // Гарантируем, что свет выключен в начале
            if (lightObject != null)
            {
                lightObject.SetActive(false);
            }
        }

        void Update()
        {
            // Если Particle System проигрывается - включаем свет
            if (particleSystem.isPlaying && particleSystem.particleCount > 0)
            {
                if (!lightOn)
                {
                    TurnOnLight();
                }
                timer = activeTime; // Сбрасываем таймер
            }

            // Таймер выключения
            if (lightOn)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    TurnOffLight();
                }
            }
        }

        void TurnOnLight()
        {
            if (lightObject != null)
            {
                lightObject.SetActive(true);
                lightOn = true;
            }
        }

        void TurnOffLight()
        {
            if (lightObject != null)
            {
                lightObject.SetActive(false);
                lightOn = false;
            }
        }

        void OnDisable()
        {
            TurnOffLight();
        }
    }
}