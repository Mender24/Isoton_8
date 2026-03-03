using UnityEngine;

namespace Akila.FPSFramework
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        private Inventory _inventory;
        private Actor _actor;
        private InteractionsManager _interactionsManager;

        public Inventory Inventory => _inventory;
        public Actor Actor => _actor;
        public InteractionsManager InteractionsManager => _interactionsManager;

        private void Awake()
        {
            Instance = this;

            _inventory = GetComponentInChildren<Inventory>();
            _interactionsManager = GetComponentInChildren<InteractionsManager>();
            _actor = GetComponent<Actor>();

            gameObject.SetActive(false);
        }
    }
}