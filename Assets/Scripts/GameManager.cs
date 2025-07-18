using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#region GameManagerClass
public class GameManager : MonoBehaviour
{
    #region Fields
    public AmmoManager ammoManager; // Reference to AmmoManager
    public EnemyManager enemyManager; // Reference to EnemyManager
    public NavalBaseController navalBaseController; // Reference to NavalBaseController
    public PlayerShipManager playerShipManager; // 引用 PlayerShipManager
    public InfiniteTileMap infiniteTileMap; // Reference to InfiniteTileMap
    public DebugPanelController debugPanelController; // Reference to DebugPanelController
    public CoastalTurretManager coastalTurretManager; // <--- 新增 CoastalTurretManager 參考
    public DockManager dockManager; // 新增：DockManager 參考

    public Text gameTimeText; // Reference to the UI Text element for game time display

    private List<Vector3> savedAmmoPositions = new List<Vector3>();
    private List<string> savedAmmoTargetTags = new List<string>(); // Save ammo target tags
    private List<Vector3> savedAmmoDirections = new List<Vector3>(); // Save ammo directions
    private List<Vector3> savedEnemyPositions = new List<Vector3>();
    private List<int> savedEnemyHealth = new List<int>(); // Save enemy health
    private List<int> savedEnemyPrefabIndices = new List<int>(); // Save enemy prefab indices
    private int savedGold;
    private int savedNavalBaseHealth; // Save naval base health
    private int savedNavalBaseMaxHealth; // Save naval base max health
    private int savedNavalBaseLevel; // Save naval base level
    private int savedNavalBaseLevelUpGoldCost; // Save level-up gold cost
    private Vector3 savedNavalBasePosition; // Save naval base position
    private List<Vector3> savedCoastalTurretPositions = new List<Vector3>(); // 新增：儲存砲塔位置
    private List<Vector3> savedDockPositions = new List<Vector3>(); // 新增：儲存碼頭位置

    private string saveFilePath;
    private float gameTime; // Track the elapsed game time
    #endregion

    #region UnityMethods
    void Start()
    {
        saveFilePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        // 延遲一幀再載入，確保 CoastalTurretManager 已初始化
        StartCoroutine(LoadGameDelayed());
    }

