using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DigController : MonoBehaviour
{
    public GridManager gridManager;
    public Camera cam;

    public ParticleSystem playerDigParticles;

    public void HandleClick(Vector3 worldPos)
    {
        if (PlayerToolManager.Instance.currentTool != PlayerTool.Pickaxe)
            return;
        Vector2Int gridPos = WorldToGrid(worldPos);

        if (!IsValid(gridPos)) return;

        if (!gridManager.IsTileDiggableForPlayer(gridPos.x, gridPos.y))
            return;

        TileData tile = gridManager.grid[gridPos.x, gridPos.y];

        if (tile.isDestroyed) return;

        SpawnPlayerDigVfx(tile);
        if (AudioDirector.Instance != null)
            AudioDirector.Instance.PlayPlayerDig();

        tile.currentHealth -= PlayerToolManager.Instance.GetPickaxePower();
        gridManager.UpdateTileVisual(tile);

        if (tile.currentHealth <= 0)
        {
            DestroyTile(tile);
        }
    }

    void SpawnPlayerDigVfx(TileData tile)
    {
        if (playerDigParticles == null) return;

        Vector3 pos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);

        ParticleSystem ps = ParticlePoolManager.Instance.Spawn(playerDigParticles, pos);

        StartCoroutine(ReturnToPool(ps, 0.3f));
    }

    IEnumerator ReturnToPool(ParticleSystem ps, float time)
    {
        yield return new WaitForSecondsRealtime(time);

        ParticlePoolManager.Instance.Release(ps);
    }

    void DestroyTile(TileData tile)
    {
        tile.isDestroyed = true;
        tile.workersAssigned = 0;

        gridManager.RemoveTile(tile.x, tile.y);

        GameManager.Instance.AddMoney(tile.value);
        Vector3 pos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);

        FloatingTextPool.Instance.Spawn(pos, tile.value);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.FloorToInt(worldPos.y);
        return new Vector2Int(x, y);
    }

    bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.width &&
               pos.y >= 0 && pos.y < gridManager.height;
    }
}