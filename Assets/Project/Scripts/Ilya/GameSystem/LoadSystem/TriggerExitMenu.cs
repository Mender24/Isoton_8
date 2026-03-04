using UnityEngine;

public class TriggerExitMenu : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SceneLoader.instance.LoadMainMenu();
    }
}
