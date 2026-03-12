using UnityEngine;

public class ActivateObject : MonoBehaviour
{

    [SerializeField] GameObject objToActivate;

    public void ActivateObj()
    {
        objToActivate.SetActive(true);
    }
}
