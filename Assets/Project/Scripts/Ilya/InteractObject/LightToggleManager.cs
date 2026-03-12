using System.Collections.Generic;
using UnityEngine;

public class LightToggleManager : MonoBehaviour
{
    [SerializeField] private List<Light> _OnLights = new();
    [SerializeField] private List<Light> _OffLights = new();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if(child.TryGetComponent(out Light light))
            {
                if (child.gameObject.activeSelf)
                    _OnLights.Add(light);
                else
                    _OffLights.Add(light);
            }
        }
    }

    public void OnChangeState()
    {
        foreach (Light child in _OnLights)
            child.gameObject.SetActive(false);

        foreach (Light child in _OffLights)
            child.gameObject.SetActive(true);
    }
}
