using System.Collections.Generic;
using UnityEngine;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    private Dictionary<int, Queue<ParticleSystem>> pools = new Dictionary<int, Queue<ParticleSystem>>();

    void Awake()
    {
        Instance = this;
    }

    // 🔥 PREWARM (call this manually)
    public void Prewarm(ParticleSystem prefab, int count)
    {
        int key = prefab.GetInstanceID();

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<ParticleSystem>();

        for (int i = 0; i < count; i++)
        {
            ParticleSystem ps = Instantiate(prefab, transform);
            ps.gameObject.SetActive(false);

            var tag = ps.GetComponent<PooledParticleTag>();
            if (tag == null) tag = ps.gameObject.AddComponent<PooledParticleTag>();

            tag.poolKey = key;

            pools[key].Enqueue(ps);
        }
    }

    public ParticleSystem Spawn(ParticleSystem prefab, Vector3 pos)
    {
        int key = prefab.GetInstanceID();

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<ParticleSystem>();

        ParticleSystem ps = null;

        if (pools[key].Count > 0)
        {
            ps = pools[key].Dequeue();
        }
        else
        {
            ps = Instantiate(prefab, transform);

            var tag = ps.GetComponent<PooledParticleTag>();
            if (tag == null) tag = ps.gameObject.AddComponent<PooledParticleTag>();

            tag.poolKey = key;
        }

        ps.transform.position = pos;
        ps.gameObject.SetActive(true);
        ps.Play(true);

        return ps;
    }

    public void Release(ParticleSystem ps)
    {
        if (ps == null) return;

        var tag = ps.GetComponent<PooledParticleTag>();

        if (tag == null)
        {
            Destroy(ps.gameObject);
            return;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);

        pools[tag.poolKey].Enqueue(ps);
    }
}