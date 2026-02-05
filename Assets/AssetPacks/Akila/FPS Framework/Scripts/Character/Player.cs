using UnityEngine;
namespace Akila.FPSFramework
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}