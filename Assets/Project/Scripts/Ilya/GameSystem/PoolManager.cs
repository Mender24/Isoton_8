using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] public List<PrefabInfo> _allPrefab = new();

    private static PoolManager _instance;

    private Dictionary<Type, Stack<MonoBehaviour>> _objectPools;

    public static PoolManager Instance
    {
        get 
        {
            if (_instance == null)
                Debug.LogError("PoolManager is NULL!");

            return _instance; 
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitPoolManager();
    }

    public T GetObject<T>() where T : MonoBehaviour
    {
        Stack<MonoBehaviour> stackObj = _objectPools[typeof(T)];

        if (stackObj.Count != 0)
            return (T)stackObj.Pop();
        else
            return CreateObject<T>();
    }

    public void SetObject(MonoBehaviour obj)
    {
        if(_objectPools.TryGetValue(obj.GetType(), out var stack))
        {
            obj.gameObject.SetActive(false);
            obj.transform.position = transform.position;
            stack.Push(obj);
        }
        else
        {
            Debug.LogError("Empty Stack<MonoBehaviour> in PoolManager!");
        }
    }

    private void InitPoolManager()
    {
        _objectPools = new Dictionary<Type, Stack<MonoBehaviour>>(_allPrefab.Count);

        foreach (var prefabInfo in _allPrefab)
        {
            if (prefabInfo.Prefab == null)
                Debug.LogError("Empty prefab PoolManager!");

            Stack<MonoBehaviour> newQueue = new();
            _objectPools[prefabInfo.Prefab.GetType()] = newQueue;

            for (int i = 0; i < prefabInfo.Count; i++)
                newQueue.Push(CreateObject(prefabInfo.Prefab));
        }
    }

    private T CreateObject<T>() where T : MonoBehaviour
    {
        MonoBehaviour prefab = null;

        foreach(var prefabInfo in _allPrefab)
        {
            if (prefabInfo.Prefab.GetType() == typeof(T))
            {
                prefab = prefabInfo.Prefab;
                break;
            }
        }

        return (T)CreateObject(prefab);
    }

    private MonoBehaviour CreateObject(MonoBehaviour prefab)
    {
        MonoBehaviour obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        obj.transform.position = transform.position;

        return obj;
    }
}

[Serializable]
public class PrefabInfo
{
    public MonoBehaviour Prefab;
    public int Count;
}