using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public string tileName;
}

[System.Serializable]
public class TileMapData
{
    public List<TileData> tiles = new List<TileData>();
}

public class InfiniteTileMap : MonoBehaviour
{
    public Tilemap oceanTileMap; // Reference to the Ocean Tilemap
    public TileBase oceanRuleTile; // Ocean Rule Tile to use
    public Tilemap landTileMap; // Reference to the Land Tilemap
    public TileBase landRuleTile; // Land Rule Tile to use
    public int tileSize = 10; // Size of each tile
    public int viewDistance = 3; // Number of tiles visible around the player
    public Transform navalBase; // Reference to the naval base
    public PlayerShipManager playerShipManager; // Reference to PlayerShipManager
    public int landSeed = 42; // Seed for land generation
    public float landThreshold = 0.5f; // Threshold for land generation
    public float islandScale = 0.1f; // 控制島嶼大小的縮放因子
    public float islandSpacing = 0.2f; // 控制島嶼之間距離的縮放因子

    public string oceanTileMapSavePath = "OceanTileMap.json"; // Path to save ocean tilemap
    public string landTileMapSavePath = "LandTileMap.json"; // Path to save land tilemap

    public Tilemap oceanSavedTileMap; // Tilemap to store rendered ocean tiles
    public Tilemap landSavedTileMap; // Tilemap to store rendered land tiles

    private HashSet<Vector2Int> activeTiles = new HashSet<Vector2Int>();

    void Start()
    {
        if ((navalBase == null && (playerShipManager == null || playerShipManager.playerShips.Count == 0)) || oceanTileMap == null || oceanRuleTile == null)
        {
            Debug.LogError("Naval Base, PlayerShipManager, Ocean Tilemap, or Ocean Rule Tile reference is missing!");
            return;
        }

        LoadSavedTileMaps(); // Load saved tilemaps on start
        UpdateTileMap(); // Initialize the tile map
    }