    private System.Collections.IEnumerator LoadGameDelayed()
    {
        yield return null; // 等待一幀
        LoadGame();

        // 強制再等一幀後重建砲塔，確保 CoastalTurretManager 已經初始化
        yield return null;
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(json);

            // 清除現有砲塔
            foreach (var turret in FindObjectsByType<CoastalTurret>(FindObjectsSortMode.None))
            {
                Destroy(turret.gameObject);
            }

            // 重新生成砲塔
            if (coastalTurretManager != null)
            {
                if (loadedData.coastalTurretPositions != null)
                {
                    foreach (var pos in loadedData.coastalTurretPositions)
                    {
                        if (coastalTurretManager.coastalTurretPrefab != null)
                        {
                            Instantiate(
                                coastalTurretManager.coastalTurretPrefab,
                                pos,
                                Quaternion.identity,
                                coastalTurretManager.transform
                            );
                        }
                    }
                }
            }

            // 清除現有 Dock
            foreach (var dock in GameObject.FindGameObjectsWithTag("Dock"))
            {
                Destroy(dock);
            }
            // 重新生成 Dock
            if (dockManager != null && dockManager.dockPrefab != null && loadedData.dockPositions != null)
            {
                foreach (var pos in loadedData.dockPositions)
                {
                    Instantiate(dockManager.dockPrefab, pos, Quaternion.identity);
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        SaveGame(); // Save game data when the application quits
    }

    void Update()
    {
        gameTime += Time.deltaTime; // Increment game time
        UpdateGameTimeUI(); // Update the game time UI
    }
    #endregion

    #region SaveLoad
    public void SaveGame()
    {
        // Save ammo positions, target tags, and directions
        savedAmmoPositions.Clear();
        savedAmmoTargetTags.Clear();
        savedAmmoDirections.Clear();
        foreach (GameObject ammo in ammoManager.GetActiveAmmo())
        {
            if (ammo != null) // Check if ammo is not null
            {
                savedAmmoPositions.Add(ammo.transform.position);
                Ammo ammoScript = ammo.GetComponent<Ammo>();
                if (ammoScript != null)
                {
                    savedAmmoTargetTags.Add(ammoScript.targetTag);
                    savedAmmoDirections.Add(ammoScript.GetDirection());
                }
            }
        }

        // Save enemy positions, health, and prefab indices
        savedEnemyPositions.Clear();
        savedEnemyHealth.Clear();
        savedEnemyPrefabIndices.Clear();
        foreach (GameObject enemy in enemyManager.GetActiveEnemies())
        {
            if (enemy != null) // Check if enemy is not null
            {
                savedEnemyPositions.Add(enemy.transform.position);
                EnemyShip enemyScript = enemy.GetComponent<EnemyShip>();
                if (enemyScript != null)
                {
                    savedEnemyHealth.Add((int)enemyScript.Health); // 使用 Health 屬性
                }

                int prefabIndex = enemyManager.enemyPrefabs.IndexOf(enemyScript?.gameObject);
                savedEnemyPrefabIndices.Add(prefabIndex >= 0 ? prefabIndex : 0); // 保存 prefab 索引
            }
        }

        // Save naval base gold and health
        savedGold = navalBaseController.gold;
        savedNavalBaseHealth = navalBaseController.health;
        savedNavalBaseMaxHealth = navalBaseController.maxHealth; // Save max health

        // Save naval base level and level-up gold cost
        savedNavalBaseLevel = navalBaseController.level;
        savedNavalBaseLevelUpGoldCost = navalBaseController.levelUpGoldCost;

        // Save naval base position
        savedNavalBasePosition = navalBaseController.GetPosition(); // Save naval base position

        // Save player ships' positions and health
        List<PlayerShipData> playerShipDataList = new List<PlayerShipData>();
        foreach (GameObject ship in playerShipManager.playerShips) // 從 PlayerShipManager 獲取玩家船隻列表
        {
            if (ship != null) // Check if ship is not null
            {
                PlayerShip playerShipScript = ship.GetComponent<PlayerShip>();
                if (playerShipScript != null)
                {
                    playerShipDataList.Add(new PlayerShipData
                    {
                        position = ship.transform.position,
                        health = playerShipScript.Health
                    });
                }
            }
        }

        // Save coastal turret positions
        savedCoastalTurretPositions.Clear();
        foreach (var turret in FindObjectsOfType<CoastalTurret>())
        {
            savedCoastalTurretPositions.Add(turret.transform.position);
        }

        // Save dock positions
        savedDockPositions.Clear();
        foreach (var dock in GameObject.FindGameObjectsWithTag("Dock"))
        {
            savedDockPositions.Add(dock.transform.position);
        }

        // Save naval base tiles
        List<NavalBaseTileData> navalBaseTiles = infiniteTileMap != null
            ? infiniteTileMap.GetAllNavalBaseTilePositions()
            : new List<NavalBaseTileData>();

        // Save chunk info
        List<Vector2Int> landChunks = new List<Vector2Int>();
        List<Vector2Int> oceanChunks = new List<Vector2Int>();
        if (infiniteTileMap != null)
        {
            landChunks.AddRange(infiniteTileMap.loadedLandChunks);
            oceanChunks.AddRange(infiniteTileMap.loadedOceanChunks);
        }

        // Create a save object
        GameData saveData = new GameData
        {
            ammoPositions = savedAmmoPositions,
            ammoTargetTags = savedAmmoTargetTags,
            ammoDirections = savedAmmoDirections,
            enemyPositions = savedEnemyPositions,
            enemyHealth = savedEnemyHealth,
            enemyPrefabIndices = savedEnemyPrefabIndices,
            navalBaseGold = savedGold,
            navalBaseHealth = savedNavalBaseHealth,
            navalBaseMaxHealth = savedNavalBaseMaxHealth, // Save max health
            navalBaseLevel = savedNavalBaseLevel, // Save naval base level
            navalBaseLevelUpGoldCost = savedNavalBaseLevelUpGoldCost, // Save level-up gold cost
            gameTime = gameTime, // Save game time
            playerShips = playerShipDataList,
            navalBasePosition = savedNavalBasePosition, // Save naval base position
            coastalTurretPositions = savedCoastalTurretPositions, // 新增
            navalBaseTiles = navalBaseTiles, // 新增
            dockPositions = savedDockPositions, // 新增
            landChunks = landChunks, // 新增
            oceanChunks = oceanChunks // 新增
        };

        // Serialize and save to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json); // Save JSON to file
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            // Read and deserialize the file
            string json = File.ReadAllText(saveFilePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(json);

            // Clear current ammo and enemies
            ammoManager.ClearAmmo();
            enemyManager.ClearEnemies();

            // Reload ammo
            for (int i = 0; i < loadedData.ammoPositions.Count; i++)
            {
                GameObject ammo = Instantiate(ammoManager.ammoPrefab, loadedData.ammoPositions[i], Quaternion.identity, ammoManager.transform); // Instantiate ammo with parent
                ammoManager.RegisterAmmo(ammo);

                Ammo ammoScript = ammo.GetComponent<Ammo>();
                if (ammoScript != null && i < loadedData.ammoTargetTags.Count && i < loadedData.ammoDirections.Count)
                {
                    ammoScript.targetTag = loadedData.ammoTargetTags[i];
                    ammoScript.SetDirection(loadedData.ammoDirections[i]);
                }
            }

            // Reload enemies and their health
            for (int i = 0; i < loadedData.enemyPositions.Count; i++)
            {
                Vector3 position = loadedData.enemyPositions[i];
                int prefabIndex = i < loadedData.enemyPrefabIndices.Count ? loadedData.enemyPrefabIndices[i] : 0; // 默認索引為 0
                GameObject enemyPrefab = enemyManager.enemyPrefabs[Mathf.Clamp(prefabIndex, 0, enemyManager.enemyPrefabs.Count - 1)];
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, enemyManager.transform); // Instantiate enemy with parent
                enemyManager.RegisterEnemy(enemy);

                EnemyShip enemyScript = enemy.GetComponent<EnemyShip>();
                if (enemyScript != null && i < loadedData.enemyHealth.Count)
                {
                    enemyScript.Health = loadedData.enemyHealth[i]; // 使用 Health 屬性
                    enemyScript.UpdateHealthUI(); // 調用公開的 UpdateHealthUI 方法
                }
            }

            // Reload naval base gold and health
            navalBaseController.gold = loadedData.navalBaseGold;
            navalBaseController.health = loadedData.navalBaseHealth;
            navalBaseController.maxHealth = loadedData.navalBaseMaxHealth; // Reload max health
            navalBaseController.UpdateGoldUI(); // Update gold UI
            navalBaseController.UpdateHealthUI(); // Update health UI

            // Reload naval base level and ensure it is at least 1
            navalBaseController.level = Mathf.Max(loadedData.navalBaseLevel, 1);
            navalBaseController.levelUpGoldCost = loadedData.navalBaseLevelUpGoldCost;
            navalBaseController.UpdateLevelUI(); // Update level UI

            // Reload naval base position
            navalBaseController.SetPosition(loadedData.navalBasePosition); // Reload naval base position


            // Clear existing coastal turrets
            foreach (var turret in FindObjectsOfType<CoastalTurret>())
            {
                Destroy(turret.gameObject);
            }

            // 重新生成砲塔
            if (coastalTurretManager != null)
            {
                if (loadedData.coastalTurretPositions != null)
                {
                    foreach (var pos in loadedData.coastalTurretPositions)
                    {
                        if (coastalTurretManager.coastalTurretPrefab != null)
                        {
                            Instantiate(
                                coastalTurretManager.coastalTurretPrefab,
                                pos,
                                Quaternion.identity,
                                coastalTurretManager.transform
                            );
                        }
                    }
                }
            }

            // 清除現有 Dock
            foreach (var dock in GameObject.FindGameObjectsWithTag("Dock"))
            {
                Destroy(dock);
            }
            // 重新生成 Dock
            if (dockManager != null && dockManager.dockPrefab != null && loadedData.dockPositions != null)
            {
                foreach (var pos in loadedData.dockPositions)
                {
                    Instantiate(dockManager.dockPrefab, pos, Quaternion.identity);
                }
            }

            // 還原海軍基地瓦片
            if (infiniteTileMap != null)
            {
                infiniteTileMap.SetNavalBaseTilesFromPositions(loadedData.navalBaseTiles);
            }

            // 還原 chunk 資訊
            if (infiniteTileMap != null && loadedData.landChunks != null && loadedData.oceanChunks != null)
            {
                infiniteTileMap.loadedLandChunks.Clear();
                infiniteTileMap.loadedOceanChunks.Clear();
                foreach (var c in loadedData.landChunks)
                    infiniteTileMap.loadedLandChunks.Add(c);
                foreach (var c in loadedData.oceanChunks)
                    infiniteTileMap.loadedOceanChunks.Add(c);
            }

            // Reload player ships
            foreach (GameObject ship in playerShipManager.playerShips) // 從 PlayerShipManager 獲取玩家船隻列表
            {
                Destroy(ship); // 清除現有的玩家船隻
            }
            playerShipManager.playerShips.Clear();

            foreach (PlayerShipData shipData in loadedData.playerShips)
            {
                GameObject newShip = Instantiate(playerShipManager.ShipPrefab, shipData.position, Quaternion.identity, playerShipManager.transform); // 使用 ShipPrefab
                PlayerShip playerShipScript = newShip.GetComponent<PlayerShip>();
                if (playerShipScript != null)
                {
                    playerShipScript.Health = shipData.health;
                    playerShipScript.UpdateHealthUI();
                }
                playerShipManager.playerShips.Add(newShip); // 添加到 PlayerShipManager 的玩家船隻列表
            }

            // Reload game time
            gameTime = loadedData.gameTime;
        }
    }
    #endregion

    #region UI
    private void UpdateGameTimeUI()
    {
        if (gameTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            gameTimeText.text = $"Time: {minutes:D2}:{seconds:D2}"; // Format as MM:SS
        }
    }
    #endregion

    #region SceneControl
    public void GoBackToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
    #endregion

    #region GameFlow
    public void StartNewGame()
    {
        // 清除彈藥、敵人、玩家船、砲塔、海軍基地瓦片
        ammoManager.ClearAmmo();
        enemyManager.ClearEnemies();

        foreach (var ship in playerShipManager.playerShips)
        {
            if (ship != null) Destroy(ship);
        }
        playerShipManager.playerShips.Clear();

        foreach (var turret in FindObjectsOfType<CoastalTurret>())
        {
            Destroy(turret.gameObject);
        }

        // 新增：清除所有 Dock
        foreach (var dock in GameObject.FindGameObjectsWithTag("Dock"))
        {
            Destroy(dock);
        }

        if (infiniteTileMap != null && infiniteTileMap.navalBaseTileMap != null)
        {
            infiniteTileMap.navalBaseTileMap.ClearAllTiles();
        }

        // 新增：重設海軍基地位置為 (0,0)，以觸發自動購買瓦片
        if (navalBaseController != null)
        {
            navalBaseController.SetPosition(Vector3.zero);
        }

        // 重設資源與狀態
        navalBaseController.gold = 0;
        navalBaseController.health = navalBaseController.maxHealth;
        navalBaseController.level = 1;
        navalBaseController.levelUpGoldCost = 100;
        navalBaseController.UpdateGoldUI();
        navalBaseController.UpdateHealthUI();
        navalBaseController.UpdateLevelUI();

        // 重設遊戲時間
        gameTime = 0f;
        UpdateGameTimeUI();

        // 讓 InfiniteTileMap 處理海軍基地自動移動到最近陸地
        if (infiniteTileMap != null)
        {
            infiniteTileMap.StartCoroutine("WaitForMapRenderAndSetNavalBase");
        }

        // 清空 chunk 資訊
        if (infiniteTileMap != null)
        {
            infiniteTileMap.loadedLandChunks.Clear();
            infiniteTileMap.loadedOceanChunks.Clear();
        }

        // 其他初始化邏輯可依需求補充
    }
    #endregion
}
#endregion

#region GameDataClasses
[System.Serializable]
public class GameData
{
    public List<Vector3> ammoPositions;
    public List<string> ammoTargetTags; // Save ammo target tags
    public List<Vector3> ammoDirections; // Save ammo directions
    public List<Vector3> enemyPositions;
    public List<int> enemyHealth; // Save enemy health
    public List<int> enemyPrefabIndices; // Save enemy prefab indices
    public int navalBaseGold; // Save naval base gold
    public int navalBaseHealth; // Save naval base health
    public int navalBaseMaxHealth; // Save naval base max health
    public int navalBaseLevel; // Save naval base level
    public int navalBaseLevelUpGoldCost; // Save level-up gold cost
    public float gameTime; // Save game time
    public List<PlayerShipData> playerShips; // 保存多個玩家船隻
    public Vector3 navalBasePosition; // Save naval base position
    public List<Vector3> coastalTurretPositions; // 新增：儲存砲塔位置
    public List<NavalBaseTileData> navalBaseTiles; // 新增：儲存海軍基地瓦片
    public List<Vector3> dockPositions; // 新增：儲存碼頭位置
    public List<Vector2Int> landChunks; // 新增：儲存已載入的陸地 chunk
    public List<Vector2Int> oceanChunks; // 新增：儲存已載入的海洋 chunk
}

[System.Serializable]
public class PlayerShipData
{
    public Vector3 position; // 玩家船隻位置
    public float health; // 玩家船隻生命值
}
#endregion

