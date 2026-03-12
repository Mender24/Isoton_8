using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerLateLoadScene : MonoBehaviour
{
    [SerializeField] private bool _isPriorityUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            Debug.Log("Trigger");

            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            if (!SceneLoader.instance.IsScenesLoaded && !SceneLoader.instance.IsProgressLoadingScenes)
                StartAddLoadScene();

            if (_isPriorityUp)
                SceneLoader.instance.PriorityUp();
        }
    }

    private void StartAddLoadScene()
    {
        SceneLoader.instance.LateLoadScene();
    }
}
