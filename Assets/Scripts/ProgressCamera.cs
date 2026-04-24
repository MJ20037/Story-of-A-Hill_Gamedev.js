using UnityEngine;

public class ProgressCameraController : MonoBehaviour
{
    public static ProgressCameraController Instance;

    [Header("References")]
    public Camera cam;
    public GridManager gridManager;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float xOffset = 3f; // how much ahead of cleared column

    private int currentDepth = 0;
    private float targetX;
    float initialCameraX;
    int initialDepth = 0;
    

    void Awake()
    {
        Instance = this;

        if (cam == null)
            cam = GetComponent<Camera>();
    }

    void Start()
    {
        initialCameraX = transform.position.x;
        targetX = initialCameraX;
    }

    void Update()
    {
        UpdateTargetDepth();
        MoveCamera();
    }

    void UpdateTargetDepth()
    {
        if (gridManager == null) return;

        int newDepth = currentDepth;

        // Move forward until we hit a column that is NOT cleared
        while (IsColumnCleared(newDepth))
        {
            newDepth++;
        }

        // Only update if changed
        if (newDepth != currentDepth)
        {
            currentDepth = newDepth;

            targetX = initialCameraX + (currentDepth - initialDepth);
        }
}

    bool IsColumnCleared(int x)
    {
        if (x < 0 || x >= gridManager.width)
            return false;

        for (int y = 0; y < gridManager.height; y++)
        {
            if (!gridManager.grid[x, y].isDestroyed)
                return false;
        }

        return true;
    }

    void MoveCamera()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * moveSpeed);

        transform.position = pos;
    }

    public float GetLeftSpawnX()
    {
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x - halfWidth - 2f; // spawn outside view
    }
}