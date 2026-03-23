using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateActiveObject : MonoBehaviour
{
    [SerializeField] private LateActiveObject _postLoadActive;
    [SerializeField] private bool _isActiveObject = false;
    [SerializeField] private bool _isMain = false;
    [SerializeField] private bool _isEnable = true;
    [SerializeField] private bool _isEnableActivator = true;
    [SerializeField] private bool _isStartActive = false;
    [Space]
    [SerializeField] private bool _isFrameSkip = false;
    [SerializeField] private int _countObjectInFrame = 3;
    [SerializeField] private float _timeBetweenActive = 0.1f;
    [SerializeField] private LateActivatorObjects _objects;
    [SerializeField]
    private List<SpeedTypeProfile> _multiplicativeCoeffSpeedType = new()
    {
        new SpeedTypeProfile()
        {
            SpeedType = SpeedType.Slowly,
            CountInFrameObject = 5,
            TimeBetweenActive = 0.15f,
            IsFrameSkip = false,
        },
        new SpeedTypeProfile()
        {
            SpeedType = SpeedType.Fast,
            CountInFrameObject = 10,
            TimeBetweenActive = 0,
            IsFrameSkip = true,
        },
        new SpeedTypeProfile()
        {
            SpeedType = SpeedType.VeryFast,
            CountInFrameObject = 40,
            TimeBetweenActive = 0,
            IsFrameSkip = true,
        }
    };

    [SerializeField] private SpeedType _currentSpeedType = SpeedType.Slowly;

    public bool IsActiveObject => _isActiveObject;

    private IEnumerator Start()
    {
        ChangeSpeedActive(_currentSpeedType);

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
            StartCoroutine(PostLoadActiveObject(speedProfile));

            if(!_isMain && speedProfile != null && speedProfile.SpeedType == SpeedType.VeryFast)
            {
                _countObjectInFrame = speedProfile.CountInFrameObject;
                _timeBetweenActive = speedProfile.TimeBetweenActive;
                _isFrameSkip = speedProfile.IsFrameSkip;
            }

            if(_isActiveObject)
            {
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
    }

    private IEnumerator PostLoadActiveObject(SpeedTypeProfile speedProfile = null)
    {
        if (_postLoadActive != null)
        {
            while (SceneLoader.instance.IsProgressUnloadingScenes)
                yield return null;

            _postLoadActive.gameObject.SetActive(true);
            yield return null;
            StartCoroutine(_postLoadActive.StartActivate());
        }
    }

    public void ChangeSpeedActive(SpeedType speedType)
    {
        _currentSpeedType = speedType;

        SpeedTypeProfile profile = GetSpeedTypeProfile(_currentSpeedType);

        _countObjectInFrame = profile.CountInFrameObject;
        _timeBetweenActive = profile.TimeBetweenActive;
        _isFrameSkip = profile.IsFrameSkip;

        if(_objects != null)
            _objects.ChangeSpeedProfile(_currentSpeedType);

        if(_postLoadActive != null)
            _postLoadActive.ChangeSpeedActive(_currentSpeedType);
    }

    private SpeedTypeProfile GetSpeedTypeProfile(SpeedType speedType)
    {
        foreach (SpeedTypeProfile profile in _multiplicativeCoeffSpeedType)
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