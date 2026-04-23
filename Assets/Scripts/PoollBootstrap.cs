using UnityEngine;

public class PoolBootstrap : MonoBehaviour
{
    public ParticleSystem[] particlePrefabs;

    void Start()
    {
        foreach (var ps in particlePrefabs)
        {
            ParticlePoolManager.Instance.Prewarm(ps, 20); 
        }
    }
}