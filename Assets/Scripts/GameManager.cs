using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AmmoManager ammoManager; // Reference to AmmoManager
    public EnemyManager enemyManager; // Reference to EnemyManager
    public NavalBaseController navalBaseController; // Reference to NavalBaseController
    public PlayerShipManager playerShipManager; // 引用 PlayerShipManager

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

    private string saveFilePath;
    private float gameTime; // Track the elapsed game time

    void Start()
    {
        saveFilePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        LoadGame(); // Load game data when the game starts
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
            gameTime = gameTime, // Save game time
            playerShips = playerShipDataList
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
                GameObject ammo = Instantiate(ammoManager.ammoPrefab, loadedData.ammoPositions[i], Quaternion.identity);
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
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
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

            // Reload player ships
            foreach (GameObject ship in playerShipManager.playerShips) // 從 PlayerShipManager 獲取玩家船隻列表
            {
                Destroy(ship); // 清除現有的玩家船隻
            }
            playerShipManager.playerShips.Clear();

            foreach (PlayerShipData shipData in loadedData.playerShips)
            {
                GameObject newShip = Instantiate(playerShipManager.ShipPrefab, shipData.position, Quaternion.identity); // 使用 ShipPrefab
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

    private void UpdateGameTimeUI()
    {
        if (gameTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            gameTimeText.text = $"Time: {minutes:D2}:{seconds:D2}"; // Format as MM:SS
        }
    }

    public void GoBackToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
}

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
    public float gameTime; // Save game time
    public List<PlayerShipData> playerShips; // 保存多個玩家船隻
}

[System.Serializable]
public class PlayerShipData
{
    public Vector3 position; // 玩家船隻位置
    public float health; // 玩家船隻生命值
}

