using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Managers/Game Manager")]
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [SerializeField] private Player _player;
        [SerializeField] DeathCamera deathCamera;
        [SerializeField] UIManager uIManager;

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Instantiate(deathCamera, _player.transform);

            if(uIManager != null)
                Instantiate(uIManager, _player.transform);
        }
    }
}