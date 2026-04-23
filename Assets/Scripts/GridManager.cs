using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public int width = 30;
    public int height = 15;

    public TileData[,] grid;

    [Header("Tile Stats")]
    public float baseHealth = 10f;
    public float healthMultiplier = 2f;

    public int baseValue = 5;
    public int valueMultiplier = 2;

    [Header("Dig Tilemap")]
    public Tilemap digTilemap;
    public TileBase digTileAsset;

    [Header("Dig Tile Gradient")]
    public Color topDepthColor = new Color(0.65f, 0.85f, 0.55f);
    public Color middleDepthColor = new Color(0.55f, 0.42f, 0.25f);
    public Color deepDepthColor = new Color(0.35f, 0.28f, 0.18f);

    void Start()
    {
        GenerateGrid();
        DrawGrid();
    }

    void GenerateGrid()
    {
        grid = new TileData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileData tile = new TileData();
                tile.x = x;
                tile.y = y;

                tile.maxHealth = (int)(baseHealth + x/2);
                tile.currentHealth = tile.maxHealth;

                tile.value = (int)(baseValue + x/3 + x/8);

                tile.isDestroyed = false;
                tile.workersAssigned=0;
                tile.maxWorkers = 3;

                tile.baseColor = GetDepthGradientColor(x);

                grid[x, y] = tile;
            }
        }
    }

    void DrawGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                digTilemap.SetTile(pos, digTileAsset);

                TileData tile = grid[x, y];
                digTilemap.SetTileFlags(pos, TileFlags.None);
                digTilemap.SetColor(pos, tile.baseColor);
            }
        }
    }

    Color GetDepthGradientColor(int x)
    {
        float t = width <= 1 ? 0f : (float)x / (width - 1);

        if (t < 0.5f)
        {
            return Color.Lerp(topDepthColor, middleDepthColor, t * 2f);
        }

        return Color.Lerp(middleDepthColor, deepDepthColor, (t - 0.5f) * 2f);
    }

    public void RemoveTile(int x, int y)
    {
        digTilemap.SetTile(new Vector3Int(x, y, 0), null);
        AudioDirector.Instance?.PlayTileDug();
    }

    public void UpdateTileVisual(TileData tile)
    {
        if (tile.isDestroyed) return;

        Vector3Int pos = new Vector3Int(tile.x, tile.y, 0);

        float t = Mathf.Clamp01(tile.currentHealth / tile.maxHealth);

        Color finalColor = Color.Lerp(new Color(0.9f,0.7f,0.3f), tile.baseColor, t);

        digTilemap.SetTileFlags(pos, TileFlags.None);
        digTilemap.SetColor(pos, finalColor);
    }

    public bool IsColumnUnlocked(int x)
    {
        if (x == 0) return true;

        for (int y = 0; y < height; y++)
        {
            if (grid[x - 1, y].isDestroyed)
                return true;
        }

        return false;
    }

    public bool IsTileDiggableForWorker(int x, int y)
    {
        if (!IsInside(x, y)) return false;

        TileData tile = grid[x, y];

        if (tile.isDestroyed)
            return false;

        if (tile.workersAssigned >= tile.maxWorkers)
            return false;

        return HasPreviousTileDestroyed(x, y);
    }

    public bool IsTileDiggableForPlayer(int x, int y)
    {
        if (!IsInside(x, y)) return false;

        TileData tile = grid[x, y];

        if (tile.isDestroyed)
            return false;

        return true;
    }

    bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool HasPreviousTileDestroyed(int x, int y)
    {
        if (!IsInside(x, y)) return false;
        if (x == 0) return true;

        return IsInside(x - 1, y) && grid[x - 1, y].isDestroyed;
    }

    public int GetMaxDepthReached()
    {
        for (int x = width - 1; x >= 0; x--)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].isDestroyed)
                    return x;
            }
        }

        return 0;
    }
}