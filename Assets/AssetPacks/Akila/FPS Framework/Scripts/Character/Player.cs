using UnityEngine;

namespace Akila.FPSFramework
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        private Inventory _inventory;
        private Actor _actor;

        public Inventory Inventory => _inventory;
        public Actor Actor => _actor;

        private void Awake()
        {
            Instance = this;

            _inventory = GetComponentInChildren<Inventory>();
            _actor = GetComponent<Actor>();

            gameObject.SetActive(false);
        }
    }
}