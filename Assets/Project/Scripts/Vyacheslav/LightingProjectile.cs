using UnityEngine;

public class LightingProjectile : AiProjectile
{
    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _period = 0.5f;
    private float _nextAttackTime; 

    public override void Setup(Vector3 direction, float lifeTime, float speed)
    {
        base.Setup(direction, lifeTime, speed);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        TryDoDamage();
    }

    private void TryDoDamage()
    {
        if (_nextAttackTime > Time.time)
        {
            return;
        }
        _nextAttackTime = Time.time + _period;
        var enemies = EnemyCounter.Instance.GetEnemyBySphere(_radius, transform.position);
        foreach (var enemy in enemies)
        {
            enemy.Damage(_damage, null);
        }
    } 
}

