using UnityEngine;
using UnityEngine.UI;

public class EnemyShip : Ship
{
    public float minDistanceToNavalBase = 3f; // Minimum distance to the naval base
    public float orbitSpeed = 1f; // Speed at which the enemy orbits the naval base
    public int goldReward = 1; // Gold rewarded when the enemy dies
    public Transform firePoint; // Point where ammo is fired
    public float playerShipDetectionRadius = 10f; // 半徑內檢測玩家船隻

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Initialize(string name, float speed, float health)
    {
        base.Initialize(name, speed, health);
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        UpdateTarget(); // 更新目標
        UpdateHealthUI(); // Initialize health display
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        SetCanvasEventCamera(); // Call method from Ship class
        UpdateHealthUI(); // Call method from Ship class
    }
    protected override void Update()
    {
        base.Update(); // Call base class Update for movement and health UI updates
        UpdateTarget(); // 更新目標
        MoveTowardsTarget();
        HandleAttack(); // 處理攻擊邏輯
    }

    private void UpdateTarget()
    {
        // 優先檢測最近的玩家船隻
        Collider2D[] playerShips = Physics2D.OverlapCircleAll(transform.position, playerShipDetectionRadius, LayerMask.GetMask("PlayerShip"));
        Transform closestPlayerShip = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D ship in playerShips)
        {
            float distance = Vector3.Distance(transform.position, ship.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayerShip = ship.transform;
            }
        }

        if (closestPlayerShip != null)
        {
            target = closestPlayerShip; // 設置目標為最近的玩家船隻
        }
        else
        {
            target = GameObject.FindWithTag("NavalBase")?.transform; // 如果沒有玩家船隻，目標為 NavalBase
        }
    }

    private void MoveTowardsTarget()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > minDistanceToNavalBase && distance <= 5f) // Within attack range but not too close
            {
                OrbitAroundNavalBase(); // Move around the naval base
            }
            else if (distance > 5f) // Too far from the naval base
            {
                Vector3 direction = (target.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                targetAzimuthAngle = Mathf.Repeat(targetAngle, 360f); // 確保範圍在 0 到 360
                targetSpeed = maxSpeed; // 使用基類的速度邏輯
            }
            else // Too close to the naval base
            {
                Vector3 direction = (transform.position - target.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                targetAzimuthAngle = Mathf.Repeat(targetAngle, 360f); // 確保範圍在 0 到 360
                targetSpeed = maxSpeed; // 使用基類的速度邏輯
            }
        }
    }

    private void OrbitAroundNavalBase()
    {
        if (target != null)
        {
            Vector3 direction = (transform.position - target.position).normalized;

            // Calculate orbit direction (perpendicular to the direction to the naval base)
            Vector3 orbitDirection = new Vector3(-direction.y, direction.x, 0).normalized;
            float targetAngle = Mathf.Atan2(orbitDirection.y, orbitDirection.x) * Mathf.Rad2Deg;

            targetAzimuthAngle = Mathf.Repeat(targetAngle, 360f); // 確保範圍在 0 到 360
            targetSpeed = orbitSpeed; // 使用基類的速度邏輯
        }
    }

    private void HandleAttack()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= 5f) // Check if within attack range
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                ShootAmmoAtTarget(); // Shoot ammo at the current target
                attackTimer = 0f; // Reset attack timer
            }
        }
    }

    private void ShootAmmoAtTarget()
    {
        if (ammoPrefab != null && firePoint != null && target != null)
        {
            GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation);
            if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript)) // Ensure Ammo component exists
            {
                string targetTag = target.CompareTag("PlayerShip") ? "PlayerShip" : "NavalBase"; // Determine target tag
                ammoScript.SetTarget(target.position, targetTag); // Pass target position and tag
                Debug.Log($"Ammo shot at {targetTag} with target position: {target.position}"); // Debug log
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (Health <= 0)
        {
            RewardPlayer(); // Handle enemy death
        }
    }

    private void RewardPlayer()
    {
        NavalBaseController navalBase = GameObject.FindWithTag("NavalBase").GetComponent<NavalBaseController>();
        if (navalBase != null)
        {
            navalBase.AddGold(goldReward); // Add gold to the naval base
        }
    }
}

