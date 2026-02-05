using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChageSceneByTrigger : MonoBehaviour
{
    [Header("Настройки сцены")]
    [SerializeField] private string sceneName = ""; // Имя сцены для загрузки

    [Header("Настройки триггера")]
    [SerializeField] private string targetTag = "Player"; // Тэг объекта-активатора
    [SerializeField] private bool oneTimeUse = true; // Триггер срабатывает только один раз

    [Header("Настройки затухания")]
    [SerializeField] private float fadeDuration = 1.0f; // Длительность затухания
    [SerializeField] private Color fadeColor = Color.black; // Цвет затухания

    [Header("Компоненты (необязательно)")]
    [SerializeField] private Image fadeImage; // UI Image для затухания

    private bool hasBeenUsed = false;
    private bool isFading = false;

    void Start()
    {
        // Проверка имени сцены
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"Не задано имя сцены для {gameObject.name}!");
        }

        // Создаем объект для затухания если не задан
        if (fadeImage == null)
        {
            CreateFadeImage();
        }
        else
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    void CreateFadeImage()
    {
        // Создаем новый Canvas если нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Самый верхний слой
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Создаем Image для затухания
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(canvas.transform);

        fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.raycastTarget = false;

        // Растягиваем на весь экран
        RectTransform rt = fadeObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeObj.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем тег объекта, вошедшего в триггер
        if (other.CompareTag(targetTag))
        {
            // Если триггер одноразовый и уже использован - выходим
            if (oneTimeUse && hasBeenUsed)
                return;

            // Если уже идет затухание - выходим
            if (isFading)
                return;

            // Начинаем плавную смену сцены
            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        isFading = true;

        // Помечаем как использованный если одноразовый
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }

        // Активируем Image для затухания
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        }

        // Фаза затемнения
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }

            yield return null;
        }

        // Завершаем затухание
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }

        Debug.Log($"Загрузка сцены: {sceneName}");

        // Загружаем сцену
        SceneManager.LoadScene(sceneName);

        isFading = false;
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Показываем имя сцены
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            $"Scene: {sceneName}\nFade: {fadeDuration}s", style);
#endif
    }

    // === Публичные методы для ручного управления ===

    /// <summary>
    /// Начать плавную загрузку сцены
    /// </summary>
    public void StartFadeAndLoad()
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    /// <summary>
    /// Загрузить сцену с указанным именем
    /// </summary>
    public void LoadSceneWithFade(string newSceneName)
    {
        sceneName = newSceneName;
        StartFadeAndLoad();
    }

    /// <summary>
    /// Перезагрузить текущую сцену с затуханием
    /// </summary>
    public void ReloadSceneWithFade()
    {
        sceneName = SceneManager.GetActiveScene().name;
        StartFadeAndLoad();
    }

    /// <summary>
    /// Загрузить следующую сцену с затуханием
    /// </summary>
    public void LoadNextSceneWithFade()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Если это последняя сцена - загружаем первую
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    /// <summary>
    /// Установить цвет затухания
    /// </summary>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
    }

    /// <summary>
    /// Установить длительность затухания
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// Сбросить состояние триггера
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isFading = false;
    }

    /// <summary>
    /// Плавное затемнение экрана без смены сцены
    /// </summary>
    public IEnumerator FadeOut()
    {
        if (fadeImage == null)
            CreateFadeImage();

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
    }

    /// <summary>
    /// Плавное осветление экрана
    /// </summary>
    public IEnumerator FadeIn()
    {
        if (fadeImage == null)
            CreateFadeImage();

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.gameObject.SetActive(false);
    }
}