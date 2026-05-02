using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LampsController : MonoBehaviour
{
    [SerializeField] private Color _newColorLight;
    [Space]
    [SerializeField] private bool _isUseLerpColor = false;
    [SerializeField] private float _speedUpColor = 1f;
    [SerializeField] private float _speedDownColor = 1f;
    [SerializeField] private float _borderOffEmission = 0.3f;
    private List<Lamp> _lampsObject = new();
    private List<Light> _lights;

    private float _baseIntensityColor = 0;

    private void Start()
    {
        foreach (Transform t in transform)
            if(t.TryGetComponent(out Lamp lamp))
                _lampsObject.Add(lamp);

        _lights = transform.GetComponentsInChildren<Light>(true).ToList();

        _baseIntensityColor = _lights[0].intensity;
    }

    public void OffLamps()
    {
        if(_isUseLerpColor)
        {
            StopAllCoroutines();
            StartCoroutine(LerpItensity(false));
            return;
        }

        foreach(Light light in _lights)
            light.enabled = false;

        foreach(Lamp lamp in _lampsObject)
            lamp.OffEmission();
    }

    public void OnLamps()
    {
        if (_isUseLerpColor)
        {
            StopAllCoroutines();
            StartCoroutine(LerpItensity(true));
            return;
        }

        foreach (Light light in _lights)
            light.enabled = true;

        foreach (Lamp lamp in _lampsObject)
            lamp.OnEmission();
    }

    public void SetNewBaseMaterial(Material material)
    {
        foreach(Lamp lamp in _lampsObject)
            lamp.SetNewBaseMaterial(material);
    }

    public void SetNewColorLight()
    {
        foreach(Light light in _lights)
            light.color = _newColorLight;
    }

    private IEnumerator LerpItensity(bool isUp)
    {
        float target;

        if (isUp)
            target = _baseIntensityColor;
        else
            target = 0;

        bool isChangeEmmision = false;

        while (Mathf.Abs(_lights[0].intensity - target) > 0.001)
        {
            float newIntensity = Mathf.Lerp(_lights[0].intensity, target, (isUp ? _speedUpColor : _speedDownColor) * Time.deltaTime);
            Debug.Log(newIntensity);
            foreach (Light light in _lights)
                light.intensity = newIntensity;

            if (!isChangeEmmision && _lights[0].intensity < _borderOffEmission)
            {
                isChangeEmmision = true;

                foreach (Lamp lamp in _lampsObject)
                    if(isUp)
                        lamp.OnEmission();
                    else
                        lamp.OffEmission();
            }
                
            yield return null;
        }

        Debug.Log("end");
    }
}
