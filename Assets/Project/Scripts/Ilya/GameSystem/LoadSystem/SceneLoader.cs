using Akila.FPSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.HDROutputUtils;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [Space]
    [SerializeField] private bool _isUseRandomSystemSound = true;
    [SerializeField] private bool _isUseSave = true;
    [Space]
    [SerializeField] private bool _isDebug = false;
    [SerializeField] private float _timeWaitNextLoad = 2f;
    [SerializeField] private List<string> _sceneNames = new();
    [SerializeField] private string _transitionName = "Transition";
    [SerializeField] private string _endSceneName = "End";
    [SerializeField] private string _startScene;
    [Space]
    [SerializeField] private Player _player;

    private Queue<int> _loadedScene = new();
    private string _currentScene;
    private bool _isFirstLoad = false;
    private bool _isMovePostLoadScene = true;

    private DoorControllerSceneChanger _nextLocationDC;

    public Player Player => _player;
    public string CurrentSceneName => _sceneNames[_currentSceneIndex];
    public int CurrentSceneId => _currentSceneIndex;
    public string NextScene => _sceneNames[_nextSceneIndex];
    public bool CheckCurrentSceneTransition => _sceneNames[_currentSceneIndex].Contains(_transitionName);

    public bool IsLoad { get; private set; }
    public bool IsInitPlayer { get; private set; }

    public event UnityAction LevelLoaded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            IsLoad = false;
            IsInitPlayer = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(RespawnPlayer);
    }

    #region LoadSystem

    private int _currentSceneIndex = 0;
    private int _nextSceneIndex = -1;

    public event UnityAction SceneLoadingComplete;

    public void LoadScenes(bool isFirstSceneLoad = false, string forceLoad = "", bool isUseSave = false)
    {
        if (isFirstSceneLoad)
            _currentScene = forceLoad != "" ? forceLoad : (isUseSave ? SaveManager.GetLastSceneName() : _startScene);

        if (isFirstSceneLoad && !isUseSave)
            SaveManager.DeleteSave(_isDebug);

        if (isFirstSceneLoad)               // search startIndex
        {
            SearchAllIndex(_currentScene);

            if (_isDebug)
                Debug.Log("Loading scene: " + _currentScene);
        }
        else
        {
            _currentSceneIndex = _nextSceneIndex;

            SearchAllIndex(_currentSceneIndex);
        }

        IsInitPlayer = false;

        if (isUseSave) // use save
            StartCoroutine(SaveDataPlayer());

        StartLoadScenes(isFirstSceneLoad); // main load system
    }

    private void SearchAllIndex(string startScene)
    {
        for (int i = 1; i < _sceneNames.Count; i++)
        {
            if (startScene == _sceneNames[i])
            {
                _currentSceneIndex = i;
                break;
            }
        }

        SearchAllIndex(_currentSceneIndex);
    }

    private void SearchAllIndex(int startSceneIndex)
    {
        _nextSceneIndex = -1;

        for (int i = startSceneIndex + 1; i < _sceneNames.Count; i++)
        {
            if (_sceneNames[i].Contains(_endSceneName) || _sceneNames[i].Contains(_transitionName))
            {
                _nextSceneIndex = i;
                break;
            }
        }

        if (_isDebug)
            Debug.Log("Search current index scene: " + _currentSceneIndex + " next index scene: " + _nextSceneIndex);
    }

    private bool CheckTransitionScene(int sceneIndex)
    {
        if(sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
            return false;

        return _sceneNames[sceneIndex].Contains(_transitionName);
    }

    private void StartLoadScenes(bool isFirstSceneLoad)
    {
        if (_currentSceneIndex == -1)
        {
            Debug.LogWarning("Start scene index not found");
            return;
        }

        IsLoad = true;

        int startSceneIndex = _currentSceneIndex;

        if (isFirstSceneLoad)
            HardLoadScene(startSceneIndex++);
        else
            startSceneIndex++;

        StartCoroutine(ProcessLoadScenes(startSceneIndex, isFirstSceneLoad));
    }

    private IEnumerator ProcessLoadScenes(int startSceneIndex, bool isFirstSceneLoad)
    {
        yield return StartCoroutine(UnloadScenesAsync());

        if (_isDebug)
            Debug.Log("Unload scenes complete");

        yield return StartCoroutine(LoadScenesAsync(startSceneIndex, _nextSceneIndex - startSceneIndex + 1, isFirstSceneLoad));

        if (_isDebug)
            Debug.Log("Scenes load complete");

        LevelLoaded?.Invoke();
    }

    private void HardLoadScene(int index)
    {
        _loadedScene.Clear();
        _loadedScene.Enqueue(index); // update loaded list

        SceneManager.LoadScene(index);
    }

    private Stack<LateActiveObject> _lateActives = new();

    private IEnumerator LoadScenesAsync(int startIndex, int count, bool isFirstSceneLoad)
    {
        if (count <= 0)
            count = 1;

        if (startIndex >= _sceneNames.Count)
            Debug.LogWarning("StartIndex in out range!");

        if (startIndex + count >= _sceneNames.Count)
        {
            count = _sceneNames.Count - startIndex;
            Debug.LogWarning("Count in out range! Correction count: " + count);
        }

        if (_isDebug)
            Debug.Log($"Start loading scenes. StartIndex: {startIndex} Count: {count}");

        for (int i = startIndex; i < startIndex + count; i++)
        {
            if (_isDebug)
                Debug.Log("Loading scene index: " + i);

            _loadedScene.Enqueue(i); // update loaded list

            AsyncOperation operation = SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);

            while(operation.progress < 0.9)
                yield return null;

            operation.allowSceneActivation = true;

            AddLateActiveObject(i);
        }

        yield return StartCoroutine(StartLateActive());

        IsLoad = false;

        if (_isDebug)
            Debug.Log("Loading scene complete");

        InitPostLoadScene(isFirstSceneLoad);

        SceneLoadingComplete?.Invoke();
    }

    private void AddLateActiveObject(int index)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        LateActiveObject late = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LateActiveObject>()).FirstOrDefault();

        if (late != null && !_sceneNames[index].Contains(_transitionName))
        {
            Debug.Log(late.name);
            _lateActives.Push(late);
        }
    }

    private IEnumerator StartLateActive()
    {
        if(_lateActives.Count > 0)
        {
            Debug.Log(_lateActives.Count);

            while (_lateActives.Count > 1)
            {
                LateActiveObject lateActive = _lateActives.Pop();
                StartCoroutine(lateActive.StartActivate());

                if (_lateActives.Count > 1)
                    yield return new WaitForSeconds(_timeWaitNextLoad);
            }

            yield return StartCoroutine(_lateActives.Pop().StartActivate());

            Debug.Log("EndAll");
        }
    }

    private IEnumerator UnloadScenesAsync()
    {
        while(_loadedScene.Count > 1)
        {
            int unloadSceneIndex = _loadedScene.Dequeue();

            if(_isDebug)
                Debug.Log("Unload scene index: " + unloadSceneIndex);

            yield return SceneManager.UnloadSceneAsync(unloadSceneIndex);
        }
    }

    private void InitPostLoadScene(bool isFirstSceneLoad)
    {
        if (!_isFirstLoad) //Use save and create weapons player
        {
            _isFirstLoad = true;
            SpawnManager.Instance.LoadPlayerWeapon(_player.Inventory);
        }

        SpawnManager.Instance.UpdateSpawnPoint(_currentSceneIndex);

        SoundManager.Instance.ChangeStateSystemRandomSound(_isUseRandomSystemSound);

        if (isFirstSceneLoad && _isMovePostLoadScene)
            SpawnManager.Instance.MovePlayerStartPositionAndOn(_player);

        if (CheckTransitionScene(_currentSceneIndex))
            OpenExitDoorTransition(FindDoorControolerInScene(_currentSceneIndex));

        if (CheckTransitionScene(_nextSceneIndex))
            OpenEnterDoorTransition(FindDoorControolerInScene(_nextSceneIndex));

        IsInitPlayer = true;
        _isMovePostLoadScene = true;

        if (_isDebug)
            Debug.Log("Init complete");
    }

    private IEnumerator SaveDataPlayer()
    {
        while (!IsInitPlayer)
            yield return null;

        SaveManager.SetLastSceneName(_sceneNames[_currentSceneIndex], _isDebug);
        SaveManager.SaveWeaponPlayer(_player.Actor, _isDebug);
        SaveManager.Save();

        if (_isDebug)
            Debug.Log("Save data");
    }

    private void OpenEnterDoorTransition(DoorControllerSceneChanger doorController)
    {
        doorController.EnterOpenDoor();
    }

    private void OpenExitDoorTransition(DoorControllerSceneChanger doorController)
    {
        doorController.EnterExitDoor();
    }

    private DoorControllerSceneChanger FindDoorControolerInScene(int sceneIndex)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);

        return scene.GetRootGameObjects()
            .SelectMany(t => t.GetComponentsInChildren<DoorControllerSceneChanger>(true))
            .FirstOrDefault();
    }

    #endregion

    #region RespawnPlayer

    public void RespawnPlayer(string player)
    {
        Player[] players = GetComponentsInChildren<Player>();
        _player = players[players.Length - 1];
        GameManager.instance.Init(_player);
        _isFirstLoad = false;
        _isMovePostLoadScene = false;

        LoadScenes(true, "", true);
    }

    #endregion

    #region AdditionalFunction

    public DoorControllerSceneChanger GetDoorControllerNextTransition()
    {
        if (!_sceneNames[_nextSceneIndex].Contains(_transitionName))
            return null;

        return FindDoorControolerInScene(_nextSceneIndex);
    }

    #endregion
}
