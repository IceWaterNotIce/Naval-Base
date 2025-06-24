using UnityEngine;

public class PlayerShip : Ship
{
    public Transform firePoint; // 子彈發射點
    public LayerMask enemyLayer; // 用於檢測敵人的圖層
    public float detectionRadius = 10f; // 檢測敵人的半徑

    protected override void Update()
    {
        // Ensure currentSpeed is set for movement
        if (Input.GetKey(KeyCode.W)) // Example: Move forward when 'W' is pressed
        {
            currentSpeed = maxSpeed;
        }
        else if (Input.GetKey(KeyCode.S)) // Example: Stop when 'S' is pressed
        {
            currentSpeed = 0f;
        }

        base.Update(); // Call base class Update for movement and health UI updates
        HandleAttack(); // 處理攻擊邏輯
    }

    private void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            DetectAndShootEnemies();
            attackTimer = 0f; // 重置攻擊計時器
        }
    }

    private void DetectAndShootEnemies()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
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
            GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation);
            if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript)) // 確保子彈有 Ammo 組件
            {
                ammoScript.SetTarget(enemy.position, "Enemy"); // 設置子彈目標
                Debug.Log($"PlayerShip shot at enemy {enemy.name} at position {enemy.position}"); // Debug log
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"PlayerShip {name} clicked."); // Log when the ship is clicked
        PlayerShipControlUI controlUI = FindFirstObjectByType<PlayerShipControlUI>(); // Use the updated method
        if (controlUI != null)
        {
            Debug.Log("PlayerShipControlUI found. Selecting ship."); // Debug log
            controlUI.SelectShip(this); // Notify the control UI that this ship is selected
        }
        else
        {
            Debug.LogWarning("PlayerShipControlUI not found!"); // Warn if the control UI is missing
        }
    }
}
