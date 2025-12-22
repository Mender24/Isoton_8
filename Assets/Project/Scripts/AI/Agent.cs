using Akila.FPSFramework;
using UnityEngine;

[RequireComponent(typeof(AIShootingSystem))]
public class Agent : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private float _timeBetweenShoot = 0.1f;
    [SerializeField] private float _percentage = 0.25f;
    [SerializeField] private float _speedBullet;

    //Time
    public bool IsLineSight; 
    public Damageable Target;
    //-----

    private AIShootingSystem _aiShootingSystem;

    private float _time = 0;

    private void Start()
    {
        _aiShootingSystem = GetComponent<AIShootingSystem>();

        _aiShootingSystem.Init(_percentage, _speedBullet);
    }

    private void Update()
    {
        if(IsLineSight && _time >= _timeBetweenShoot)
        {
            _aiShootingSystem.Fire(Target);
            _time = 0;
        }

        _time += Time.deltaTime;
    }
}
