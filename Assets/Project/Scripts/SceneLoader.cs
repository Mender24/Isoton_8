using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [SerializeField] private bool _isUseSave = true;
    [Space]
    [SerializeField] private bool _isDebug = false;
    [SerializeField] private List<string> _sceneNames = new();
    [SerializeField] private string _transitionName = "Transition";
    [SerializeField] private string _endSceneName = "End";
    [SerializeField] private string _startScene;
    [Space]
    [SerializeField] private Player _player;

    private List<string> sceneNames;
    private Queue<int> _loadedScene = new();
    private string _currentScene;
    private string _nextScene;
    private bool _isDone = true;

    public Player Player => _player;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(SetPlayer);
        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(ResetAllEnemies);
    }

    public void ResetAllEnemies(string name)
    {
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            if (enemy.spawnType == EnemyAI.SpawnSource.Manually)
            {
                enemy.FullReset();
            }
            else
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    public void SetPlayer(string player)
    {
        Player[] players = GetComponentsInChildren<Player>();
        _player = players[players.Length - 1];
    }

    public void LoadStartScene(string name)
    {
        if (name == "")
        {
            string lastSaveScene = SaveManager.Instance.GetLastSceneName();

            if (lastSaveScene == "")
                name = _startScene;
            else
                name = lastSaveScene;
        }

        _currentScene = name;

        SceneManager.LoadScene(name);

        int currentSceneInd = SerachIndexScene(name);

        _loadedScene.Enqueue(currentSceneInd);

        _nextScene = SerachTransitionScene(currentSceneInd);

        if (_nextScene == "")
            Debug.LogError("Uncorrect scene queue");

        StartCoroutine(SceneLoadNext(currentSceneInd));
        StartCoroutine(MovePlayerToPointStart());
    }

    public void LoadMenu()
    {
        _player.gameObject.SetActive(false);
        _player.transform.position = transform.position;
        _loadedScene.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    public void SavePlayerScene()
    {
        if (_isUseSave)
        {
            SaveManager.Instance.SetLastSceneName(_currentScene);
            SpawnManager.Instance.SavePlayer(_player.GetComponent<Actor>());
            SaveManager.Instance.Save();
        }
    }

    public IEnumerator SceneRotationProcess()
    {
        _currentScene = _nextScene;

        SavePlayerScene();

        GetCurrentSceneName();

        _isDone = false;
        int sceneIndex = _sceneNames.IndexOf(_currentScene);

        if (sceneIndex == -1)
        {
            Debug.LogError("Загруженная сцена не найдена в списке сцен.");
            yield break;
        }

        //int previousTransitionIndex = -1;
        //for (int i = sceneIndex - 1; i >= 0; i--)
        //{
        //    if (_sceneNames[i].StartsWith(_transitionName) || _sceneNames[i] == "StartTunnel")
        //    {
        //        previousTransitionIndex = i;
        //        break;
        //    }
        //}

        //if (previousTransitionIndex != -1)
        //{
        //    for (int i = sceneIndex - 1; i > previousTransitionIndex; i--)
        //        yield return StartCoroutine(UnloadSceneByIndexAsync(i));

        //    yield return StartCoroutine(UnloadSceneByIndexAsync(previousTransitionIndex));
        //}
        while (_loadedScene.Count > 1)
            StartCoroutine(UnloadSceneByIndexAsync(_loadedScene.Dequeue()));

        int nextTransitionIndex = -1;
        for (int i = sceneIndex + 1; i < _sceneNames.Count; i++)
        {
            _loadedScene.Enqueue(i);

            if (_sceneNames[i].StartsWith(_transitionName) || _sceneNames[i].StartsWith(_endSceneName))
            {
                nextTransitionIndex = i;
                break;
            }
        }

        if (nextTransitionIndex != -1)
        {
            for (int i = sceneIndex + 1; i < nextTransitionIndex; i++)
                yield return StartCoroutine(LoadSceneByIndexAsync(i));

            yield return StartCoroutine(LoadSceneByIndexAsync(nextTransitionIndex));

            _nextScene = SceneManager.GetSceneByBuildIndex(nextTransitionIndex).name;

            ActivateButton(_nextScene, true);

            SpawnManager.Instance.UpdateSpawnPoint(nextTransitionIndex);
        }
        else
        {
            Debug.LogWarning($"Следующая сцена {_transitionName} не найдена. Загружены сцены только до конца.");
        }

        _isDone = true;
    }

    private void ActivateButton(string sceneName, bool isOpenNext = false)
    {
        ActivateButton(_sceneNames.IndexOf(sceneName), isOpenNext);
    }

    private void ActivateButton(int sceneInd, bool isOpenNext = false)
    {
        if (!_sceneNames[sceneInd].StartsWith(_transitionName))
            return; 

        Scene scene = SceneManager.GetSceneByBuildIndex(sceneInd);

        DoorControllerSceneChanger dc = scene.GetRootGameObjects()
            .SelectMany(t => t.GetComponentsInChildren<DoorControllerSceneChanger>(true))
            .FirstOrDefault();

        if (dc == null)
            return;

        if (!isOpenNext)
            dc.EnterNext();
        else
            dc.EnterLastLocation();
    }

    private string SerachTransitionScene(int currentInd)
    {
        for (int i = currentInd + 1; i < _sceneNames.Count; i++)
            if (_sceneNames[i].StartsWith(_transitionName) || _sceneNames[i].StartsWith(_endSceneName))
                return _sceneNames[i];

        return "";
    }

    private IEnumerator MovePlayerToPointStart()
    {
        while (true)
        {
            if(_isDone)
                break;

            yield return null;
        }

        SpawnManager.Instance.MovePlayerStartPositionAndOn(_player);
    }

    private IEnumerator SceneLoadNext(int currentSceneInd)
    {
        _isDone = false;

        int nextScene = -1;
        for (int i = currentSceneInd + 1; i < _sceneNames.Count; i++)
        {
            _loadedScene.Enqueue(i);

            if (_sceneNames[i].StartsWith(_transitionName) || _sceneNames[i].StartsWith(_endSceneName))
            {
                nextScene = i;
                break;
            }
        }

        if (nextScene != -1)
            for (int i = currentSceneInd + 1; i <= nextScene; i++)
                yield return SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);

        Scene scene = SceneManager.GetSceneByBuildIndex(currentSceneInd);

        while(!scene.isLoaded)
            yield return null;

        if (_currentScene.StartsWith(_transitionName))
            ActivateButton(currentSceneInd);

        if (_nextScene != "")
            ActivateButton(_nextScene, true);

        SpawnManager.Instance.UpdateSpawnPoint(currentSceneInd);

        _isDone = true;
    }

    private int SerachIndexScene(string name)
    {
        for (int i = 0; i < _sceneNames.Count; i++)
            if (_sceneNames[i] == name)
                return i;

        return -1;
    }

    private GameObject FindGameObjectInSceneByName(Scene scene, string name)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .FirstOrDefault(transform => transform.gameObject.name == name)?.gameObject;
    }

    private IEnumerator UnloadSceneByIndexAsync(int sceneIndex)
    {
        yield return SceneManager.UnloadSceneAsync(sceneIndex);

        if (_isDebug)
            Debug.Log("Scene " + sceneIndex + " unloaded successfully.");
    }

    private IEnumerator LoadSceneByIndexAsync(int sceneIndex)
    {
        Scene originalScene = SceneManager.GetSceneByBuildIndex(sceneIndex - 1);

        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);

        //if (loadedScene.isLoaded)
        //{
        //    GameObject endLevelPoint = FindGameObjectInSceneByName(originalScene, "EndLevelPoint");
        //    GameObject startLevelPoint = FindGameObjectInSceneByName(loadedScene, "StartLevelPoint");
        //    GameObject levelContainer = FindGameObjectInSceneByName(loadedScene, "LevelContainer");

        //    if (endLevelPoint != null && startLevelPoint != null && levelContainer != null)
        //    {
        //        levelContainer.transform.eulerAngles = endLevelPoint.transform.eulerAngles;
        //        levelContainer.transform.position -= startLevelPoint.transform.position - endLevelPoint.transform.position;

        //        if (_isDebug)
        //            Debug.Log("Scene ID " + sceneIndex + " loaded successfully.");
        //    }
        //    else
        //    {
        //        Debug.LogError("Couldn't find necessary game objects in the loaded scene.");
        //    }
        //}
    }

    private void GetCurrentSceneName()
    {
        string sceneName = _currentScene;

        if (_isDebug)
            Debug.Log("Текущая сцена: " + sceneName);
    }
}
