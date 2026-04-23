using UnityEngine;
using UnityEngine.Tilemaps;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [Header("References")]
    public GridManager gridManager;
    public Tilemap environmentTilemap;
    public TileBase environmentTileAsset;

    [Header("Environment Colors")]
    public Color aliveColor = new Color(0.55f, 0.85f, 0.55f);
    public Color midColor = new Color(0.55f, 0.45f, 0.25f);
    public Color deadColor = new Color(0.25f, 0.25f, 0.25f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //DrawEnvironmentGrid();
    }

    void Update()
    {
        UpdateEnvironmentTint();
    }

    void DrawEnvironmentGrid()
    {
        if (gridManager == null || environmentTilemap == null || environmentTileAsset == null)
            return;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                environmentTilemap.SetTile(pos, environmentTileAsset);
                environmentTilemap.SetTileFlags(pos, TileFlags.None);
                environmentTilemap.SetColor(pos, aliveColor);
            }
        }
    }

    void UpdateEnvironmentTint()
    {
        if (environmentTilemap == null || gridManager == null)
            return;

        float depth01 = Mathf.Clamp01(
            (float)gridManager.GetMaxDepthReached() / Mathf.Max(1, gridManager.width - 1)
        );

        Color envColor;

        if (depth01 < 0.5f)
        {
            envColor = Color.Lerp(aliveColor, midColor, depth01 * 2f);
        }
        else
        {
            envColor = Color.Lerp(midColor, deadColor, (depth01 - 0.5f) * 2f);
        }

        // 👇 THIS IS THE IMPORTANT CHANGE
        BoundsInt bounds = environmentTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!environmentTilemap.HasTile(pos)) continue;

            environmentTilemap.SetTileFlags(pos, TileFlags.None);
            environmentTilemap.SetColor(pos, envColor);
        }
    }
}