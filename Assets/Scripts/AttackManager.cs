using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static System.Action<Animal> AnimalAttack;
    public static AttackManager Instance;

    public GameObject animalPrefab;
    public GridManager gridManager;

    public int minDepthToStart = 5;
    public int depthToStopAt = 25;

    public int minAnimals = 1;
    public int maxAnimals = 10;

    public float baseInterval = 18f;
    public float minInterval = 8f;

    public float spawnGap = 0.2f;

    private WorkerManager workerManager;
    private float timer;
    private int lastWorkerCount = -1;

    private bool waveActive;
    private bool waveDormant;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (workerManager == null)
            workerManager = FindObjectOfType<WorkerManager>();

        lastWorkerCount = GetWorkerCount();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing)
            return;

        if (gridManager == null)
            return;

        if (workerManager == null)
            workerManager = FindObjectOfType<WorkerManager>();

        HandleWorkerTransitions();

        int workerCount = GetWorkerCount();
        if (workerCount <= 0)
            return;

        if (waveActive)
        {
            if (AnimalTracker.Instance != null && AnimalTracker.Instance.ActiveCount == 0)
            {
                ClearWave();
            }

            return;
        }

        int depth = gridManager.GetMaxDepthReached();
        if (depth < minDepthToStart) return;
        if (depth >= depthToStopAt) return;

        float t = Mathf.InverseLerp(minDepthToStart, depthToStopAt, depth);
        float interval = Mathf.Lerp(baseInterval, minInterval, t);

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            SpawnWave(t);
            timer = 0f;
        }
    }

    int GetWorkerCount()
    {
        return workerManager != null ? workerManager.workerCount : 0;
    }

    void HandleWorkerTransitions()
    {
        int currentCount = GetWorkerCount();

        if (lastWorkerCount == -1)
        {
            lastWorkerCount = currentCount;
            return;
        }

        if (currentCount == lastWorkerCount)
            return;

        if (lastWorkerCount > 0 && currentCount == 0)
        {
            OnWorkersDepleted();
        }
        else if (lastWorkerCount == 0 && currentCount > 0)
        {
            OnWorkerHired();
        }

        lastWorkerCount = currentCount;
    }

    void SpawnWave(float depthT)
    {
        waveActive = true;
        waveDormant = false;

        AudioDirector.Instance?.PlayAttackSiren();

        int count = Mathf.RoundToInt(Mathf.Lerp(minAnimals, maxAnimals, depthT));

        for (int i = 0; i < count; i++)
        {
            SpawnAnimal(i * spawnGap, i == 0);
        }
    }

    void SpawnAnimal(float delay, bool triggerUI)
    {
        StartCoroutine(SpawnRoutine(delay, triggerUI));
    }

    System.Collections.IEnumerator SpawnRoutine(float delay, bool triggerUI)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (animalPrefab == null || GameObjectPoolManager.Instance == null)
            yield break;

        float y = Random.Range(0.5f, gridManager.height - 0.5f);
        float spawnX = ProgressCameraController.Instance.GetLeftSpawnX();

        Vector3 pos = new Vector3(spawnX, y, 0f);

        GameObject obj = GameObjectPoolManager.Instance.Spawn(animalPrefab, pos, Quaternion.identity);
        Animal animal = obj != null ? obj.GetComponent<Animal>() : null;

        if (animal == null)
            yield break;

        if (triggerUI && AnimalAttack != null)
        {
            AnimalAttack.Invoke(animal);
        }
    }

    public void OnWorkersDepleted()
    {
        if (!waveActive)
            return;

        waveDormant = true;
        AnimalTracker.Instance?.RetreatWave();
    }

    public void OnWorkerHired()
    {
        if (!waveActive)
            return;

        if (!waveDormant)
            return;

        waveDormant = false;
        AnimalTracker.Instance?.ResumeWave();
    }

    void ClearWave()
    {
        waveActive = false;
        waveDormant = false;
        timer = 0f;
    }
}