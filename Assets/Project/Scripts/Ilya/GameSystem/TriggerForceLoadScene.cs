using Akila.FPSFramework;
using UnityEngine;

public class TriggerForceLoadScene : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");

        if (other.TryGetComponent(out Player _))
        {
            Debug.Log("Exit");
            SceneLoader.instance.LoadNextScene();
        }
    }
}
