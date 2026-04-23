using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolManager : MonoBehaviour
{
    public static GameObjectPoolManager Instance;

    private readonly Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        int key = prefab.GetInstanceID();

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);

            var tag = obj.GetComponent<PooledGameObjectTag>();
            if (tag == null) tag = obj.AddComponent<PooledGameObjectTag>();
            tag.poolKey = key;

            pools[key].Enqueue(obj);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null) return null;

        int key = prefab.GetInstanceID();

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();

        GameObject obj = null;

        while (pools[key].Count > 0 && obj == null)
            obj = pools[key].Dequeue();

        if (obj == null)
        {
            obj = Instantiate(prefab);
            var tag = obj.GetComponent<PooledGameObjectTag>();
            if (tag == null) tag = obj.AddComponent<PooledGameObjectTag>();
            tag.poolKey = key;
        }

        obj.transform.SetParent(parent, false);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawnedFromPool();

        return obj;
    }

    public void Release(GameObject obj)
    {
        if (obj == null) return;

        var tag = obj.GetComponent<PooledGameObjectTag>();
        if (tag == null)
        {
            Destroy(obj);
            return;
        }

        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnedToPool();

        obj.SetActive(false);
        obj.transform.SetParent(transform, false);

        if (!pools.ContainsKey(tag.poolKey))
            pools[tag.poolKey] = new Queue<GameObject>();

        pools[tag.poolKey].Enqueue(obj);
    }
}