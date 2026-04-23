using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayPoolBootstrap : MonoBehaviour
{
    [Serializable]
    public class PoolEntry
    {
        public GameObject prefab;
        public int count = 6;
    }

    public List<PoolEntry> entries = new List<PoolEntry>();

    void Start()
    {
        foreach (var entry in entries)
        {
            if (entry.prefab == null) continue;
            GameObjectPoolManager.Instance.Prewarm(entry.prefab, entry.count);
        }
    }
}