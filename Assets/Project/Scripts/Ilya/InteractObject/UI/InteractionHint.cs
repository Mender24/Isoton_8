using Akila.FPSFramework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class InteractionHint : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _radiusHint = 4f;
    [SerializeField] private Image _circleImage;
    [SerializeField] private TMP_Text _textInteract;
    [SerializeField] private float _speedColorUp = 1f;
    [SerializeField] private float _speedColorDown = 1f;
    [Space]
    [SerializeField] private Image _addCircleImage;
    [SerializeField] private int _maxAlpha = 50;
    [SerializeField] protected float _speedUp = 3f;
    [SerializeField] protected float _speedDown = 1f;

    private SphereCollider _sphereCollider;
    private IInteractable _interactable;

    private bool _isRange = false;
    private bool _isEnable = false;
    private bool _isCurrentObject = false;

    private bool _isEnabledEffect = false;

    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();

        _sphereCollider.radius = _radiusHint;
        _sphereCollider.isTrigger = true;
        _interactable = transform.parent.GetComponent<IInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _))
            _isRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _))
            _isRange = false;
    }

    public void LateUpdate()
    {
        if (!_isRange && _circleImage.color.a == 0)
        {
            _addCircleImage.color = OddAlpha(_addCircleImage.color, _speedDown);
            return;
        }

        if (!Player.Instance.InteractionsManager.IsDestroy 
            && transform.parent != null 
            && Player.Instance.InteractionsManager.CurrentInteractable != null
            && Player.Instance.InteractionsManager.CurrentInteractable.isInstant
            && Player.Instance.InteractionsManager.CurrentInteractable.transform.name == transform.parent.gameObject.name)
            _isCurrentObject = true;
        else
            _isCurrentObject = false;

        if (_isRange && Player.Instance != null)
        {
            Vector3 dir = Player.Instance.transform.position - _canvas.transform.position;

            dir.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(-dir);
            _canvas.transform.rotation = targetRotation;
        }

        if (_isRange && !_isCurrentObject && _interactable.isInstant)
        {
            if(!_isEnabledEffect)
                StartCoroutine(ChangeAddCercleEffect());

            _circleImage.color = AddAlpha(_circleImage.color, _speedColorUp);
            _textInteract.color = OddAlpha(_textInteract.color, _speedColorDown);
        }
        else
        {
            if (!_isRange)
            {
                StopAllCoroutines();
                _isEnabledEffect = false;
                _addCircleImage.color = OddAlpha(_addCircleImage.color, _speedDown);
            }

            _circleImage.color = OddAlpha(_circleImage.color, _speedColorDown);

            if (_isRange && _interactable.isInstant && _isCurrentObject)
                _textInteract.color = AddAlpha(_textInteract.color, _speedColorDown);
            else
                _textInteract.color = OddAlpha(_textInteract.color, _speedColorDown);
        }
    }

    private IEnumerator ChangeAddCercleEffect()
    {
        _isEnabledEffect = true;

        while (_addCircleImage.color.a < (float)_maxAlpha / 100)
        {
            _addCircleImage.color = AddAlpha(_addCircleImage.color, _speedUp, (float)_maxAlpha / 100);
            yield return null;
        }

        yield return null;

        while (_addCircleImage.color.a != 0)
        {
            _addCircleImage.color = OddAlpha(_addCircleImage.color, _speedDown);
            yield return null;
        }
    }

    public void WriteDebug()
    {
        Debug.Log("Interacted");
    }

    private Color AddAlpha(Color color, float speed, float maxAlpha = 1)
    {
        color.a += speed * Time.deltaTime;

        if (color.a > maxAlpha)
            color.a = maxAlpha;

        return color;
    }

    private Color OddAlpha(Color color, float speed, float minAlpha = 0)
    {
        color.a -= speed * Time.deltaTime;

        if (color.a < minAlpha)
            color.a = minAlpha;

        return color;
    }
}
