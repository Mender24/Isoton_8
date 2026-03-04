using Akila.FPSFramework;
using UnityEngine;

public class TriggerExitMenu : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player _))
            SceneLoader.instance.LoadMainMenu();
    }
}