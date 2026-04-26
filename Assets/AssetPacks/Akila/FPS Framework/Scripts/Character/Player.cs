using System;
using System.Globalization;
using UnityEngine;

namespace Akila.FPSFramework
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;
        [SerializeField] private bool _isStartOff = false;

        private Inventory _inventory;
        private Actor _actor;
        private InteractionsManager _interactionsManager;
        private FirstPersonController _firstPersonController;
        private CharacterManager _characterManager;

        public Inventory Inventory => _inventory;
        public Actor Actor => _actor;
        public InteractionsManager InteractionsManager => _interactionsManager;

        private void Awake()
        {
            Instance = this;

            _inventory = GetComponentInChildren<Inventory>();
            _interactionsManager = GetComponentInChildren<InteractionsManager>();
            _actor = GetComponent<Actor>();
            _firstPersonController = GetComponent<FirstPersonController>();
            _characterManager = GetComponent<CharacterManager>();
        }

        private void Start()
        {
            if (_isStartOff)
                gameObject.SetActive(false);
        }

        public void SetRotation(Quaternion rotation)
        {
            _firstPersonController.SetPlayerRotation(rotation);
        }

        public void ShakeCamera(float multiplier, float roughness, float fadeInTime, float fadeOutTime)
        {
            if(_characterManager == null)
            {
                Debug.LogWarning("_characterManager is null");
                return;
            }

            _characterManager.cameraManager.ShakeCameras(multiplier, roughness, fadeInTime, fadeOutTime);
        }
    }
}