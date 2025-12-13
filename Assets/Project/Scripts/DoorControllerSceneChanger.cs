using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorControllerSceneChanger : MonoBehaviour
{

    public Transform pivot;
    public BunkerDoor enterDoor;
    public BunkerDoor exitDoor;
    public string interactionName = "Activate Bunker Doors";
    private bool _isActivated = false;
    public float roughness = 2;
    private Quaternion targetRotation;
    private SceneManagerMy sceneManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