    void OnApplicationQuit()
    {
        SaveTileMaps(); // Save tilemaps when the application quits
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
        HashSet<Vector2Int> oceanTilesToAdd = new HashSet<Vector2Int>();
        HashSet<Vector2Int> landTilesToAdd = new HashSet<Vector2Int>();

        // 計算所有需要保留的瓦片
        foreach (var centerTile in GetAllEntityTilePositions())
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int y = -viewDistance; y <= viewDistance; y++)
                {
                    Vector2Int tilePosition = centerTile + new Vector2Int(x, y);
                    tilesToKeep.Add(tilePosition);

                    // 檢查是否需要新增瓦片
                    if (!activeTiles.Contains(tilePosition))
                    {
                        oceanTilesToAdd.Add(tilePosition);
                        landTilesToAdd.Add(tilePosition);
                    }
                }
            }
        }

        // 新增海洋瓦片
        foreach (var tilePosition in oceanTilesToAdd)
        {
            CreateTile(tilePosition, oceanTileMap, oceanRuleTile);
        }

        // 新增陸地瓦片
        foreach (var tilePosition in landTilesToAdd)
        {
            CreateTile(tilePosition, landTileMap, landRuleTile);
        }

        // 移除未使用的瓦片（分開處理海洋和陸地）
        RemoveUnusedTiles(tilesToKeep, oceanTileMap);
        RemoveUnusedTiles(tilesToKeep, landTileMap);
    }

    private void CreateTile(Vector2Int tilePosition, Tilemap tilemap, TileBase ruleTile)
    {
        if (tilemap == null || ruleTile == null) return;

        Vector3Int tilemapPosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);

        // 如果是陸地瓦片地圖，使用 Perlin 噪聲生成島嶼
        if (tilemap == landTileMap)
        {
            float noiseValue = Mathf.PerlinNoise(
                (tilePosition.x + landSeed) * islandScale * islandSpacing,
                (tilePosition.y + landSeed) * islandScale * islandSpacing
            );
            if (noiseValue > landThreshold)
            {
                tilemap.SetTile(tilemapPosition, ruleTile);
                landSavedTileMap?.SetTile(tilemapPosition, ruleTile); // Save rendered land tile
            }
        }
        else
        {
            tilemap.SetTile(tilemapPosition, ruleTile);
            oceanSavedTileMap?.SetTile(tilemapPosition, ruleTile); // Save rendered ocean tile
        }

        activeTiles.Add(tilePosition);
    }

    private void RemoveUnusedTiles(HashSet<Vector2Int> tilesToKeep, Tilemap tilemap)
    {
        if (tilemap == null) return;

        // 取得當前 tilemap 中的所有瓦片位置
        List<Vector3Int> positionsToRemove = new List<Vector3Int>();
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) != null)
            {
                Vector2Int tilePos = new Vector2Int(position.x, position.y);
                if (!tilesToKeep.Contains(tilePos))
                {
                    positionsToRemove.Add(position);
                }
            }
        }

        // 實際移除瓦片
        foreach (var position in positionsToRemove)
        {
            tilemap.SetTile(position, null);

            // 從 activeTiles 中移除（如果存在）
            Vector2Int tilePos = new Vector2Int(position.x, position.y);
            activeTiles.Remove(tilePos);
        }
    }

    private void SaveTileMaps()
    {
        if (oceanSavedTileMap != null)
        {
            SaveTileMap(oceanSavedTileMap, oceanTileMapSavePath);
        }
        if (landSavedTileMap != null)
        {
            SaveTileMap(landSavedTileMap, landTileMapSavePath);
        }
    }

    private void LoadSavedTileMaps()
    {
        if (oceanSavedTileMap != null)
        {
            LoadTileMap(oceanSavedTileMap, oceanTileMapSavePath);
        }
        if (landSavedTileMap != null)
        {
            LoadTileMap(landSavedTileMap, landTileMapSavePath);
        }
    }

    private void SaveTileMap(Tilemap tilemap, string filePath)
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is null. Cannot save tilemap.");
            return;
        }

        string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

        // 確保目錄存在
        string directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        TileMapData tileMapData = new TileMapData();

        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                tileMapData.tiles.Add(new TileData
                {
                    x = position.x,
                    y = position.y,
                    tileName = tile.name
                });
                Debug.Log($"Saving tile: {tile.name} at position {position}");
            }
        }

        if (tileMapData.tiles.Count == 0)
        {
            Debug.LogWarning($"No tiles to save for tilemap: {filePath}");
            return;
        }

        string json = JsonUtility.ToJson(tileMapData, true);
        File.WriteAllText(fullPath, json);
        Debug.Log($"Tilemap saved to {fullPath} with {tileMapData.tiles.Count} tiles.");
    }

    private void LoadTileMap(Tilemap tilemap, string filePath)
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is null. Cannot load tilemap.");
            return;
        }


        string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            TileMapData tileMapData = JsonUtility.FromJson<TileMapData>(json);

            if (tileMapData == null || tileMapData.tiles == null || tileMapData.tiles.Count == 0)
            {
                Debug.LogWarning($"Tilemap data is null or empty in file: {fullPath}");
                return;
            }

            tilemap.ClearAllTiles(); // 清除现有瓦片

            foreach (var tileData in tileMapData.tiles)
            {
                Vector3Int position = new Vector3Int(tileData.x, tileData.y, 0);

                // 尝试从Resources加载Tile
                TileBase tile = Resources.Load<TileBase>(tileData.tileName);

                if (tile == null)
                {
                    // 如果直接加载失败，尝试从已分配的RuleTile匹配
                    if (tilemap == oceanTileMap && oceanRuleTile != null && oceanRuleTile.name == tileData.tileName)
                    {
                        tile = oceanRuleTile;
                    }
                    else if (tilemap == landTileMap && landRuleTile != null && landRuleTile.name == tileData.tileName)
                    {
                        tile = landRuleTile;
                    }
                }

                if (tile != null)
                {
                    tilemap.SetTile(position, tile);
                }
                else
                {
                    Debug.LogWarning($"Failed to load tile: {tileData.tileName} at position {position}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Tilemap file not found at path: {fullPath}");
        }
    }
}