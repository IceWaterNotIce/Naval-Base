using UnityEngine;

public class Warship : Ship
{
    public Transform firePoint; // 子彈發射點
    public GameObject ammoPrefab; // 子彈預製體
    public float attackInterval = 1f; // 攻擊間隔（秒）
    protected float attackTimer; // 攻擊計時器
    public int attackDamage = 1; // 攻擊傷害

    public virtual void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            PerformAttack();
            attackTimer = 0f; // 重置攻擊計時器
        }
    }

    protected virtual void PerformAttack()
    {
        // 攻擊邏輯由子類別實現
    }

    protected override void Update()
    {
        base.Update(); // 繼承基類的移動與旋轉邏輯
        HandleAttack(); // 處理攻擊邏輯
    }
}
