using System;
using System.Collections.Generic;
using UnityEngine;



public interface IPoolObject
{
    public void Init(string name);
}




// 게임오브젝트 전용 풀 (비제네릭)
[Serializable]
public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;

    public ObjectPool(GameObject prefab, int initialCount, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialCount; i++)
        {
            var obj = GameObject.Instantiate(prefab, parent);
            DontDestroyOnLoad(obj);

            if (obj.TryGetComponent<IPoolObject>(out var poolObj))
            {
                poolObj.Init(prefab.name);
            }
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        while (pool.Count > 0)
        {
            var obj = pool.Dequeue();

            if (obj == null)
                continue;

            // 이미 사용 중이면 건너뜀
            if (obj.activeInHierarchy)
                continue;

            obj.SetActive(true);
            return obj;
        }

        // 없으면 새로 생성
        var newObj = GameObject.Instantiate(prefab, parent);
        DontDestroyOnLoad(newObj);

        if (newObj.TryGetComponent<IPoolObject>(out var poolObj))
        {
            poolObj.Init(prefab.name);
        }

        return newObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}


// 타입별 풀 매니저 (비제네릭)
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    public Dictionary<string, ObjectPool> pools;
    private Transform parent;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        pools = new Dictionary<string, ObjectPool>();
    }

    public ObjectPoolManager(Transform parent = null)
    {
        this.parent = parent;
    }

    // 풀 생성
    public void CreatePool(string name, GameObject prefab, int initialCount)
    {
        if (!pools.ContainsKey(name))
        {
            pools.Add(name, new

                (prefab, initialCount, parent));
        }
    }

    // 풀에서 꺼내기
    public GameObject Get(string name)
    {
        if (pools.TryGetValue(name, out var pool))
        {
            return pool.Get();
        }
        return null;
    }


    Vector3 PoolPos = new Vector3(2000, 0, 0);
    // 풀에 반환
    public void Return(GameObject obj, string name)
    {
        if (pools.TryGetValue(name, out var pool))
        {
            //임시
            obj.transform.position = PoolPos;
            pool.Return(obj);
        }
        else
        {
            foreach (var a in pools.Keys)
            {
                Logger.Log(a.ToString());

            }
            GameObject.Destroy(obj);
        }
    }
}
