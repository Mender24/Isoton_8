using UnityEngine;

public class DumbEnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int _clipNumber = 0;
    [SerializeField] private string _parameterName = "ClipNumber";
    [SerializeField] private bool _randomizeClips = false;
    [SerializeField] private float _randomizeCycle = 3f;
    [SerializeField] private int _amountOfClips = 8;
    private float _time = 0f;
    
    void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        _animator.SetInteger(_parameterName, _clipNumber);
    }

    void Update()
    {
        if (_randomizeClips)
        {
            _time += Time.deltaTime;
            if (_time >= _randomizeCycle)
            {
                _time = 0f;
                _clipNumber = Random.Range(0, _amountOfClips);
                _animator.SetInteger(_parameterName, _clipNumber);
            }
        }
    }
}
