using UnityEngine;

public abstract class Weapon : MonoBehaviour // 將類別標記為 abstract
{
    public string WeaponName;
    public int Damage;
    public float Range;
    public float Cooldown;

    [Header("武器屬性")]
    public Transform firePoint;            // 子彈發射點
    public GameObject ammoPrefab;          // 子彈預製體
    public float attackInterval = 1f;      // 攻擊間隔（秒）
    private float m_attackTimer;           // 攻擊計時器

    public abstract void Fire(Vector3 position, Vector3 direction);

    public void HandleAttack(Vector3 position, Vector3 direction)
    {
        m_attackTimer += Time.deltaTime;
        if (m_attackTimer >= attackInterval)
        {
            Fire(position, direction);
            m_attackTimer = 0f; // 重置攻擊計時器
        }
    }

    public void DrawAttackRange(Vector3 position)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, Range);
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, Range);
        }
    }
}
