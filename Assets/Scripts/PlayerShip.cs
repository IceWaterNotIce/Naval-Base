using UnityEngine;

public class PlayerShip : Ship
{
    public Transform firePoint; // 子彈發射點
    public LayerMask enemyLayer; // 用於檢測敵人的圖層
    public float detectionRadius = 10f; // 檢測敵人的半徑

    void Update()
    {
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
}
