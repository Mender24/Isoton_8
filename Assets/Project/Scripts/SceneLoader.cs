using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [SerializeField] private bool _isDebug = false;
    [SerializeField] private List<string> _sceneNames = new();
    [SerializeField] private string _transitionName = "Transition";
    [SerializeField] private string _startScene;

    private List<string> sceneNames;
    private string _currentScene;
    private string _nextScene;
    private bool _isDone = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        sceneNames = GetSceneNamesInBuild();
    }

    public void LoadStartScene(string name)
    {
        if(!name.StartsWith(_transitionName))
            name = _startScene;

        _currentScene = name;

        SceneManager.LoadScene(name);

        int currentSceneInd = SerachIndexScene(name);

        _nextScene = SerachTransitionScene(currentSceneInd);

        if (_nextScene == "")
            Debug.LogError("Uncorrect scene queue");

        Debug.Log(_nextScene);

        //SpawnManager.Instance.UpdateSpawnPoint(currentSceneInd);

        StartCoroutine(SceneLoadNext(currentSceneInd));
    }

    public IEnumerator SceneRotationProcess()
    {
        _currentScene = _nextScene;

        GetCurrentSceneName();

        _isDone = false;
        int sceneIndex = _sceneNames.IndexOf(_currentScene);

        if (sceneIndex == -1)
        {
            Debug.LogError("Загруженная сцена не найдена в списке сцен.");
            yield break;
        }

        Debug.Log(sceneIndex);
        int previousTransitionIndex = -1;
        for (int i = sceneIndex - 1; i >= 0; i--)
        {
            if (_sceneNames[i].StartsWith(_transitionName) || _sceneNames[i] == "StartTunnel")
            {
                previousTransitionIndex = i;
                break;
            }
        }

        if (previousTransitionIndex != -1)
        {
            for (int i = sceneIndex - 1; i > previousTransitionIndex; i--)
                yield return StartCoroutine(UnloadSceneByIndexAsync(i));

            yield return StartCoroutine(UnloadSceneByIndexAsync(previousTransitionIndex));
        }

        int nextTransitionIndex = -1;
        for (int i = sceneIndex + 1; i < _sceneNames.Count; i++)
        {
            if (_sceneNames[i].StartsWith(_transitionName))
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
        Scene scene = SceneManager.GetSceneByBuildIndex(sceneInd);

        DoorControllerSceneChanger dc = scene.GetRootGameObjects()
            .SelectMany(t => t.GetComponentsInChildren<DoorControllerSceneChanger>(true))
            .First();

        if (!isOpenNext)
            dc.EnterNext();
        else
            dc.EnterLastLocation();
    }

    private string SerachTransitionScene(int currentInd)
    {
        for(int i = currentInd + 1; i < _sceneNames.Count; i++)
            if (_sceneNames[i].StartsWith(_transitionName))
                return _sceneNames[i];

        return "";
    }

    private IEnumerator SceneLoadNext(int currentSceneInd)
    {
        int nextScene = -1;
        for (int i = currentSceneInd + 1; i < _sceneNames.Count; i++)
        {
            if (_sceneNames[i].StartsWith(_transitionName))
            {
                nextScene = i;
                break;
            }
        }  

        if (nextScene != -1)
            for (int i = currentSceneInd + 1; i <= nextScene; i++)
                yield return SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);

        if (_currentScene.StartsWith(_transitionName))
            ActivateButton(currentSceneInd);

        if(_nextScene != "")
            ActivateButton(_nextScene, true);

        SpawnManager.Instance.UpdateSpawnPoint(currentSceneInd);
    }

    private int SerachIndexScene(string name)
    {
        for (int i = 0; i < _sceneNames.Count; i++)
            if (_sceneNames[i] == name)
                return i;

        return -1;
    }

    private List<string> GetSceneNamesInBuild()
    {
        List<string> names = new List<string>();
        var scenes = EditorBuildSettings.scenes;

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                names.Add(sceneName);

                if (_isDebug)
                    Debug.Log("Scene found: " + sceneName);
            }
        }

        if (_isDebug)
            Debug.Log("Total scenes found: " + names.Count);

        return names;
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
