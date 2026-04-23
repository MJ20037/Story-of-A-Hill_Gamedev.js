using System.Collections.Generic;
using UnityEngine;

public class BirdManager : MonoBehaviour
{
    public GameObject birdPrefab;

    [Header("Bird Count")]
    public int maxBirds = 10;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnHeightMin = 2f;
    public float spawnHeightMax = 6f;

    [Header("Flock Settings")]
    public int minFlockSize = 2;
    public int maxFlockSize = 5;
    public float flockSpread = 0.5f;

    private float spawnTimer;

    private List<GameObject> activeBirds = new List<GameObject>();
    private GridManager grid;
    private Camera cam;

    public int ActiveBirdCount => activeBirds.Count;
    public int MaxBirdCount => maxBirds;

    void Start()
    {
        grid = FindObjectOfType<GridManager>();
        cam = Camera.main;
    }

    void Update()
    {
        UpdateBirds();
    }

    void UpdateBirds()
    {
        int depth = grid.GetMaxDepthReached();

        float t = Mathf.Clamp01(depth / (grid.width * 0.8f));
        int targetCount = Mathf.RoundToInt(Mathf.Lerp(maxBirds, 0, t));

        // 🟥 Remove extra birds
        while (activeBirds.Count > targetCount)
        {
            GameObject b = activeBirds[0];
            activeBirds.RemoveAt(0);

            var bird = b.GetComponent<Bird>();
            if (bird != null)
                bird.BeginExit();
            else
                GameObjectPoolManager.Instance.Release(b);
        }

        // 🟩 Spawn flocks over time
        spawnTimer += Time.deltaTime;

        if (activeBirds.Count < targetCount && spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnFlock(targetCount - activeBirds.Count);
        }
    }

    void SpawnFlock(int remainingSlots)
    {
        int flockSize = Random.Range(minFlockSize, maxFlockSize + 1);
        flockSize = Mathf.Min(flockSize, remainingSlots);

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        bool leftToRight = Random.value > 0.5f;

        float startX = leftToRight
            ? cam.transform.position.x - halfWidth - 2f
            : cam.transform.position.x + halfWidth + 2f;

        float endX = leftToRight
            ? cam.transform.position.x + halfWidth + 2f
            : cam.transform.position.x - halfWidth - 2f;

        float baseY = Random.Range(spawnHeightMin, spawnHeightMax);

        for (int i = 0; i < flockSize; i++)
        {
            float offsetX = Random.Range(-flockSpread, flockSpread);
            float offsetY = Random.Range(-flockSpread * 0.5f, flockSpread * 0.5f);

            Vector3 startPos = new Vector3(startX, baseY, 0f);
            Vector3 endPos = new Vector3(endX, baseY + Random.Range(-1f, 1f), 0f);

            GameObject obj = GameObjectPoolManager.Instance.Spawn(
                birdPrefab,
                startPos,
                Quaternion.identity
            );

            var bird = obj.GetComponent<Bird>();

            if (bird != null)
            {
                bird.Configure(
                    startPos,
                    endPos,
                    Random.Range(3f, 6f),
                    new Vector3(offsetX, offsetY, 0f),
                    Random.value
                );
            }

            activeBirds.Add(obj);
        }
    }
}