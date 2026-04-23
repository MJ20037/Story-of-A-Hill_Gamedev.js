using System.Collections;
using UnityEngine;

public class ShotVfxSpawner : MonoBehaviour
{
    public static ShotVfxSpawner Instance;

    [Header("Prefabs")]
    public ParticleSystem gunShotPrefab;
    public ParticleSystem tranqShotPrefab;

    [Header("Pooling")]
    public float releaseDelay = 0.35f;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnGun(Vector3 worldPos)
    {
        Spawn(gunShotPrefab, worldPos);
    }

    public void SpawnTranq(Vector3 worldPos)
    {
        Spawn(tranqShotPrefab, worldPos);
    }

    void Spawn(ParticleSystem prefab, Vector3 worldPos)
    {
        if (prefab == null) return;
        if (ParticlePoolManager.Instance == null) return;

        ParticleSystem ps = ParticlePoolManager.Instance.Spawn(prefab, worldPos);
        if (ps != null)
            StartCoroutine(ReturnAfter(ps, releaseDelay));
    }

    IEnumerator ReturnAfter(ParticleSystem ps, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (ps != null && ParticlePoolManager.Instance != null)
            ParticlePoolManager.Instance.Release(ps);
    }
}