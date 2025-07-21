using UnityEngine;
using UnityEngine.UI;

public class EnemyShip : Warship
{
    public float minDistanceToNavalBase = 3f; // Minimum distance to the naval base
    public float orbitSpeed = 1f; // Speed at which the enemy orbits the naval base
    public int goldReward = 1; // Gold rewarded when the enemy dies
    public int experienceReward = 20; // 擊殺獎勵經驗值

    public Transform firePoint;            // 子彈發射點
    public GameObject ammoPrefab;          // 子彈預製體

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Initialize(string name, float speed, float health)
    {
        base.Initialize(name, speed, health);
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        UpdateTarget(); // 更新目標
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
    }
    protected override void Update()
    {
        base.Update(); // Call base class Update for movement and health UI updates
        UpdateTarget(); // 更新目標
        MoveTowardsTarget();
    }

    private void UpdateTarget()
    {
        // 確保 "PlayerShip" 層存在
        Collider2D[] playerShips = Physics2D.OverlapCircleAll(transform.position, detectDistance, LayerMask.GetMask("PlayerShip"));
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
            Transform navalBase = GameObject.FindWithTag("NavalBase")?.transform;
            if (navalBase != null)
            {
                target = navalBase; // 如果沒有玩家船隻，目標為 NavalBase
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > minDistanceToNavalBase && distance <= 6f) // 調整距離閾值
            {
                currentMovementMode = MovementMode.SpeedAndTarget; // 設置移動模式
                OrbitAroundNavalBase(); // Move around the naval base
            }
            else
            {
                currentMovementMode = MovementMode.SpeedAndAngle; // 設置移動模式
                Vector3 direction = (target.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                TargetAzimuthAngle = Mathf.Repeat(targetAngle, 360f); // 使用目標角度更新方向
                TargetSpeed = maxSpeed; // 使用基類的速度邏輯
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

            TargetAzimuthAngle = Mathf.Repeat(targetAngle, 360f); // 使用軌道角度更新方向
            TargetSpeed = orbitSpeed; // 使用基類的速度邏輯
        }
    }

    public void PerformAttack()
    {
        // 實現敵人攻擊邏輯
        Debug.Log("EnemyShip is performing an attack.");
    }

    private void ShootAmmoAtTarget()
    {
        if (ammoPrefab != null && firePoint != null && target != null)
        {
            AmmoManager ammoManager = FindFirstObjectByType<AmmoManager>();
            if (ammoManager != null)
            {
                GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation, ammoManager.transform);
                if (ammo.TryGetComponent<Ammo>(out Ammo ammoScript))
                {
                    string targetTag = target.CompareTag("PlayerShip") ? "PlayerShip" : "NavalBase";
                    ammoScript.SetTarget(target.position, targetTag);
                }
            }
            else
            {
                Debug.LogWarning("AmmoManager not found in the scene.");
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

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (Health <= 0)
        {
            RewardPlayer();
        }
    }

    private void RewardPlayer()
    {
        NavalBaseController navalBase = GameObject.FindWithTag("NavalBase")?.GetComponent<NavalBaseController>();
        if (navalBase != null)
        {
            navalBase.AddGold(goldReward);
        }
        else
        {
            Debug.LogWarning("NavalBaseController not found.");
        }

        PlayerShip playerShip = FindFirstObjectByType<PlayerShip>();
        if (playerShip != null)
        {
            playerShip.GainExperience(experienceReward); // 獎勵經驗值給玩家船隻
        }
        else
        {
            Debug.LogWarning("PlayerShip not found.");
        }
    }
}
