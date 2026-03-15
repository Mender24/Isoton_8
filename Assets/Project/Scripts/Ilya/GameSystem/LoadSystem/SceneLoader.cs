using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [Space]
    [SerializeField] private bool _isUseRandomSystemSound = true;
    [SerializeField] private bool _isUseSave = true;
    [Space]
    [Header("LateLoadSystem")]
    [SerializeField] private bool _isUseFullLate = true;
    [SerializeField] private float _timeWaitNextLoad = 2f;
    [Space]
    [SerializeField] private bool _isDebug = false;
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
    public int GetIndexNotTransition
    {
        get
        {
            if (CheckCurrentSceneTransition)
                return _currentSceneIndex + 1;

            return _currentSceneIndex;
        }
    }

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

    private IEnumerator Start()
    {
        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(RespawnPlayer);

        yield return new WaitForSeconds(0.2f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #region LoadSystem

    private int _currentSceneIndex = 0;
    private int _nextSceneIndex = -1;

    public event UnityAction SceneLoadingComplete;

    public void LoadMainMenu()
    {
        Player.Instance.gameObject.SetActive(false);
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }

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

        _nextSceneIndex = SearchAllIndex(_currentSceneIndex);
    }

    private int SearchAllIndex(int startSceneIndex)
    {
        int nextSceneIndex = -1;

        for (int i = startSceneIndex + 1; i < _sceneNames.Count; i++)
        {
            if (_sceneNames[i].Contains(_endSceneName) || _sceneNames[i].Contains(_transitionName))
            {
                nextSceneIndex = i;
                break;
            }
        }

        if (_isDebug)
            Debug.Log("Search current index scene: " + _currentSceneIndex + " next index scene: " + nextSceneIndex);

        return nextSceneIndex;
    }

    private bool CheckTransitionScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
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
        if(_loadedScene.Count > 1)
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

    private IEnumerator LoadScenesAsync(int startIndex, int count, bool isFirstSceneLoad, bool isLateLoadScene = false)
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

            while (operation.progress < 0.9)
                yield return null;

            operation.allowSceneActivation = true;

            while (!operation.isDone)
                yield return null;

            if (isLateLoadScene)
                AddLateActiveObject(i);
        }

        if (isLateLoadScene)
            yield return StartCoroutine(StartLateActive());

        IsLoad = false;

        if (_isDebug)
            Debug.Log("Loading scene complete");

        if (!isLateLoadScene)
            InitPostLoadScene(isFirstSceneLoad);

        SceneLoadingComplete?.Invoke();
    }

    private void AddLateActiveObject(int index)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        LateActiveObject late = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LateActiveObject>()).FirstOrDefault();

        if (late != null && !_sceneNames[index].Contains(_transitionName))
            _lateActives.Push(late);
    }

    private IEnumerator StartLateActive()
    {
        if (_lateActives.Count > 0)
        {
            if (_isDebug)
                Debug.Log("Late Active scene: " + _lateActives.Count);

            while (_lateActives.Count > 1)
            {
                LateActiveObject lateActive = _lateActives.Pop();

                if (_isUseFullLate)
                {
                    yield return StartCoroutine(lateActive.StartActivate());
                    continue;
                }

                StartCoroutine(lateActive.StartActivate());

                if (_lateActives.Count > 1)
                    yield return new WaitForSeconds(_timeWaitNextLoad);
            }

            yield return StartCoroutine(_lateActives.Pop().StartActivate());

            if (_isDebug)
                Debug.Log("End LateLoad");
        }
    }

    private IEnumerator UnloadScenesAsync(int count = -1)
    {
        if (count == -1)
            count = _loadedScene.Count;

        for(int i = 0; i < count; i++)
        {
            int unloadSceneIndex = _loadedScene.Dequeue();

            if (_isDebug)
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

        Player.Instance.gameObject.SetActive(true);

        LockCursor();

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

    #region LateLoadSystem

    private int _currentEndTransition = -1;
    private int _nextEndScene = -1;
    private int _currentCountLoadedScene;

    private bool _isProgressLoadingScenes = false;
    private bool _isProgressUnloadingScenes = false;
    private bool _isScenesLoaded = false;
    private SpeedType _speedType;

    public bool IsProgressLoadingScenes => _isProgressLoadingScenes;
    public bool IsProgressUnloadingScenes => _isProgressUnloadingScenes;
    public bool IsScenesLoaded => _isScenesLoaded;
    public SpeedType SpeedType => _speedType;

    public void LateLoadScene()
    {
        _currentCountLoadedScene = _loadedScene.Count;
        _currentEndTransition = _nextSceneIndex;

        if (_currentEndTransition == -1)
        {
            Debug.LogError("currentEndTransition is null!");
            return;
        }

        _nextEndScene = SearchAllIndex(_currentEndTransition);

        if (_nextEndScene == -1)
        {
            Debug.LogError("Not found end scene!");
            return;
        }

        IsInitPlayer = false;

        if (_isDebug)
        {
            Debug.Log("Start LateLoadScene");
            Debug.Log("_currentEndTransition: " + _currentEndTransition + " _nextEndScene: " + _nextEndScene);
            Debug.Log("Count loaded scene: " + _currentCountLoadedScene);
        }

        StartCoroutine(StartLateLoadScene());
    }

    public void FinishLateLoadScene()
    {
        _currentSceneIndex = _currentEndTransition;
        _nextSceneIndex = _nextEndScene;

        InitPostLoadScene(false);

        StartCoroutine(StartLateUnloadScenes());
    }

    private IEnumerator StartLateLoadScene()
    {
        _isProgressLoadingScenes = true;

        _speedType = SpeedType.Slowly;

        yield return StartCoroutine(LoadScenesAsync(_currentEndTransition + 1, _nextEndScene - _currentEndTransition, false, true));

        if (_isDebug)
            Debug.Log("Finish late loaded scene");

        _isProgressLoadingScenes = false;
        _isScenesLoaded = true;
    }

    private IEnumerator StartLateUnloadScenes()
    {
        _isProgressUnloadingScenes = true;

        if (_isDebug)
            Debug.Log("Start late unloaded scene");

        yield return StartCoroutine(UnloadScenesAsync(_currentCountLoadedScene - 1));

        if (_isDebug)
            Debug.Log("Finish late unloaded scene");

        _isProgressUnloadingScenes = false;
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

    public void PriorityUp()
    {
        if (_speedType == SpeedType.VeryFast)
            return;

        _speedType = _speedType++;
    }

    public DoorControllerSceneChanger GetDoorControllerNextTransition()
    {
        if (!_sceneNames[_nextSceneIndex].Contains(_transitionName))
            return null;

        return FindDoorControolerInScene(_nextSceneIndex);
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #endregion
}

public enum SpeedType
{
    Slowly,
    Fast,
    VeryFast,
}