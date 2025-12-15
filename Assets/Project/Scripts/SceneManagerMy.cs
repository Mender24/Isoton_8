using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEditor;


namespace Akila.FPSFramework
{
    public class SceneManagerMy : MonoBehaviour
    {
        private GameObject endLevelPoint;
        private GameObject startLevelPoint;
        private GameObject levelContainer;
        public String loadedScenes = "Transition_1";

        private List<string> sceneNames;
        private int _sceneIndex;
        private bool _isDone = true;
        private CharacterController _playerCharacter;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            sceneNames = GetSceneNamesInBuild();
            GetPlayerCharacter();
        }

        void Update()
        {

        }

        public void SetLoadedScenes(string loadedSceneName)
        {
            loadedScenes = loadedSceneName;
        }

        private void GetPlayerCharacter()
        {
            _playerCharacter = FindFirstObjectByType<CharacterController>();
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
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneIndex);
            while (!asyncUnload.isDone)
            {
                Debug.Log("Unloading scene: " + sceneIndex + " - Progress: " + (asyncUnload.progress * 100) + "%");
                yield return new WaitForSeconds(2f);
            }

            Debug.Log("Scene " + sceneIndex + " unloaded successfully.");
        }

        private IEnumerator LoadSceneByIndexAsync(int sceneIndex)
        {
            Scene originalScene = SceneManager.GetSceneByBuildIndex(sceneIndex - 1);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false;
            while (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Loading scene: ID " + sceneIndex + " - Progress: " + (asyncLoad.progress * 100) + "%");
                yield return null;
            }
            asyncLoad.allowSceneActivation = true;
            yield return new WaitForSeconds(0.5f);
            endLevelPoint = FindGameObjectInSceneByName(originalScene, "EndLevelPoint");
            Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (loadedScene.isLoaded)
            {
                startLevelPoint = FindGameObjectInSceneByName(loadedScene, "StartLevelPoint");
                levelContainer = FindGameObjectInSceneByName(loadedScene, "LevelContainer");
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

        private void GetCurendSceneName()
        {
            string sceneName = loadedScenes;
            Debug.Log("Текущая сцена: " + sceneName);
        }

        public IEnumerator SceneRotationProcess()
        {
            GetCurendSceneName();
            _isDone = false;
            _sceneIndex = sceneNames.IndexOf(loadedScenes);
            if (_sceneIndex == -1)
            {
                Debug.LogError("Загруженная сцена не найдена в списке сцен.");
                yield break;
            }

            int previousTransitionIndex = -1;
            for (int i = _sceneIndex - 1; i >= 0; i--)
            {
                if (sceneNames[i].StartsWith("Transition"))
                {
                    previousTransitionIndex = i;
                    break;
                }
            }

            if (previousTransitionIndex != -1)
            {
                for (int i = _sceneIndex - 1; i > previousTransitionIndex; i--)
                {
                    yield return StartCoroutine(UnloadSceneByIndexAsync(i));
                }
                yield return StartCoroutine(UnloadSceneByIndexAsync(previousTransitionIndex));
            }

            int nextTransitionIndex = -1;
            for (int i = _sceneIndex + 1; i < sceneNames.Count; i++)
            {
                if (sceneNames[i].StartsWith("Transition"))
                {
                    nextTransitionIndex = i;
                    break;
                }
            }

            if (nextTransitionIndex != -1)
            {
                for (int i = _sceneIndex + 1; i < nextTransitionIndex; i++)
                {
                    yield return StartCoroutine(LoadSceneByIndexAsync(i));
                }
                yield return StartCoroutine(LoadSceneByIndexAsync(nextTransitionIndex));
            }
            else
            {
                Debug.LogWarning("Следующая сцена 'Transition' не найдена. Загружены сцены только до конца.");
            }
            _isDone = true;
        }



    }
}
