using Akila.FPSFramework;
using System;
using System.Globalization;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerActiveEarthquake : MonoBehaviour
{
    [SerializeField] private DataCameraShake _dataCameraShake;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player _))
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            Earthquake.Instance.ShakeCamera(_dataCameraShake);
        }
    }
}

[Serializable]
public class DataCameraShake
{
    public string SoundName = "";
    public float Duration = 11;
    [Space]
    public float CameraShakeMultiplier = 1;
    public float Roughness = 1;
    public float FadeInTime = 20;
    public float FadeOutTime = 5;
}