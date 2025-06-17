using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AmmoManager ammoManager; // Reference to AmmoManager
    public EnemyManager enemyManager; // Reference to EnemyManager
    public NavalBaseController navalBaseController; // Reference to NavalBaseController

    private List<Vector3> savedAmmoPositions = new List<Vector3>();
    private List<Vector3> savedEnemyPositions = new List<Vector3>();
    private int savedGold;

    private string saveFilePath;

    void Start()
    {
        saveFilePath = Path.Combine(Application.streamingAssetsPath, "GameData.json");
        LoadGame(); // Load game data when the game starts
    }

    void OnApplicationQuit()
    {
        SaveGame(); // Save game data when the game quits
    }

    public void SaveGame()
    {
        // Save ammo positions
        savedAmmoPositions.Clear();
        foreach (GameObject ammo in ammoManager.GetActiveAmmo())
        {
            if (ammo != null)
            {
                savedAmmoPositions.Add(ammo.transform.position);
            }
        }

        // Save enemy positions
        savedEnemyPositions.Clear();
        foreach (GameObject enemy in enemyManager.GetActiveEnemies())
        {
            if (enemy != null)
            {
                savedEnemyPositions.Add(enemy.transform.position);
            }
        }

        // Save naval base gold
        savedGold = navalBaseController.gold;

        // Create a save object
        GameData saveData = new GameData
        {
            ammoPositions = savedAmmoPositions,
            enemyPositions = savedEnemyPositions,
            navalBaseGold = savedGold
        };

        // Serialize and save to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
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
            foreach (Vector3 position in loadedData.ammoPositions)
            {
                GameObject ammo = Instantiate(ammoManager.ammoPrefab, position, Quaternion.identity);
                ammoManager.RegisterAmmo(ammo);
            }

            // Reload enemies
            foreach (Vector3 position in loadedData.enemyPositions)
            {
                GameObject enemy = Instantiate(enemyManager.enemyPrefab, position, Quaternion.identity);
                enemyManager.RegisterEnemy(enemy);
            }

            // Reload naval base gold
            navalBaseController.gold = loadedData.navalBaseGold;
        }
    }
}

[System.Serializable]
public class GameData
{
    public List<Vector3> ammoPositions;
    public List<Vector3> enemyPositions;
    public int navalBaseGold; // Save naval base gold
}
