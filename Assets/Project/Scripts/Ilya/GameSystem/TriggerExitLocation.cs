using Akila.FPSFramework;
using UnityEngine;

public class TriggerExitLocation : MonoBehaviour
{
    [SerializeField] private string _nameNextLevel;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");

        if(other.TryGetComponent(out Player _))
        {
            Debug.Log("Exit");
            SceneLoader.instance.LoadStartScene(_nameNextLevel, true);
        }
    }
}
