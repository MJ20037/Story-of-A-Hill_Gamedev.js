using UnityEngine;

public class Bird : MonoBehaviour, IPoolable
{
    private enum BirdState
    {
        Flying,
        Exiting
    }

    [Header("Flight")]
    public float speed = 2f;
    public float curveAmplitude = 0.6f;
    public float wobbleAmplitude = 0.18f;
    public float wobbleFrequency = 2.2f;

    [Header("Exit")]
    public float exitSpeed = 3.5f;
    public float exitPadding = 2f;

    private BirdState state;

    private Camera cachedCamera;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 formationOffset;
    private Vector3 exitTarget;

    private float flightDuration = 4f;
    private float flightT;
    private float phase;
    private float noiseSeed;
    private Vector3 lastPosition;

    void Awake()
    {
        cachedCamera = Camera.main;
    }

    void Update()
    {
        if (state == BirdState.Exiting)
        {
            UpdateExit();
            return;
        }

        UpdateFlight();
    }

    public void Configure(Vector3 start, Vector3 end, float duration, Vector3 groupOffset, float seed)
    {
        startPos = start;
        endPos = end;
        flightDuration = Mathf.Max(0.5f, duration);
        formationOffset = groupOffset;
        phase = seed * 10f;
        noiseSeed = seed * 100f;

        flightT = Random.Range(0f, 0.25f);
        state = BirdState.Flying;
    }

    void UpdateFlight()
    {
        flightT += Time.deltaTime / flightDuration;

        if (flightT >= 1f)
        {
            PickNewRoute();
            return;
        }

        float smoothT = Mathf.SmoothStep(0f, 1f, flightT);

        Vector3 basePos = Vector3.Lerp(startPos, endPos, smoothT);
        Vector3 dir = (endPos - startPos).normalized;

        Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

        float arc = Mathf.Sin(flightT * Mathf.PI) * curveAmplitude;
        float wobbleY = Mathf.Sin((Time.time * wobbleFrequency) + phase) * wobbleAmplitude;
        float wobbleX = (Mathf.PerlinNoise(noiseSeed, Time.time * 0.35f) - 0.5f) * wobbleAmplitude;

        Vector3 newPos = basePos
                        + perp * arc
                        + new Vector3(wobbleX, wobbleY, 0f)
                        + formationOffset;

        transform.position = newPos;

        // 👉 Face movement direction
        Vector3 moveDir = newPos - lastPosition;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle-90f, Vector3.forward);
        }

        lastPosition = newPos;
    }

    void PickNewRoute()
    {
        if (cachedCamera == null)
            cachedCamera = Camera.main;

        if (cachedCamera == null)
            return;

        float halfHeight = cachedCamera.orthographicSize;
        float halfWidth = halfHeight * cachedCamera.aspect;

        bool leftToRight = Random.value > 0.5f;

        float startX = leftToRight
            ? cachedCamera.transform.position.x - halfWidth - exitPadding
            : cachedCamera.transform.position.x + halfWidth + exitPadding;

        float endX = leftToRight
            ? cachedCamera.transform.position.x + halfWidth + exitPadding
            : cachedCamera.transform.position.x - halfWidth - exitPadding;

        float startY = Random.Range(-0.5f, halfHeight * 0.8f);
        float endY = Mathf.Clamp(startY + Random.Range(-1.2f, 1.2f), -halfHeight * 0.7f, halfHeight * 0.9f);

        startPos = new Vector3(startX, startY, 0f);
        endPos = new Vector3(endX, endY, 0f);
        flightDuration = Random.Range(3f, 6f);

        flightT = 0f;

        // small formation drift so they do not overlap exactly
        formationOffset = new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.12f, 0.12f),
            0f
        );
    }

    void UpdateExit()
    {
        Vector3 newPos = Vector3.MoveTowards(
            transform.position,
            exitTarget,
            exitSpeed * Time.deltaTime
        );

        transform.position = newPos;

        // 👉 Face movement direction
        Vector3 moveDir = newPos - lastPosition;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle-90f, Vector3.forward);
        }

        lastPosition = newPos;

        if (Vector3.Distance(transform.position, exitTarget) <= 0.05f)
        {
            ReturnToPool();
        }
    }

    public void BeginExit()
    {
        if (cachedCamera == null)
            cachedCamera = Camera.main;

        if (cachedCamera == null)
        {
            ReturnToPool();
            return;
        }

        float halfHeight = cachedCamera.orthographicSize;
        float halfWidth = halfHeight * cachedCamera.aspect;

        float leftX = cachedCamera.transform.position.x - halfWidth - exitPadding;
        float rightX = cachedCamera.transform.position.x + halfWidth + exitPadding;

        // fly out on the side they are already closer to
        exitTarget = transform.position.x < cachedCamera.transform.position.x
            ? new Vector3(leftX, transform.position.y, 0f)
            : new Vector3(rightX, transform.position.y, 0f);

        state = BirdState.Exiting;
    }

    void ReturnToPool()
    {
        if (GameObjectPoolManager.Instance != null)
            GameObjectPoolManager.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }

    public void OnSpawnedFromPool()
    {
        state = BirdState.Flying;
        lastPosition = transform.position;
    }

    public void OnReturnedToPool()
    {
        state = BirdState.Flying;
    }
}