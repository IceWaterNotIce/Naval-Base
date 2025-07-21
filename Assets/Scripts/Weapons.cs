using UnityEngine;

public abstract class Weapon : MonoBehaviour
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
}

public class Bomb : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Bomb dropped at {position} with damage {Damage}");
        // 實現炸彈邏輯
    }
}

public class Airplane : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Airplane launched from {position} with damage {Damage}");
        // 實現飛機邏輯
    }
}

public class FishBomb : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Fish Bomb launched at {position} with damage {Damage}");
        // 實現魚雷邏輯
    }
}
