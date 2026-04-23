using UnityEngine;

public class BloodSplat : MonoBehaviour, IPoolable
{
    private Camera cam;
    private float padding = 2f;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 camPos = cam.transform.position;
        Vector3 pos = transform.position;

        bool offscreen =
            pos.x < camPos.x - halfW - padding ||
            pos.x > camPos.x + halfW + padding ||
            pos.y < camPos.y - halfH - padding ||
            pos.y > camPos.y + halfH + padding;

        if (offscreen)
        {
            ReturnToPool();
        }
    }

    public void Setup(Vector3 position)
    {
        transform.position = position;

        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        float scale = Random.Range(0.9f, 1.3f);
        transform.localScale = Vector3.one * scale;
    }

    void ReturnToPool()
    {
        if (GameObjectPoolManager.Instance != null)
        {
            GameObjectPoolManager.Instance.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnSpawnedFromPool() { }
    public void OnReturnedToPool() { }
}