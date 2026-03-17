using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateActiveObject : MonoBehaviour
{
    [SerializeField] private bool _isMain = false;
    [SerializeField] private bool _isEnable = true;
    [SerializeField] private bool _isEnableActivator = true;
    [SerializeField] private bool _isStartActive = false;
    [Space]
    [SerializeField] private bool _isFrameSkip = false;
    [SerializeField] private int _countObjectInFrame = 3;
    [SerializeField] private float _timeBetweenActive = 0.1f;
    [SerializeField] private LateActivatorObjects _objects;
    [SerializeField] private List<SpeedTypeProfile> _multiplicativeCoeffSpeed = new();

    [SerializeField] private SpeedType _currentSpeedType = SpeedType.Slowly;

    private IEnumerator Start()
    {
        if (_isStartActive)
        {
            yield return StartActivate();
        }
    }

    private void Update()
    {
        if(_isMain && _currentSpeedType != SceneLoader.instance.SpeedType)
        {
            ChangeSpeedActive(SceneLoader.instance.SpeedType);
        }
    }

    public IEnumerator StartActivate(SpeedTypeProfile speedProfile = null)
    {
        if (_isEnable)
        {
            if(!_isMain && speedProfile != null && speedProfile.SpeedType == SpeedType.VeryFast)
            {
                _countObjectInFrame = speedProfile.CountInFrameObject;
                _timeBetweenActive = speedProfile.TimeBetweenActive;
                _isFrameSkip = speedProfile.IsFrameSkip;
            }

            int currentActive = 0;
            float time;

            foreach (Transform obj in transform)
            {
                currentActive++;
                obj.gameObject.SetActive(true);

                if (currentActive >= _countObjectInFrame)
                {
                    currentActive = 0;

                    if (_isFrameSkip)
                        time = Time.deltaTime;
                    else
                        time = _timeBetweenActive;

                    while (time > 0f)
                    {
                        time -= Time.deltaTime;
                        yield return null;
                    }
                }
            }
        }

        if (_objects != null && _isEnableActivator)
            yield return StartCoroutine(_objects.ActivateLateActiveObject(GetSpeedTypeProfile(_currentSpeedType)));
    }

    private void ChangeSpeedActive(SpeedType speedType)
    {
        _currentSpeedType = speedType;

        SpeedTypeProfile profile = GetSpeedTypeProfile(_currentSpeedType);

        _countObjectInFrame = profile.CountInFrameObject;
        _timeBetweenActive = profile.TimeBetweenActive;
        _isFrameSkip = profile.IsFrameSkip;
    }

    private SpeedTypeProfile GetSpeedTypeProfile(SpeedType speedType)
    {
        foreach (SpeedTypeProfile profile in _multiplicativeCoeffSpeed)
            if (profile.SpeedType == _currentSpeedType)
                return profile;

        return null;
    }
}

[Serializable]
public class SpeedTypeProfile
{
    public SpeedType SpeedType;
    public int CountInFrameObject;
    public float TimeBetweenActive;
    public bool IsFrameSkip = false;
}