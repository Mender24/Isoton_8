using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance => _instance;

    [SerializeField] private bool _isPlayAwake = true;
    [SerializeField] private AudioSource _ambiemtSource;
    [Header("AmbientInLocation")]
    [SerializeField] private AudioClip _audioClipInTransition;
    [Range(0, 1f)]
    [SerializeField] private float _maxVolumeTransition;
    [Space]
    [SerializeField] private List<AudioClipInLocation> _audioAmbientInIdLocation = new();
    [SerializeField] private float _speedDown = 2f;
    [SerializeField] private float _speedUp = 2f;
    private Dictionary<int, AudioClipInLocation> _ambientInIdLocationAudioClip;

    public float SpeedDown { get; private set; }
    public float SpeedUp { get; private set; }

    [Space]
    [Header("RandomAudioClip")]
    [SerializeField] private AudioSource _randomSoundSource;
    [SerializeField] private bool _isChangePositionAudioSource;
    [SerializeField] private float _radiusInCircle = 2f;
    [SerializeField] private float _percentageOccurence = 0.2f;
    [SerializeField] private float _timeBetweenRandomAudio = 5f;
    [SerializeField] private List<ProfileRandomAudioClip> _randomAudioClip = new();
    private Dictionary<int, List<CellAudioClip>> _audioProfileLocationId = new();
    private float _currentLastTimeRandomAudio = 0;
    private bool _isActiveSystemRandomAudio = false;

    [Space]
    [Header("ScriptedAudioClip")]
    [SerializeField] private AudioSource _scriptedAudioSourse;
    [SerializeField] private List<ProfileScriptedAudioClip> _scriptedAudios = new();
    private Dictionary<string, ProfileScriptedAudioClip> _scriptedAudioClipInName;

    private void OnDestroy()
    {
        SceneLoader.instance.LevelLoaded -= OnLevelLoaded;
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitAmbientInLocation();
        InitRandomAudioClip();
        InitScriptedAudioClip();

        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(ResetScriptedAudioClip);

        if (_isPlayAwake)
            StartCoroutine(SetAmbientClip(_ambiemtSource, 0, false));
    }

    public void Update()
    {
        if (_isActiveSystemRandomAudio)
            UpdateFrame();
    }

    #region AmbientInLocation

    private void InitAmbientInLocation()
    {
        if (_ambiemtSource != null)
            _ambiemtSource.volume = 0;

        SpeedDown = _speedDown;
        SpeedUp = _speedUp;

        _ambientInIdLocationAudioClip = new();

        foreach (AudioClipInLocation locationAudioClip in _audioAmbientInIdLocation)
            _ambientInIdLocationAudioClip.Add(locationAudioClip.IdLocation, locationAudioClip);

        SceneLoader.instance.LevelLoaded += OnLevelLoaded;
    }

    public IEnumerator SetAmbientClip(AudioSource audioSource, int idLocation, bool isTransition)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Audio source is null!");
        }
        else
        {
            if (audioSource.isPlaying)
                yield return VolumeDown(audioSource);

            if(idLocation != -1 && !_ambientInIdLocationAudioClip.ContainsKey(idLocation))
            {
                Debug.LogWarning("Audio ambient is empty!");
                goto exit;
            }

            if (isTransition && _audioClipInTransition != null)
                audioSource.clip = _audioClipInTransition;
            else if (!isTransition)
                audioSource.clip = _ambientInIdLocationAudioClip[idLocation].AudioClip;
            else
                goto exit;

            audioSource.Play();

            yield return VolumeUp(audioSource, !isTransition ? _ambientInIdLocationAudioClip[idLocation].MaxVolume : _maxVolumeTransition);
        }

    exit:;
    }

    public void TransitionIn()
    {
        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, -1, true));
    }

    public void TransitionOut()
    {
        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, SceneLoader.instance.CurrentSceneId, false));
    }

    private void OnLevelLoaded()
    {
        int currentLocationId = SceneLoader.instance.CurrentSceneId;
        bool isTransition = SceneLoader.instance.CheckCurrentSceneTransition;

        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, currentLocationId, isTransition));
    }

    private IEnumerator VolumeUp(AudioSource audioSource, float border)
    {
        while (audioSource.volume < 1 && audioSource.volume < border)
        {
            audioSource.volume += Time.unscaledDeltaTime * SpeedUp;

            if (audioSource.volume > border)
                audioSource.volume = border;

            yield return null;
        }
    }

    private IEnumerator VolumeDown(AudioSource audioSource)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.unscaledDeltaTime * SpeedDown;

            if (audioSource.volume < 0)
                audioSource.volume = 0;

            yield return null;
        }
    }

    #endregion

    #region RandomAudioClip

    private void InitRandomAudioClip()
    {
        foreach (var profile in _randomAudioClip)
            _audioProfileLocationId.Add(profile.LocationId, profile.RandomAudioClip);
    }

    public void ChangeStateSystemRandomSound(bool isActive)
    {
        _isActiveSystemRandomAudio = isActive;
    }

    public void PlayRandomAudioClip(int currentIdLocation)
    {
        if (!_audioProfileLocationId.ContainsKey(currentIdLocation))
        {
            Debug.LogWarning("Sound profile not found!");
            return;
        }

        List<CellAudioClip> audioClips = _audioProfileLocationId[currentIdLocation];
        int randomValue = Random.Range(0, audioClips.Count);

        if (_isChangePositionAudioSource)
            ChangePositionSource(_randomSoundSource);

        audioClips[randomValue].PlayAudioClipOneShot(_randomSoundSource);
    }

    private void UpdateFrame()
    {
        _currentLastTimeRandomAudio -= Time.deltaTime;

        if (_currentLastTimeRandomAudio <= 0)
        {
            _currentLastTimeRandomAudio = _timeBetweenRandomAudio;

            float randomValue = Random.value;

            if (randomValue <= _percentageOccurence)
            {
                int currentId = SceneLoader.instance.CurrentSceneId;

                if (SceneLoader.instance.CheckCurrentSceneTransition)
                    currentId++;

                PlayRandomAudioClip(currentId);
            }
        }
    }

    private void ChangePositionSource(AudioSource audioSource)
    {
        float angle = Random.value * Mathf.PI * 2f;

        Vector3 newPos = new(Mathf.Cos(angle) * _radiusInCircle, 0, Mathf.Sin(angle) * _radiusInCircle);

        Player player = Player.Instance;

        if (player == null)
            return;

        audioSource.transform.parent = player.transform;
        audioSource.transform.localPosition = newPos;
    }

    #endregion

    #region ScriptedAudioClip

    private void InitScriptedAudioClip()
    {
        _scriptedAudioClipInName = new();

        foreach (ProfileScriptedAudioClip clip in _scriptedAudios)
            _scriptedAudioClipInName.Add(clip.NameAudioClip, clip);
    }

    public void ResetScriptedAudioClip(string name)
    {
        _scriptedAudioSourse.Stop();
    }

    public void PlayScriptedSoundName(string name)
    {
        if (_scriptedAudioSourse == null)
            return;

        _scriptedAudioClipInName[name].Clip.PlayAudioClip(_scriptedAudioSourse);
    }

    #endregion
}

[System.Serializable]
public class AudioClipInLocation
{
    public AudioClip AudioClip;
    public float MaxVolume;
    public int IdLocation;
}

[System.Serializable]
public class ProfileRandomAudioClip
{
    public int LocationId;
    public List<CellAudioClip> RandomAudioClip;
}

[System.Serializable]
public class ProfileScriptedAudioClip
{
    public string NameAudioClip;
    public CellAudioClip Clip;
}