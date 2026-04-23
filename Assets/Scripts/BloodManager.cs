using UnityEngine;

public class BloodManager : MonoBehaviour
{
    public static BloodManager Instance;

    public GameObject[] bloodPrefabs;

    public float randomOffset = 0.3f;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnBlood(Vector3 position)
    {
        if (bloodPrefabs == null || bloodPrefabs.Length == 0)
            return;

        GameObject prefab = bloodPrefabs[Random.Range(0, bloodPrefabs.Length)];

        Vector3 offset = new Vector3(
            Random.Range(-randomOffset, randomOffset),
            Random.Range(-randomOffset, randomOffset),
            0f
        );

        GameObject obj = GameObjectPoolManager.Instance.Spawn(prefab, position + offset, Quaternion.identity);

        BloodSplat splat = obj.GetComponent<BloodSplat>();
        if (splat != null)
        {
            splat.Setup(position + offset);
        }
    }
}