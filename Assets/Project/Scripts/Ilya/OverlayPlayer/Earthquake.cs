using Akila.FPSFramework;
using System;
using UnityEngine;

public class Earthquake : MonoBehaviour
{
    private static Earthquake _instance;

    [SerializeField] private float _cameraShakeMultiplier = 0.5f;
    [SerializeField] private float _roughness = 1;
    [Range(0, 100)]
    [SerializeField] private float _fadeInTime = 0.01f;
    [Range(0, 10)]
    [SerializeField] private float _fadeOutTime = 2f;
    [Space]
    [SerializeField] private string _baseSoundEarthquakeName = "";
    [Space]
    [SerializeField] private bool _isTest = false;

    public event Action<float> StartEarthquake;

    public static Earthquake Instance => _instance;

    private void Awake()
    {
        if( _instance == null )
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        if(_isTest)
        {
            _isTest = false;
            ShakeCamera(new DataCameraShake());
        }
    }

    public void ShakeCamera(DataCameraShake dataCameraShake)
    {
        if(Player.Instance == null)
        {
            Debug.LogWarning("Player is null!");
            return;
        }

        StartEarthquake?.Invoke(dataCameraShake.Duration);
        Player.Instance.ShakeCamera(dataCameraShake.CameraShakeMultiplier, dataCameraShake.Roughness, dataCameraShake.FadeInTime, dataCameraShake.FadeOutTime);
        SoundManager.Instance.PlayScriptedSoundName(dataCameraShake.SoundName);
    }
}
