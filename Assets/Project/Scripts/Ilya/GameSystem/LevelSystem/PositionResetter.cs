using Akila.FPSFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionResetter : MonoBehaviour
{
    [SerializeField] private List<GameObject> _points = new();

    private bool _isSubscribed = false;
    private int _currentIndexPoint = 0;

    private float _timeWaitBeforeNextSubscribe = 3f;

    private void OnDestroy()
    {
        Unsubscribe();
        _isSubscribed = false;
    }

    private void OnEnable()
    {
        Subscribe();

        foreach (Transform t in transform)
            _points.Add(t.gameObject);
    }

    private void Update()
    {
        if (!_isSubscribed)
            Subscribe();
    }

    public void ChangeCurrentPoint(int id)
    {
        _currentIndexPoint = id;
    }

    private void Subscribe()
    {
        if (Player.Instance != null)
        {
            Player.Instance.Actor.Damageable.OnDeath.AddListener(OnDeath);
            Player.Instance.ResettingPosition += OnResettingPosition;
            _isSubscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (Player.Instance != null)
        {
            Player.Instance.Actor.Damageable.OnDeath.RemoveListener(OnDeath);
            Player.Instance.ResettingPosition -= OnResettingPosition;
        }
    }

    private void OnDeath()
    {
        if (Player.Instance != null)
        {
            Unsubscribe();

            StartCoroutine(WaitChangeNextSubscribe());
        }
    }

    private IEnumerator WaitChangeNextSubscribe()
    {
        yield return new WaitForSeconds(_timeWaitBeforeNextSubscribe);

        _isSubscribed = false;
    }

    private void OnResettingPosition()
    {
        if (Player.Instance == null || _currentIndexPoint == -1)
            return;

        Player.Instance.transform.position = _points[_currentIndexPoint].transform.position;
    }
}
