using System.Collections;
using UnityEngine;

public class Reactor : MonoBehaviour
{
    [SerializeField] private float _secondDestroy = 1f;

    private Battery[] _batterys;

    private int _health;

    private void OnDisable()
    {
        for (int i = 0; i < _batterys.Length; i++)
            _batterys[i].OnDeath.RemoveListener(OnDeath);
    }

    private void Start()
    {
        _batterys = transform.parent.GetComponentsInChildren<Battery>();

        _health = _batterys.Length;

        for (int i = 0; i < _batterys.Length; i++)
            _batterys[i].OnDeath.AddListener(OnDeath);
    }

    public void OnDeath()
    {
        _health -= 1;

        if(_health <= 0)
            StartCoroutine(Death());
    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(_secondDestroy);

        gameObject.SetActive(false);
    }
}
