using UnityEngine;
using UnityEngine.UI;

public class LoadSceneView : MonoBehaviour
{
    [SerializeField] private bool _isAsyncSceneLoadView = false;
    [SerializeField] private Image _image;
    [SerializeField] private float _speed = 2f;

    private void Update()
    {
        if (SceneLoader.instance.IsProgressUnloadingScenes)
        {
            if (!_image.enabled)
                _image.enabled = true;

            _image.transform.Rotate(0, 0, -_speed * Time.deltaTime);

            return;
        }
        else
        {
            if (_image.enabled)
                _image.enabled = false;
        }

        if (!_isAsyncSceneLoadView)
        {
            if (SceneLoader.instance.IsProgressLoadingScenes)
            {
                if (!_image.enabled)
                    _image.enabled = true;

                _image.transform.Rotate(0, 0, -_speed * Time.deltaTime);
            }
            else
            {
                if (_image.enabled)
                    _image.enabled = false;
            }
        }
        else
        {
            if (SceneLoader.instance.IsProgressAsyncLoadingScene)
            {
                if (!_image.enabled)
                    _image.enabled = true;

                _image.transform.Rotate(0, 0, -_speed * Time.deltaTime);
            }
            else
            {
                if (_image.enabled)
                    _image.enabled = false;
            }
        }
    }
}
