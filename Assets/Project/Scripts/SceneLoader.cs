using Akila.FPSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    public String loadedScenes = "Transition_1";

    private List<string> sceneNames;
    private bool _isDone = true;

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        sceneNames = GetSceneNamesInBuild();
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
                Debug.Log("Scene found: " + sceneName);
            }
        }

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
        Debug.Log("Scene " + sceneIndex + " unloaded successfully.");
    }

    private IEnumerator LoadSceneByIndexAsync(int sceneIndex)
    {
        Scene originalScene = SceneManager.GetSceneByBuildIndex(sceneIndex - 1);

        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);

        if (loadedScene.isLoaded)
        {
            GameObject endLevelPoint = FindGameObjectInSceneByName(originalScene, "EndLevelPoint");
            GameObject startLevelPoint = FindGameObjectInSceneByName(loadedScene, "StartLevelPoint");
            GameObject levelContainer = FindGameObjectInSceneByName(loadedScene, "LevelContainer");

            if (endLevelPoint != null && startLevelPoint != null && levelContainer != null)
            {
                levelContainer.transform.eulerAngles = endLevelPoint.transform.eulerAngles;
                levelContainer.transform.position -= startLevelPoint.transform.position - endLevelPoint.transform.position;
                Debug.Log("Scene ID " + sceneIndex + " loaded successfully.");
            }
            else
            {
                Debug.LogError("Couldn't find necessary game objects in the loaded scene.");
            }
        }
    }

    private void GetCurrentSceneName()
    {
        string sceneName = loadedScenes;
        Debug.Log("Текущая сцена: " + sceneName);
    }

    public IEnumerator SceneRotationProcess()
    {
        GetCurrentSceneName();

        _isDone = false;
        int sceneIndex = sceneNames.IndexOf(loadedScenes);

        if (sceneIndex == -1)
        {
            Debug.LogError("Загруженная сцена не найдена в списке сцен.");
            yield break;
        }

        int previousTransitionIndex = -1;
        for (int i = sceneIndex - 1; i >= 0; i--)
        {
            if (sceneNames[i].StartsWith("Transition"))
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
        for (int i = sceneIndex + 1; i < sceneNames.Count; i++)
        {
            if (sceneNames[i].StartsWith("Transition"))
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

            loadedScenes = SceneManager.GetSceneByBuildIndex(nextTransitionIndex).name;
            SpawnManager.Instance.UpdateSpawnPoint(nextTransitionIndex);
        }
        else
        {
            Debug.LogWarning("Следующая сцена 'Transition' не найдена. Загружены сцены только до конца.");
        }

        _isDone = true;
    }
}
