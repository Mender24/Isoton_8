using UnityEngine;

public class EnemyEffectController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _fireEffect;

    private RangedCombatModule _rangedCombatModule;

    private void OnDestroy()
    {
        if(_rangedCombatModule != null )
            _rangedCombatModule.OnFire -= OnFire;
    }

    private void Awake()
    {
        _rangedCombatModule = GetComponent<RangedCombatModule>();

        _rangedCombatModule.OnFire += OnFire;
    }

    private void OnFire()
    {
        _fireEffect.Clear();
        _fireEffect.Play();
    }
}
