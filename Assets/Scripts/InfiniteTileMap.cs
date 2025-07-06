using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#region DataClasses
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

[System.Serializable]
public class NavalBaseTileData
{
    public int x;
    public int y;
}
#endregion

#region InfiniteTileMapClass
public class InfiniteTileMap : MonoBehaviour
{
    #region Fields
    public Tilemap oceanSavedTileMap; // Tilemap to store rendered ocean tiles
    public Tilemap landSavedTileMap; // Tilemap to store rendered land tiles
    public TileBase oceanRuleTile; // Ocean Rule Tile to use
    public TileBase landRuleTile; // Land Rule Tile to use
    public Tilemap navalBaseTileMap; // Tilemap for naval base tiles
    public TileBase navalBaseTile; // Tile to represent purchased tiles
    public int tileSize = 10; // Size of each tile
    public int viewDistance = 3; // Number of tiles visible around the player
    public Transform navalBase; // Reference to the naval base
    public PlayerShipManager playerShipManager; // Reference to PlayerShipManager
    public int landSeed = 42; // Seed for land generation
    public float landThreshold = 0.5f; // Threshold for land generation
    public float islandScale = 0.1f; // 控制島嶼大小的縮放因子
    public float islandSpacing = 0.2f; // 控制島嶼之間距離的縮放因子
    public int landTileCost = 50; // Cost to buy a land tile
    public int oceanTileCost = 30; // Cost to buy an ocean tile
    public NavalBaseController navalBaseController; // Reference to NavalBaseController

    public string oceanTileMapSavePath = "OceanTileMap.json"; // Path to save ocean tilemap
    public string landTileMapSavePath = "LandTileMap.json"; // Path to save land tilemap

    private HashSet<Vector2Int> activeTiles = new HashSet<Vector2Int>();

    [Header("Coastal Turret Settings")]
    public GameObject coastalTurretPrefab; // 沿海砲塔預製體
    public int coastalTurretCost = 100; // 建造砲塔所需金幣

    [Header("UI")]
    public Button buyNavalBaseTileButton; // 參考購買海軍基地瓦片的按鈕

    private bool isWaitingForTileClick = false; // 是否等待玩家點擊地圖

    public int chunkSize = 8; // 每個 chunk 的 tile 數量（例如 8x8）
    // 已載入的 chunk 記錄（key: chunk座標, value: true）
    public HashSet<Vector2Int> loadedLandChunks = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> loadedOceanChunks = new HashSet<Vector2Int>();
    #endregion

    #region UnityMethods
    void Start()
    {
        if ((navalBase == null && (playerShipManager == null || playerShipManager.playerShips.Count == 0)) || oceanSavedTileMap == null || oceanRuleTile == null)
        {
            Debug.LogError("Naval Base, PlayerShipManager, Ocean Tilemap, or Ocean Rule Tile reference is missing!");
            return;
        }

        StartCoroutine(WaitForMapRenderAndSetNavalBase()); // Wait for map rendering before setting naval base position
        LoadSavedTileMaps(); // Load saved tilemaps on start
        UpdateTileMap(); // Initialize the tile map

        if (buyNavalBaseTileButton != null)
        {
            buyNavalBaseTileButton.onClick.AddListener(OnBuyNavalBaseTileButtonClicked);
        }
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

        // 處理等待玩家點擊地圖以購買瓦片
        if (isWaitingForTileClick && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = oceanSavedTileMap.WorldToCell(mouseWorldPos); // <--- 修改
            tilePos.z = 0; // 修正 z 軸

            Debug.Log($"[BuyTile Debug] MouseWorldPos: {mouseWorldPos}, TilePos(raw): {oceanSavedTileMap.WorldToCell(mouseWorldPos)}, TilePos(z=0): {tilePos}");

            BuyTile(tilePos);

            isWaitingForTileClick = false; // 完成一次點擊後退出等待狀態
        }
    }
    #endregion

