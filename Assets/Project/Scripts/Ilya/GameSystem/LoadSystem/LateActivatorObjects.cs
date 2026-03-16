using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateActivatorObjects : MonoBehaviour
{
    [SerializeField] private List<LateActiveObject> _lateActivators = new();

    public IEnumerator ActivateLateActiveObject(SpeedTypeProfile speedProfile = null)
    {
        foreach (var activate in _lateActivators)
        {
            yield return StartCoroutine(activate.StartActivate(speedProfile));
        }
    }
}
