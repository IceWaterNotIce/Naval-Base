using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public abstract class Weapon : ScriptableObject
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
}

[CreateAssetMenu(fileName = "NewBomb", menuName = "Weapons/Bomb")]
public class Bomb : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Bomb dropped at {position} with damage {Damage}");
        // 實現炸彈邏輯
    }
}

[CreateAssetMenu(fileName = "NewAirplane", menuName = "Weapons/Airplane")]
public class Airplane : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Airplane launched from {position} with damage {Damage}");
        // 實現飛機邏輯
    }
}

[CreateAssetMenu(fileName = "NewFishBomb", menuName = "Weapons/FishBomb")]
public class FishBomb : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Fish Bomb launched at {position} with damage {Damage}");
        // 實現魚雷邏輯
    }
}