    #region TileMapLogic
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
                    //Debug.Log($"Player ship {ship.name} at tile position {GetTilePosition(ship.transform.position)}");
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
            CreateTile(tilePosition, oceanSavedTileMap, oceanRuleTile); // <--- 修改
        }

        // 新增陸地瓦片
        foreach (var tilePosition in landTilesToAdd)
        {
            CreateTile(tilePosition, landSavedTileMap, landRuleTile); // <--- 修改
        }

        // 移除未使用的瓦片（分開處理海洋和陸地）
        RemoveUnusedTiles(tilesToKeep, oceanSavedTileMap); // <--- 修改
        RemoveUnusedTiles(tilesToKeep, landSavedTileMap); // <--- 修改
    }

    private void CreateTile(Vector2Int tilePosition, Tilemap tilemap, TileBase ruleTile)
    {
        if (tilemap == null || ruleTile == null) return;

        // 計算 chunk 座標
        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt((float)tilePosition.x / chunkSize),
            Mathf.FloorToInt((float)tilePosition.y / chunkSize)
        );

        // 決定是陸地還是海洋
        bool isLand = tilemap == landSavedTileMap; // <--- 修改
        var loadedChunks = isLand ? loadedLandChunks : loadedOceanChunks;
        Tilemap savedTileMap = tilemap; // <--- 直接用傳入的 tilemap

        // 若 chunk 已經載入過，直接返回（避免重複渲染）
        if (loadedChunks.Contains(chunkCoord))
            return;

        // 處理整個 chunk
        for (int dx = 0; dx < chunkSize; dx++)
        {
            for (int dy = 0; dy < chunkSize; dy++)
            {
                Vector2Int pos = new Vector2Int(
                    chunkCoord.x * chunkSize + dx,
                    chunkCoord.y * chunkSize + dy
                );
                Vector3Int tilemapPosition = new Vector3Int(pos.x, pos.y, 0);

                // 先從 savedTileMap 載入
                if (savedTileMap != null)
                {
                    TileBase savedTile = savedTileMap.GetTile(tilemapPosition);
                    if (savedTile != null)
                    {
                        tilemap.SetTile(tilemapPosition, savedTile);
                        activeTiles.Add(pos);
                        continue;
                    }
                }

                // 若無存檔，依規則生成
                if (isLand)
                {
                    float noiseValue = Mathf.PerlinNoise(
                        (pos.x + landSeed) * islandScale * islandSpacing,
                        (pos.y + landSeed) * islandScale * islandSpacing
                    );
                    if (noiseValue > landThreshold)
                    {
                        tilemap.SetTile(tilemapPosition, ruleTile);
                        landSavedTileMap?.SetTile(tilemapPosition, ruleTile);
                    }
                }
                else
                {
                    tilemap.SetTile(tilemapPosition, ruleTile);
                    oceanSavedTileMap?.SetTile(tilemapPosition, ruleTile);
                }
                activeTiles.Add(pos);
            }
        }
        // 標記此 chunk 已載入
        loadedChunks.Add(chunkCoord);
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
    #endregion

    #region TileMapSaveLoad
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
                    // oceanSavedTileMap 和 landSavedTileMap
                    if (tilemap == oceanSavedTileMap && oceanRuleTile != null && oceanRuleTile.name == tileData.tileName)
                    {
                        tile = oceanRuleTile;
                    }
                    else if (tilemap == landSavedTileMap && landRuleTile != null && landRuleTile.name == tileData.tileName)
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
    #endregion

    #region NavalBaseInit
    // 只在遊戲開始時自動移動海軍基地，不在每次載入遊戲時
    private System.Collections.IEnumerator WaitForMapRenderAndSetNavalBase()
    {
        yield return new WaitForEndOfFrame(); // Wait for the end of the frame to ensure map rendering is complete
        // 僅當海軍基地位置為 (0,0) 時才自動移動
        if (navalBase != null && navalBase.position == Vector3.zero)
        {
            SetNavalBaseToNearestLandTile(); // Set naval base position to the nearest land tile
        }
    }

    private void SetNavalBaseToNearestLandTile()
    {
        if (navalBase == null || landSavedTileMap == null) return; // <--- 修改

        // Check if the naval base position is (0, 0)
        if (navalBase.position == Vector3.zero)
        {
            Vector3Int baseTilePosition = landSavedTileMap.WorldToCell(navalBase.position); // <--- 修改
            Vector3Int nearestLandTile = baseTilePosition;

            float shortestDistance = float.MaxValue;

            foreach (var position in landSavedTileMap.cellBounds.allPositionsWithin) // <--- 修改
            {
                if (landSavedTileMap.GetTile(position) != null) // Check if the tile is a land tile
                {
                    float distance = Vector3.Distance(landSavedTileMap.CellToWorld(position), navalBase.position); // <--- 修改
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestLandTile = position;
                    }
                }
            }

            if (shortestDistance < float.MaxValue)
            {
                Vector3 newPosition = landSavedTileMap.CellToWorld(nearestLandTile); // <--- 修改
                newPosition.x += 0.5f; // Adjust for tile center
                newPosition.y += 0.5f; // Adjust for tile center
                navalBase.position = newPosition;
                Debug.Log($"Naval base moved to nearest land tile at {nearestLandTile}");

                // 自動購買該瓦片及其相鄰瓦片
                BuyNavalBaseAndAdjacentTiles(nearestLandTile);
            }
            else
            {
                Debug.LogWarning("No land tiles found near the naval base.");
            }
        }
        else
        {
            Debug.Log("Naval base position is not at (0, 0), no adjustment needed.");
        }
    }

    // 新增：購買中心瓦片及其相鄰瓦片
    private void BuyNavalBaseAndAdjacentTiles(Vector3Int centerTile)
    {
        if (navalBaseTileMap == null || navalBaseTile == null) return;

        Vector3Int[] offsets = new Vector3Int[]
        {
            Vector3Int.zero, // 中心
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (var offset in offsets)
        {
            Vector3Int pos = centerTile + offset;
            // 僅在該位置尚未有navalBaseTile時設置
            if (navalBaseTileMap.GetTile(pos) != navalBaseTile)
            {
                navalBaseTileMap.SetTile(pos, navalBaseTile);
            }
        }
    }
    #endregion

    #region TileBuying
    public void BuyTile(Vector3Int tilePosition)
    {
        tilePosition.z = 0; // 保證 z 軸為 0

        if (navalBaseTileMap == null || navalBaseController == null) return;

        // Check if the tile is adjacent to an existing naval base tile
        if (!IsTileAdjacentToNavalBase(tilePosition))
        {
            Debug.LogWarning("Tile is not adjacent to any naval base tile.");
            return;
        }

        // Determine the cost based on the type of tile
        TileBase existingTile = landSavedTileMap.GetTile(tilePosition) ?? oceanSavedTileMap.GetTile(tilePosition); // <--- 修改
        int tileCost = existingTile == landRuleTile ? landTileCost : oceanTileCost;

        // Check if the player has enough gold
        if (navalBaseController.gold < tileCost)
        {
            Debug.LogWarning("Not enough gold to buy this tile.");
            return;
        }

        // Deduct gold and mark the tile as purchased
        navalBaseController.AddGold(-tileCost);
        navalBaseTileMap.SetTile(tilePosition, navalBaseTile);
        Debug.Log($"Tile purchased at {tilePosition} for {tileCost} gold.");
    }

    private bool IsTileAdjacentToNavalBase(Vector3Int tilePosition)
    {
        Vector3Int[] adjacentOffsets = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (var offset in adjacentOffsets)
        {
            Vector3Int adjacentPosition = tilePosition + offset;
            if (navalBaseTileMap.GetTile(adjacentPosition) != null)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region TurretLogic
    // 判斷指定陸地瓦片是否靠近海洋瓦片
    public bool CanBuildCoastalTurret(Vector3Int landTilePos)
    {
        if (landSavedTileMap.GetTile(landTilePos) != landRuleTile) return false; // 必須是陸地瓦片
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        foreach (var dir in directions)
        {
            Vector3Int neighbor = landTilePos + dir;
            if (oceanSavedTileMap.GetTile(neighbor) == oceanRuleTile) // <--- 修改
                return true;
        }
        return false;
    }
    #endregion

    #region UI
    // 新增：按鈕點擊事件
    private void OnBuyNavalBaseTileButtonClicked()
    {
        isWaitingForTileClick = true;
        Debug.Log("請在地圖上點擊要購買的海軍基地瓦片位置");
    }
    #endregion

    #region NavalBaseTileSaveLoad
    // 取得所有已購買的海軍基地瓦片座標
    public List<NavalBaseTileData> GetAllNavalBaseTilePositions()
    {
        List<NavalBaseTileData> positions = new List<NavalBaseTileData>();
        if (navalBaseTileMap == null) return positions;
        foreach (var pos in navalBaseTileMap.cellBounds.allPositionsWithin)
        {
            if (navalBaseTileMap.GetTile(pos) == navalBaseTile)
            {
                positions.Add(new NavalBaseTileData { x = pos.x, y = pos.y });
            }
        }
        return positions;
    }

    // 根據存檔還原所有已購買的海軍基地瓦片
    public void SetNavalBaseTilesFromPositions(List<NavalBaseTileData> positions)
    {
        if (navalBaseTileMap == null || navalBaseTile == null) return;
        navalBaseTileMap.ClearAllTiles();
        if (positions == null) return;
        foreach (var data in positions)
        {
            navalBaseTileMap.SetTile(new Vector3Int(data.x, data.y, 0), navalBaseTile);
        }
    }
    #endregion
}
#endregion