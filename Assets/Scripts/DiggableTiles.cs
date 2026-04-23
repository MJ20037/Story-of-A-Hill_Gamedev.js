using UnityEngine;

public class DiggableTile
{
    public TileData tile;
    private GridManager gridManager;

    public DiggableTile(TileData tile, GridManager gridManager)
    {
        this.tile = tile;
        this.gridManager = gridManager;
    }

    public bool Dig(float power)
    {
        if (tile.isDestroyed) return true;

        tile.currentHealth -= power * Time.deltaTime;
        gridManager.UpdateTileVisual(tile);
        
        if (tile.currentHealth <= 0)
        {
            tile.isDestroyed = true;

            gridManager.RemoveTile(tile.x, tile.y);
            GameManager.Instance.AddMoney(tile.value);
            Vector3 pos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
            FloatingTextPool.Instance.Spawn(pos, tile.value);

            return true; // finished
        }

        return false; // still digging
    }
}