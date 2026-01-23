using UnityEngine;
namespace Akila.FPSFramework
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}