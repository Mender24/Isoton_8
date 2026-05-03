using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DrillController : MonoBehaviour
{
    [SerializeField] private AudioSource _drillAudio;
    [SerializeField] private List<ParticleSystem> _particles = new();
    [SerializeField] private float _speedChangePitch = 1f;
    [Space]
    [SerializeField] private DataCameraShake _dataCameraShake;
    [Space]
    [Header("DrillController")]
    [SerializeField] private float _timeWaitBeforeAlert = 2f;
    [Space]
    [SerializeField] private string _alarmSoundName = "";
    [SerializeField] private int _countRepeat = 5;
    [SerializeField] private float _timeWaitBeforeNextRepeat = 0.5f;
    [Space]
    [SerializeField] private float _timeWaitBeforeStartNotificationAudio = 1.5f;
    [SerializeField] private string _notificationSoundName = "";
    [Space]
    [SerializeField] private float _timeWaitBeforeStarting = 1f;
    [Space]
    [SerializeField] private float _timeWaitBeforeStartEffect = 1f;
    [Space]
    [SerializeField] private float _timeWaitBeforeEndInteraction = 2f;
    [Space]
    [SerializeField] private int _currentHealth = 3;

    private bool _isDied = false;

    public UnityEvent InteractionStarted;
    public UnityEvent AlertStarted;
    public UnityEvent AfterAlert;
    public UnityEvent InteractionEnded;

    public event Action Started;
    public event Action Stopped;

    private void Update()
    {
        if (!_isDied && _currentHealth == 0)
        {
            _isDied = true;
            StartCoroutine(StartEventDeath());
        }
    }

    public void TakeDamage()
    {
        _currentHealth -= 1;
    }

    private IEnumerator StartEventDeath()
    {
        Stopped?.Invoke();
        InteractionStarted?.Invoke();

        float startPitch = _drillAudio.pitch;
        Coroutine action = StartCoroutine(LerpPith(0));

        foreach (var particle in _particles)
            particle.Stop();

        yield return new WaitForSeconds(_timeWaitBeforeAlert);

        AlertStarted?.Invoke();

        StartCoroutine(AudioRepeater(_alarmSoundName, _countRepeat, _timeWaitBeforeNextRepeat));

        yield return new WaitForSeconds(_timeWaitBeforeStartNotificationAudio);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayScriptedSoundName(_notificationSoundName);

        AfterAlert?.Invoke();

        yield return new WaitForSeconds(_timeWaitBeforeStarting);

        Started?.Invoke();

        if (Earthquake.Instance != null)
            Earthquake.Instance.ShakeCamera(_dataCameraShake);

        if(action != null)
            StopCoroutine(action);
        
        StartCoroutine(LerpPith(startPitch));

        yield return new WaitForSeconds(_timeWaitBeforeStartEffect);

        foreach (var particle in _particles)
            particle.Play();

        yield return new WaitForSeconds(_timeWaitBeforeEndInteraction);

        InteractionEnded?.Invoke();
    }

    private IEnumerator LerpPith(float target)
    {
        if (target != 0)
            _drillAudio.Play();

        while (Mathf.Abs(_drillAudio.pitch - target) > 0.001)
        {
            float newPitch = Mathf.Lerp(_drillAudio.pitch, target, _speedChangePitch * Time.deltaTime);
            _drillAudio.pitch = newPitch;
            yield return null;
        }
        if(target == 0)
            _drillAudio.Stop();
    }

    private IEnumerator AudioRepeater(string soundName, int countRepeat, float timeWaitBeforeNextRepeat)
    {
        for(int i = 0; i < countRepeat; i++)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayScriptedOneShotSoundName(soundName);

            yield return new WaitForSeconds(_timeWaitBeforeNextRepeat);
        }
    }
}
