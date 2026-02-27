using System.Collections;
using UnityEngine;

public class LateActiveObject : MonoBehaviour
{
    [SerializeField] private int _countObjectInFrame = 3;
    [SerializeField] private float _timeBetweenActive = 0.1f;

    public IEnumerator StartActivate()
    {
        int currentActive = 0;
        float time;

        foreach (Transform obj in transform)
        {
            currentActive++;
            obj.gameObject.SetActive(true);

            if(currentActive >= _countObjectInFrame)
            {
                currentActive = 0;

                time = _timeBetweenActive;
                while (time > 0f)
                {
                    time -= Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}
