using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AmmoManager ammoManager; // Reference to AmmoManager
    public EnemyManager enemyManager; // Reference to EnemyManager
    public NavalBaseController navalBaseController; // Reference to NavalBaseController

    private List<Vector3> savedAmmoPositions = new List<Vector3>();
    private List<string> savedAmmoTargetTags = new List<string>(); // Save ammo target tags
    private List<Vector3> savedAmmoDirections = new List<Vector3>(); // Save ammo directions
    private List<Vector3> savedEnemyPositions = new List<Vector3>();
    private List<int> savedEnemyHealth = new List<int>(); // Save enemy health
    private int savedGold;
    private int savedNavalBaseHealth; // Save naval base health

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
        // Save ammo positions, target tags, and directions
        savedAmmoPositions.Clear();
        savedAmmoTargetTags.Clear();
        savedAmmoDirections.Clear();
        foreach (GameObject ammo in ammoManager.GetActiveAmmo())
        {
            if (ammo != null)
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

        // Save enemy positions and health
        savedEnemyPositions.Clear();
        savedEnemyHealth.Clear();
        foreach (GameObject enemy in enemyManager.GetActiveEnemies())
        {
            if (enemy != null)
            {
                savedEnemyPositions.Add(enemy.transform.position);
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    savedEnemyHealth.Add(enemyScript.health);
                }
            }
        }

        // Save naval base gold and health
        savedGold = navalBaseController.gold;
        savedNavalBaseHealth = navalBaseController.health;

        // Create a save object
        GameData saveData = new GameData
        {
            ammoPositions = savedAmmoPositions,
            ammoTargetTags = savedAmmoTargetTags,
            ammoDirections = savedAmmoDirections,
            enemyPositions = savedEnemyPositions,
            enemyHealth = savedEnemyHealth,
            navalBaseGold = savedGold,
            navalBaseHealth = savedNavalBaseHealth
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
                GameObject enemy = Instantiate(enemyManager.enemyPrefab, position, Quaternion.identity);
                enemyManager.RegisterEnemy(enemy);

                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null && i < loadedData.enemyHealth.Count)
                {
                    enemyScript.health = loadedData.enemyHealth[i];
                    enemyScript.UpdateHealthUI(); // Update health UI
                }
            }

            // Reload naval base gold and health
            navalBaseController.gold = loadedData.navalBaseGold;
            navalBaseController.health = loadedData.navalBaseHealth;
            navalBaseController.UpdateGoldUI(); // Update gold UI
            navalBaseController.UpdateHealthUI(); // Update health UI
        }
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
    public int navalBaseGold; // Save naval base gold
    public int navalBaseHealth; // Save naval base health
}
