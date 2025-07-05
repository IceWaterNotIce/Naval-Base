using UnityEngine;

public class CoastalTurret : MonoBehaviour
{
    public float detectionRadius = 8f;
    public float attackInterval = 1.5f;
    public int attackDamage = 2;
    public GameObject ammoPrefab;
    public Transform firePoint;
    [Tooltip("請在 Inspector 設定為 Enemy Layer")] 
    public LayerMask enemyLayer; // 確保 Inspector 設定正確

    private float attackTimer;

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            AttackNearestEnemy();
            attackTimer = 0f;
        }
    }

    private void AttackNearestEnemy()
    {
        // 確認 LayerMask 設定正確，敵人物件需在 Enemy Layer
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }
        if (nearest != null)
        {
            Vector3 dir = (nearest.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (ammoPrefab != null && firePoint != null)
            {
                var ammoManager = FindFirstObjectByType<AmmoManager>();
                GameObject ammo = Instantiate(ammoPrefab, firePoint.position, firePoint.rotation, ammoManager != null ? ammoManager.transform : null);
                if (ammo.TryGetComponent<Ammo>(out var ammoScript))
                {
                    ammoScript.SetTarget(nearest.position, "Enemy");
                    ammoScript.damage = attackDamage;
                }
            }
        }
        // 若 enemies 長度為 0，請檢查敵人是否在正確 Layer
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
