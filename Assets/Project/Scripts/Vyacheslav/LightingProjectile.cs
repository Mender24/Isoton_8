using UnityEngine;

public class LightingProjectile : AiProjectile
{
    [SerializeField] private float _lifeTime = 5f;
    public float LifeTime => _lifeTime;
}
