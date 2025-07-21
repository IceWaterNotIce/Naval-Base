using UnityEngine;

public class PlayerShip : Warship
{
    private LayerMask m_enemyLayer;
    public Transform firePoint;            // 子彈發射點
    public GameObject ammoPrefab;          // 子彈預製體

    new void Awake()
    {
        m_enemyLayer = LayerMask.GetMask("Enemy"); // 獲取敵人船隻的 LayerMask
    }

    protected override void Update()
    {
        base.Update(); // Call base class Update for movement and health UI updates
    }

    public void PerformAttack()
    {
        // 實現玩家攻擊邏輯
        Debug.Log("PlayerShip is performing an attack.");
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

    private void Fire()
    {
        if (ammoPrefab != null && firePoint != null)
        {
            Instantiate(ammoPrefab, firePoint.position, firePoint.rotation);
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"PlayerShip {name} clicked."); // Log when the ship is clicked

        // Try to find the UI in the scene
        PlayerShipUI controlUI = FindFirstObjectByType<PlayerShipUI>();
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
                    controlUI = uiInstance.GetComponent<PlayerShipUI>();
                    Debug.Log("PlayerShipUI loaded from Resources and added to Canvas.");
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
