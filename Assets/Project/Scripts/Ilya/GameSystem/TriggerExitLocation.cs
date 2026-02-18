using Akila.FPSFramework;
using UnityEngine;

public class TriggerExitLocation : MonoBehaviour
{
    [SerializeField] private string _loadSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player _))
            SceneLoader.instance.LoadScenes(true, _loadSceneName, true);
    }
}
