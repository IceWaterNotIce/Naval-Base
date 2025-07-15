using UnityEngine;

public class PlayerShip : Warship
{
    private LayerMask m_enemyLayer; // 用於檢測敵人的圖層
    private float m_detectionRadius = 10f; // 檢測敵人的半徑

    protected override void Update()
    {
        base.Update(); // Call base class Update for movement and health UI updates
    }

    protected override void PerformAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, m_detectionRadius, m_enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null)
            {
                ShootAmmoAtEnemy(enemy.transform);
            }
        }
    }

    private void ShootAmmoAtEnemy(Transform enemy)
    {
        if (ammoPrefab != null && firePoint != null)
        {
            AmmoManager ammoManager = FindFirstObjectByType<AmmoManager>(); // 獲取 AmmoManager 實例
            GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation, ammoManager.transform);
            if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript)) // 確保子彈有 Ammo 組件
            {
                ammoScript.SetTarget(enemy.position, "Enemy"); // 設置子彈目標
                ammoScript.OnHitEnemy += () => GainExperience(10); // 擊中敵人時獲得經驗值
                Debug.Log($"PlayerShip shot at enemy {enemy.name} at position {enemy.position}"); // Debug log
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"PlayerShip {name} clicked."); // Log when the ship is clicked

        // Try to find the UI in the scene
        PlayerShipControlUI controlUI = FindFirstObjectByType<PlayerShipControlUI>();
        if (controlUI == null)
        {
            // If not found, load from Resources and add to UI Canvas
            GameObject uiPrefab = Resources.Load<GameObject>("UI/PlayerShipControlUI");
            if (uiPrefab != null)
            {
                Canvas uiCanvas = FindFirstObjectByType<Canvas>();
                if (uiCanvas != null)
                {
                    GameObject uiInstance = Instantiate(uiPrefab, uiCanvas.transform);
                    controlUI = uiInstance.GetComponent<PlayerShipControlUI>();
                    Debug.Log("PlayerShipControlUI loaded from Resources and added to Canvas.");
                }
                else
                {
                    Debug.LogWarning("UI Canvas not found in the scene!");
                }
            }
            else
            {
                Debug.LogWarning("PlayerShipControlUI prefab not found in Resources!");
            }
        }

        if (controlUI != null)
        {
            Debug.Log("PlayerShipControlUI found. Selecting ship."); // Debug log
            controlUI.SelectShip(this); // Notify the control UI that this ship is selected
        }
        else
        {
            Debug.LogWarning("PlayerShipControlUI not found or failed to load!"); // Warn if the control UI is missing
        }
    }
}

