using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _len = 7535.646f;
    [Space]
    [SerializeField] private GameObject _rootObject;
    [SerializeField] private GameObject _additionalObject;

    private float _border;

    private void Start()
    {
        _border = _rootObject.transform.localPosition.y + _len;
    }

    private void Update()
    {
        _rootObject.transform.Translate(Vector3.up * _speed * Time.deltaTime);
        _additionalObject.transform.Translate(Vector3.up * _speed * Time.deltaTime);

        if (_rootObject.transform.localPosition.y >= _border)
            _rootObject.transform.localPosition = new Vector3(_rootObject.transform.localPosition.x, -1719.3f, _rootObject.transform.localPosition.z);

        if (_additionalObject.transform.localPosition.y >= _border)
            _additionalObject.transform.localPosition = new Vector3(_additionalObject.transform.localPosition.x, -1719.3f, _additionalObject.transform.localPosition.z);
    }
}
