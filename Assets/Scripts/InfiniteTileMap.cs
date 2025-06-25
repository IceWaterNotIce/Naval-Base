using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InfiniteTileMap : MonoBehaviour
{
    public Tilemap oceanTileMap; // Reference to the Tilemap
    public TileBase oceanRuleTile; // Rule Tile to use
    public int tileSize = 10; // Size of each tile
    public int viewDistance = 3; // Number of tiles visible around the player
    public Transform navalBase; // Reference to the naval base
    public PlayerShipManager playerShipManager; // Reference to PlayerShipManager

    private HashSet<Vector2Int> activeTiles = new HashSet<Vector2Int>();
    private Vector2Int currentTilePosition;

    void Start()
    {
        if ((navalBase == null && (playerShipManager == null || playerShipManager.playerShips.Count == 0)) || oceanTileMap == null || oceanRuleTile == null)
        {
            Debug.LogError("Naval Base, PlayerShipManager, Tilemap, or Rule Tile reference is missing!");
            return;
        }

        UpdateTileMap(); // Initialize the tile map
    }

    void Update()
    {
        HashSet<Vector2Int> newTilePositions = GetAllEntityTilePositions();
        if (!newTilePositions.SetEquals(activeTiles))
        {
            activeTiles = newTilePositions;
            UpdateTileMap(); // Update the tile map based on new positions
        }
    }

    private HashSet<Vector2Int> GetAllEntityTilePositions()
    {
        HashSet<Vector2Int> entityTilePositions = new HashSet<Vector2Int>();

        // Add naval base position
        if (navalBase != null)
        {
            entityTilePositions.Add(GetTilePosition(navalBase.position));
        }

        // Add all player ships' positions
        if (playerShipManager != null)
        {
            foreach (var ship in playerShipManager.playerShips)
            {
                if (ship != null)
                {
                    entityTilePositions.Add(GetTilePosition(ship.transform.position));
                    Debug.Log($"Player ship {ship.name} at tile position {GetTilePosition(ship.transform.position)}");
                }
            }
        }

        return entityTilePositions;
    }

    private Vector2Int GetTilePosition(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / tileSize),
            Mathf.FloorToInt(position.y / tileSize)
        );
    }

    private void UpdateTileMap()
    {
        HashSet<Vector2Int> tilesToKeep = new HashSet<Vector2Int>();
        HashSet<Vector2Int> tilesToAdd = new HashSet<Vector2Int>();

        // 計算所有需要保留的瓦片（實體周圍的範圍）
        foreach (var centerTile in GetAllEntityTilePositions())
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int y = -viewDistance; y <= viewDistance; y++)
                {
                    Vector2Int tilePosition = centerTile + new Vector2Int(x, y);
                    tilesToKeep.Add(tilePosition); // 標記為需保留

                    // 如果是新瓦片，加入待新增列表
                    if (!activeTiles.Contains(tilePosition))
                    {
                        tilesToAdd.Add(tilePosition);
                    }
                }
            }
        }

        // 新增瓦片
        foreach (var tilePosition in tilesToAdd)
        {
            CreateTile(tilePosition);
        }

        // 移除不在保留列表中的瓦片
        RemoveUnusedTiles(tilesToKeep);
    }

    private void CreateTile(Vector2Int tilePosition)
    {
        Vector3Int tilemapPosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);
        oceanTileMap.SetTile(tilemapPosition, oceanRuleTile);
        activeTiles.Add(tilePosition);
    }

    private void RemoveUnusedTiles(HashSet<Vector2Int> tilesToKeep)
    {
        foreach (var tilePosition in new HashSet<Vector2Int>(activeTiles))
        {
            if (!tilesToKeep.Contains(tilePosition))
            {
                Vector3Int tilemapPosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);
                oceanTileMap.SetTile(tilemapPosition, null);
                activeTiles.Remove(tilePosition);
            }
        }
    }
}
