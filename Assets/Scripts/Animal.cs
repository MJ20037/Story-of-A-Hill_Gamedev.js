using UnityEngine;

public class Animal : MonoBehaviour, IPoolable
{
    private enum AnimalState
    {
        Chasing,
        Retreating,
        Waiting,
        Neutralized,
        Captured,
        Dead
    }

    public float speed = 3f;
    public float attackRange = 0.5f;
    public float killCooldown = 0.5f;

    public ParticleSystem walkParticles;

    private float killTimer = 0f;

    private WorkerManager workerManager;
    private Worker targetWorker;
    private Animator anim;

    private AnimalState state = AnimalState.Chasing;

    void Awake()
    {
        workerManager = FindObjectOfType<WorkerManager>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        walkParticles.Play();
    }

    void Update()
    {
        if (state == AnimalState.Dead || state == AnimalState.Neutralized || state == AnimalState.Captured)
            return;

        if (state == AnimalState.Retreating)
        {
            RetreatUpdate();
            return;
        }

        if (state == AnimalState.Waiting)
            return;

        if (workerManager == null)
            workerManager = FindObjectOfType<WorkerManager>();

        if (workerManager == null || workerManager.workerCount <= 0 || workerManager.workers.Count == 0)
        {
            RetreatToLeftAndWait();
            return;
        }

        if (killTimer > 0f)
        {
            killTimer -= Time.deltaTime;
            return;
        }

        AcquireTarget();

        if (targetWorker == null)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, 0, 90);
            return;
        }

        Vector3 dir = (targetWorker.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        if (Vector3.Distance(transform.position, targetWorker.transform.position) <= attackRange)
        {
            KillWorker();
        }
    }

    void AcquireTarget()
    {
        if (workerManager == null)
            workerManager = FindObjectOfType<WorkerManager>();

        if (workerManager == null || workerManager.workers.Count == 0)
        {
            targetWorker = null;
            return;
        }

        if (targetWorker != null && targetWorker.gameObject != null && targetWorker.gameObject.activeInHierarchy)
            return;

        float bestDist = float.MaxValue;
        Worker best = null;

        foreach (var w in workerManager.workers)
        {
            if (w == null || !w.gameObject.activeInHierarchy)
                continue;

            float d = Vector3.Distance(transform.position, w.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = w;
            }
        }

        targetWorker = best;
    }

    void KillWorker()
    {
        if (targetWorker == null) return;

        workerManager.workers.Remove(targetWorker);
        workerManager.workerCount = Mathf.Max(0, workerManager.workerCount - 1);
        workerManager.EnableHiring();

        BloodManager.Instance?.SpawnBlood(targetWorker.transform.position);

        Destroy(targetWorker.gameObject);
        targetWorker = null;

        killTimer = killCooldown;

        if (workerManager.workerCount <= 0)
        {
            AttackManager.Instance?.OnWorkersDepleted();
        }
    }

    void RetreatUpdate()
    {
        float waitX = ProgressCameraController.Instance != null
            ? ProgressCameraController.Instance.GetLeftSpawnX()
            : transform.position.x - 5f;

        Vector3 target = new Vector3(waitX, transform.position.y, 0f);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0, 0, 90);

        if (transform.position.x <= waitX + 0.05f)
        {
            state = AnimalState.Waiting;
            targetWorker = null;
        }
    }

    public void RetreatToLeftAndWait()
    {
        if (state == AnimalState.Dead || state == AnimalState.Neutralized || state == AnimalState.Captured)
            return;

        targetWorker = null;
        state = AnimalState.Retreating;
    }

    public void ResumeAttack()
    {
        if (state == AnimalState.Waiting || state == AnimalState.Retreating)
        {
            state = AnimalState.Chasing;
            targetWorker = null;
        }
    }

    public void KillAnimal()
    {
        if (state == AnimalState.Dead) return;

        state = AnimalState.Dead;
        targetWorker = null;

        walkParticles.Stop();

        BloodManager.Instance?.SpawnBlood(transform.position);

        AnimalTracker.Instance?.Unregister(this);
        AudioDirector.Instance?.PlayAnimalKill();
        GameStats.Instance?.RegisterKill();

        ReturnToPool();
    }

    public void Tranquilize()
    {
        if (state == AnimalState.Neutralized || state == AnimalState.Dead) return;

        state = AnimalState.Neutralized;
        targetWorker = null;

        AnimalTracker.Instance?.OnAnimalNeutralized();
        AudioDirector.Instance?.PlayAnimalFall();
        GameStats.Instance?.RegisterRescue();

        walkParticles.Stop();

        if (anim != null)
            anim.SetTrigger("Stop");

        if (RescueVehicleManager.Instance != null)
        {
            RescueVehicleManager.Instance.RequestPickup(this);
        }
    }

    public void AttachToVehicle(Transform vehicle)
    {
        state = AnimalState.Captured;
        targetWorker = null;

        transform.SetParent(vehicle, true);
        transform.localPosition = new Vector3(-0.15f, 0f, 0f);
    }

    public void ReleaseAfterRescue()
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (AnimalTracker.Instance != null)
            AnimalTracker.Instance.Unregister(this);

        if (GameObjectPoolManager.Instance != null)
            GameObjectPoolManager.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }

    public void OnSpawnedFromPool()
    {
        state = AnimalState.Chasing;
        killTimer = 0f;
        targetWorker = null;

        transform.SetParent(null, true);

        if (anim == null)
            anim = GetComponent<Animator>();

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        if (AnimalTracker.Instance != null)
            AnimalTracker.Instance.Register(this);

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;
    }

    public void OnReturnedToPool()
    {
        state = AnimalState.Chasing;
        killTimer = 0f;
        targetWorker = null;
        transform.SetParent(null, true);
    }

    public bool IsDangerous()
    {
        return state == AnimalState.Chasing;
    }
}