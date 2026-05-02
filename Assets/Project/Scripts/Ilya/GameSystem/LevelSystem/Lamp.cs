using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField] private Material _offEmissionMaterial;
    [SerializeField] private bool _isAwakeOff = false;

    private MeshRenderer _meshRenderer;
    private Material _baseMaterial;
    private bool _isEmissionMaterial = false;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _baseMaterial = _meshRenderer.material;

        if (_isAwakeOff)
            OffEmission();
    }

    public void OffEmission()
    {
        _isEmissionMaterial = true;
        _meshRenderer.material = _offEmissionMaterial;
    }

    public void OnEmission()
    {
        _isEmissionMaterial = false;
        _meshRenderer.material = _baseMaterial;
    }

    public void SetNewBaseMaterial(Material material)
    {
        _baseMaterial = material;

        if (!_isEmissionMaterial)
            OnEmission();
    }
}
