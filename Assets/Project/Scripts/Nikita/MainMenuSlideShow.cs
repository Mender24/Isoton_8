using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class MainMenuSlideShow : MonoBehaviour
{
    [Header("Настройки изображений")]
    [SerializeField] private List<Sprite> images = new List<Sprite>();

    [Header("Компоненты UI")]
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;

    [Header("Настройки перехода")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private bool startAutomatically = true;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        // Автоматически находим Image компоненты если не заданы
        if (currentImage == null || nextImage == null)
        {
            Image[] imageComponents = GetComponentsInChildren<Image>();
            if (imageComponents.Length >= 2)
            {
                if (currentImage == null) currentImage = imageComponents[0];
                if (nextImage == null) nextImage = imageComponents[1];
            }
            else if (imageComponents.Length == 1)
            {
                // Создаем второй Image если есть только один
                currentImage = imageComponents[0];
                GameObject nextImageObj = new GameObject("NextImage");
                nextImageObj.transform.SetParent(transform);
                nextImageObj.transform.localPosition = Vector3.zero;
                nextImageObj.transform.localScale = Vector3.one;
                nextImage = nextImageObj.AddComponent<Image>();
                nextImage.rectTransform.sizeDelta = currentImage.rectTransform.sizeDelta;
            }
            else
            {
                Debug.LogError("Нужно как минимум 2 компонента Image!");
                return;
            }
        }

        // Настраиваем начальное состояние
        if (images.Count > 0)
        {
            currentImage.sprite = images[0];
            currentImage.color = Color.white;
            nextImage.color = new Color(1, 1, 1, 0);

            if (startAutomatically && images.Count > 1)
            {
                StartCoroutine(ImageCycle());
            }
        }
        else
        {
            Debug.LogWarning("Список изображений пуст!");
        }
    }

    IEnumerator ImageCycle()
    {
        while (true)
        {
            // Ждем указанное время отображения
            yield return new WaitForSeconds(displayDuration);

            // Переходим к следующему изображению
            yield return StartCoroutine(TransitionToNextImage());
        }
    }

    IEnumerator TransitionToNextImage()
    {
        if (isTransitioning || images.Count <= 1) yield break;

        isTransitioning = true;

        // Устанавливаем следующее изображение
        int nextIndex = (currentIndex + 1) % images.Count;
        nextImage.sprite = images[nextIndex];
        nextImage.color = new Color(1, 1, 1, 0);

        // Плавный переход от currentImage к nextImage
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            // Уменьшаем прозрачность текущего изображения
            // и увеличиваем прозрачность следующего
            currentImage.color = new Color(1, 1, 1, 1 - progress);
            nextImage.color = new Color(1, 1, 1, progress);

            yield return null;
        }

        // Завершаем переход
        currentImage.sprite = images[nextIndex];
        currentImage.color = Color.white;
        nextImage.color = new Color(1, 1, 1, 0);

        currentIndex = nextIndex;
        isTransitioning = false;
    }

    // Метод для ручного переключения
    public void NextImage()
    {
        if (!isTransitioning && images.Count > 1)
        {
            StartCoroutine(TransitionToNextImage());
        }
    }

    // Метод для переключения на конкретное изображение
    public void GoToImage(int index)
    {
        if (!isTransitioning && index >= 0 && index < images.Count && index != currentIndex)
        {
            StartCoroutine(TransitionToSpecificImage(index));
        }
    }

    IEnumerator TransitionToSpecificImage(int targetIndex)
    {
        if (isTransitioning || images.Count <= 1) yield break;

        isTransitioning = true;

        // Устанавливаем целевое изображение
        nextImage.sprite = images[targetIndex];
        nextImage.color = new Color(1, 1, 1, 0);

        // Плавный переход
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            currentImage.color = new Color(1, 1, 1, 1 - progress);
            nextImage.color = new Color(1, 1, 1, progress);

            yield return null;
        }

        // Завершаем переход
        currentImage.sprite = images[targetIndex];
        currentImage.color = Color.white;
        nextImage.color = new Color(1, 1, 1, 0);

        currentIndex = targetIndex;
        isTransitioning = false;
    }

    // Методы управления
    public void StartSlideshow()
    {
        if (images.Count > 1 && !IsTransitioning())
        {
            StartCoroutine(ImageCycle());
        }
    }

    public void StopSlideshow()
    {
        StopAllCoroutines();
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    // Методы для изменения настроек
    public void SetDisplayDuration(float newDuration)
    {
        displayDuration = Mathf.Max(0.5f, newDuration);
    }

    public void SetTransitionDuration(float newDuration)
    {
        transitionDuration = Mathf.Max(0.1f, newDuration);
    }

    public void AddImage(Sprite newImage)
    {
        if (newImage != null)
        {
            images.Add(newImage);
        }
    }

    public void RemoveImage(int index)
    {
        if (index >= 0 && index < images.Count)
        {
            images.RemoveAt(index);
            if (currentIndex >= images.Count && images.Count > 0)
            {
                currentIndex = 0;
                currentImage.sprite = images[0];
            }
        }
    }
}