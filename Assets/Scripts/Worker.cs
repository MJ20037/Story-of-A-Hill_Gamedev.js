using UnityEngine;

enum WorkerState
{
    Idle,
    Moving,
    Digging
}

public class Worker : MonoBehaviour
{
    public float digPower = 2f;
    public float moveSpeed = 3f;

    public GameObject workerShadow;
    public GameObject jcbShadow;

    private DiggableTile currentTarget;
    private GridManager gridManager;
    private WorkerState state;

    private Vector3 targetWorldPos;
    public Tool currentTool;

    private ParticleSystem currentDigVfx;
    Animator anim;
    private bool isPanicking = false;
    private AudioSource toolAudioSource;
    private AudioClip lastLoopClip;

    void Start()
    {
        anim = GetComponent<Animator>();
        gridManager = FindObjectOfType<GridManager>();
        state = WorkerState.Idle;

        toolAudioSource = GetComponent<AudioSource>();

        if (toolAudioSource == null)
            toolAudioSource = gameObject.AddComponent<AudioSource>();

        toolAudioSource.playOnAwake = false;
        toolAudioSource.loop = true;
        toolAudioSource.spatialBlend = 0f;
    }

    void OnDisable()
    {
        StopDigVfx();
    }

    void Update()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null) return;
        }

        if (isPanicking)
        {
            Panic();
            StopDigVfx();
            return;
        }

        if (anim != null)
        {
            anim.SetBool("isDigging", state == WorkerState.Digging);
        }

        switch (state)
        {
            case WorkerState.Idle:
                FindNewTarget();
                break;

            case WorkerState.Moving:
                MoveToTarget();
                break;

            case WorkerState.Digging:
                Dig();
                break;
        }
        UpdateToolAudio();
    }

    void FindNewTarget()
    {
        if (currentTarget != null) return;

        TileData bestTile = null;
        float bestScore = float.MinValue;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (!gridManager.IsTileDiggableForWorker(x, y))
                    continue;

                TileData tile = gridManager.grid[x, y];

                // Allow replacement logic
                if (tile.assignedWorkers.Count >= tile.maxWorkers)
                {
                    Worker weakest = null;

                    foreach (var w in tile.assignedWorkers)
                    {
                        if (weakest == null || GetToolPower(w) < GetToolPower(weakest))
                            weakest = w;
                    }

                    // If current worker is stronger → replace
                    if (weakest != null && GetToolPower(this) > GetToolPower(weakest))
                    {
                        weakest.ForceReassign(); // kick weak worker
                        tile.assignedWorkers.Remove(weakest);
                    }
                    else
                    {
                        continue;
                    }
                }

                float depthScore = x * -5f;
                float toolPower = currentTool != null ? currentTool.digPower : 1f;

                float frontPriority = -x * (1f + toolPower * 5f);
                float crowdPenalty = tile.workersAssigned * 4f;
                float randomJitter = Random.Range(0f, 0.25f);

                float score = frontPriority + depthScore - crowdPenalty + randomJitter;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }
        }

        if (bestTile != null)
        {
            bestTile.assignedWorkers.Add(this);
            currentTarget = new DiggableTile(bestTile, gridManager);

            float random = Random.Range(-0.3f, 0.3f);

            if(currentTool.toolName=="Bulldozer"){
                targetWorldPos = new Vector3(bestTile.x - 0.5f, bestTile.y + 0.5f + random, 0);
            }
            else{
                targetWorldPos = new Vector3(bestTile.x - 0.25f, bestTile.y + 0.5f + random, 0);
            }

            state = WorkerState.Moving;
        }
    }

    void MoveToTarget()
    {
        if (currentTarget == null)
        {
            StopDigVfx();
            state = WorkerState.Idle;
            return;
        }

        if (currentTarget.tile.isDestroyed)
        {
            currentTarget.tile.workersAssigned = Mathf.Max(0, currentTarget.tile.workersAssigned - 1);
            currentTarget = null;
            StopDigVfx();
            state = WorkerState.Idle;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetWorldPos) <= 0.08f)
        {
            transform.position = targetWorldPos;
            state = WorkerState.Digging;
            StartDigVfx();
        }
    }

    void UpdateToolAudio()
    {
        if (toolAudioSource == null) return;

        bool isBulldozer = currentTool != null && currentTool.toolName == "Bulldozer";

        if (isPanicking)
        {
            if (toolAudioSource.isPlaying)
                toolAudioSource.Stop();
            return;
        }

        bool shouldPlay =
            currentTool != null &&
            currentTool.workerLoopClip != null &&
            (state == WorkerState.Digging || isBulldozer);

        if (shouldPlay)
        {
            // Only change clip if different
            if (toolAudioSource.clip != currentTool.workerLoopClip)
            {
                toolAudioSource.clip = currentTool.workerLoopClip;
                toolAudioSource.pitch = Random.Range(0.9f, 1.1f);
                toolAudioSource.loop = true;
            }
            if (!toolAudioSource.isPlaying)
            {
                toolAudioSource.Play(); // recover if stopped
            }

            // Volume controlled globally
            toolAudioSource.volume = AudioDirector.Instance != null
                ? AudioDirector.Instance.GetWorkerToolVolumeMultiplier()
                : 0.5f;
        }
        else
        {
            if (toolAudioSource.isPlaying)
            {
                toolAudioSource.Stop();
            }
        }
    }

    void Dig()
    {
        if (currentTarget == null)
        {
            StopDigVfx();
            state = WorkerState.Idle;
            return;
        }

        if (currentTarget.tile.isDestroyed)
        {
            currentTarget.tile.assignedWorkers.Remove(this);
            currentTarget = null;
            StopDigVfx();
            state = WorkerState.Idle;
            return;
        }

        StartDigVfx();

        float power = currentTool != null ? currentTool.digPower : digPower;
        bool finished = currentTarget.Dig(power);

        if (finished)
        {
            currentTarget.tile.assignedWorkers.Remove(this);
            currentTarget = null;
            StopDigVfx();
            state = WorkerState.Idle;
        }
    }

    void StartDigVfx()
    {
        if (currentDigVfx != null) return;
        if (currentTool == null || currentTool.digParticles == null) return;
        if (currentTarget == null) return;

        Vector3 pos = new Vector3(
            currentTarget.tile.x,
            currentTarget.tile.y + 0.5f,
            0f
        );

        currentDigVfx = ParticlePoolManager.Instance.Spawn(currentTool.digParticles, pos);
    }

    void StopDigVfx()
    {
        if (currentDigVfx == null) return;

        ParticlePoolManager.Instance.Release(currentDigVfx);
        currentDigVfx = null;
    }

    public void ApplyTool(Tool tool)
    {
        if (tool == null) return;

        currentTool = tool;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && tool.workerSprite != null)
            sr.sprite = tool.workerSprite;

        var anim = GetComponent<Animator>();
        if (anim != null && tool.animator != null)
            anim.runtimeAnimatorController = tool.animator;
        
        if (toolAudioSource != null)
        {
            toolAudioSource.Stop();
            toolAudioSource.clip = null;
        }
        if(tool.toolName=="Bulldozer"){
            jcbShadow.SetActive(true);
            workerShadow.SetActive(false);
        }
    }

    void Panic()
    {
        transform.position += Vector3.left * moveSpeed * 0.7f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0,0,180);
    }

    public void TriggerPanic()
    {
        isPanicking = true;

        if (toolAudioSource != null && toolAudioSource.isPlaying)
            toolAudioSource.Stop();
    }

    public void ForceReassign()
    {
        StopDigVfx();
        if (currentTarget != null)
        {
            currentTarget.tile.assignedWorkers.Remove(this);
            currentTarget = null;
        }

        if (toolAudioSource != null && toolAudioSource.isPlaying && currentTool.toolName!="Bulldozer")
            toolAudioSource.Stop();

        state = WorkerState.Idle;
    }

    float GetToolPower(Worker w)
    {
        return w.currentTool != null ? w.currentTool.digPower : 1f;
    }
}