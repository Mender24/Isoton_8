using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChageSceneByTrigger : MonoBehaviour
{
    [Header("��������� �����")]
    [SerializeField] private string sceneName = ""; // ��� ����� ��� ��������

    [Header("��������� ��������")]
    [SerializeField] private string targetTag = "Player"; // ��� �������-����������
    [SerializeField] private bool oneTimeUse = true; // ������� ����������� ������ ���� ���

    [Header("��������� ���������")]
    [SerializeField] private float fadeDuration = 1.0f; // ������������ ���������
    [SerializeField] private Color fadeColor = Color.black; // ���� ���������

    [Header("���������� (�������������)")]
    [SerializeField] private Image fadeImage; // UI Image ��� ���������

    private bool hasBeenUsed = false;
    private bool isFading = false;

    void Start()
    {
        // �������� ����� �����
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"�� ������ ��� ����� ��� {gameObject.name}!");
        }

        // ������� ������ ��� ��������� ���� �� �����
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
        // ������� ����� Canvas ���� ���
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // ����� ������� ����
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ������� Image ��� ���������
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(canvas.transform);

        fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.raycastTarget = false;

        // ����������� �� ���� �����
        RectTransform rt = fadeObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeObj.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // ��������� ��� �������, ��������� � �������
        if (other.CompareTag(targetTag))
        {
            // ���� ������� ����������� � ��� ����������� - �������
            if (oneTimeUse && hasBeenUsed)
                return;

            // ���� ��� ���� ��������� - �������
            if (isFading)
                return;

            // �������� ������� ����� �����
            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        isFading = true;

        // �������� ��� �������������� ���� �����������
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }

        // ���������� Image ��� ���������
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        }

        // ���� ����������
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

        // ��������� ���������
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }

        Debug.Log($"�������� �����: {sceneName}");

        // ��������� �����
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(sceneName);

        isFading = false;
    }

    void OnDrawGizmos()
    {
        // ������������ � ���������
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // ���������� ��� �����
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            $"Scene: {sceneName}\nFade: {fadeDuration}s", style);
#endif
    }

    // === ��������� ������ ��� ������� ���������� ===

    /// <summary>
    /// ������ ������� �������� �����
    /// </summary>
    public void StartFadeAndLoad()
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    /// <summary>
    /// ��������� ����� � ��������� ������
    /// </summary>
    public void LoadSceneWithFade(string newSceneName)
    {
        sceneName = newSceneName;
        StartFadeAndLoad();
    }

    /// <summary>
    /// ������������� ������� ����� � ����������
    /// </summary>
    public void ReloadSceneWithFade()
    {
        sceneName = SceneManager.GetActiveScene().name;
        StartFadeAndLoad();
    }

    /// <summary>
    /// ��������� ��������� ����� � ����������
    /// </summary>
    public void LoadNextSceneWithFade()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // ���� ��� ��������� ����� - ��������� ������
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    /// <summary>
    /// ���������� ���� ���������
    /// </summary>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
    }

    /// <summary>
    /// ���������� ������������ ���������
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// �������� ��������� ��������
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isFading = false;
    }

    /// <summary>
    /// ������� ���������� ������ ��� ����� �����
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
    /// ������� ���������� ������
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