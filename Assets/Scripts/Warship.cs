using UnityEngine;
using UnityEngine.UI;

public class Warship : Ship
{    public float detectDistance = 10f; // 新增：偵測距離
    public float beDetectDistance = 8f; // 新增：被偵測距離

    public Transform firePoint; // 子彈發射點
    public GameObject ammoPrefab; // 子彈預製體
    public float attackInterval = 1f; // 攻擊間隔（秒）
    private float m_attackTimer; // 攻擊計時器

    private int m_level = 1; // 初始等級
    private int m_experience = 0; // 當前經驗值
    private int m_experienceToNextLevel = 100; // 升級所需經驗值

    private int m_attackDamage = 1; // 攻擊傷害

    // 新增：等級顯示 UI
    public Text levelText;

    // 新增：經驗條與經驗值顯示
    public Slider experienceSlider;
    public Text experienceText;

    public virtual void GainExperience(int amount)
    {
        m_experience += amount;
        if (m_experience >= m_experienceToNextLevel)
        {
            LevelUp();
        }
        UpdateExperienceUI(); // 新增：經驗值變動時更新 UI
    }

    protected virtual void LevelUp()
    {
        m_level++;
        m_experience -= m_experienceToNextLevel;
        m_experienceToNextLevel += 50; // 每次升級增加所需經驗值
        m_attackDamage += 1; // 提升攻擊傷害
        maxHealth += 10; // 提升最大血量
        Health = maxHealth; // 恢復血量
        Debug.Log($"Warship leveled up to {m_level}! Attack Damage: {m_attackDamage}, Max Health: {maxHealth}");
        // 新增：更新等級顯示
        UpdateLevelUI();
        UpdateExperienceUI(); // 新增：升級時也更新經驗 UI
    }

    // 新增：等級顯示更新方法
    public void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{m_level}";
        }
    }

    // 新增：經驗條與經驗值顯示更新方法
    public void UpdateExperienceUI()
    {
        if (experienceSlider != null)
        {
            experienceSlider.value = Mathf.Clamp01((float)m_experience / Mathf.Max(1, m_experienceToNextLevel));
        }
        if (experienceText != null)
        {
            experienceText.text = $"{m_experience} / {m_experienceToNextLevel}";
        }
    }

    void Start()
    {
        UpdateLevelUI();
        UpdateExperienceUI(); // 新增：初始化時也更新經驗 UI
    }

    public virtual void HandleAttack()
    {
        m_attackTimer += Time.deltaTime;
        if (m_attackTimer >= attackInterval)
        {
            PerformAttack();
            m_attackTimer = 0f; // 重置攻擊計時器
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
        DetectNearbyEnemies(); // 新增：偵測附近敵人
    }

    private float stealthFactor = 5f; // 隱蔽值
    private float environmentModifier = 0f; // 環境修正值
    private float detectionThreshold = 10f; // 偵測閾值

    private bool IsDetected(Warship detector)
    {
        float distance = Vector2.Distance(transform.position, detector.transform.position);
        float distanceFactor = Mathf.Clamp01(1 - distance / detector.detectDistance);
        float weatherModifier = WeatherManager.Instance.GetCurrentVisibility(); // 環境修正
        float detectionValue = (detector.detectDistance * 0.5f) * distanceFactor * weatherModifier - stealthFactor;
        return detectionValue > detectionThreshold;
    }

    private void DetectNearbyEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectDistance);
        foreach (var hit in hits)
        {
            Warship enemy = hit.GetComponent<Warship>();
            if (enemy == null || enemy == this) continue;

            float detectionCertainty = CalculateDetectionCertainty(enemy);
            if (detectionCertainty > 0.7f)
            {
                Debug.Log($"[確信] 偵測到 {enemy.ShipName}！");
                enemy.OnDetected(this); // 觸發敵人的被偵測事件
            }
            else if (detectionCertainty > 0.3f)
            {
                Debug.Log($"[懷疑] 可能發現 {enemy.ShipName}...");
            }
        }
    }

    private float CalculateDetectionCertainty(Warship enemy)
    {
        float distance = Vector2.Distance(transform.position, enemy.transform.position);
        return Mathf.Clamp01(1 - distance / detectDistance);
    }

    private void OnDrawGizmosSelected()
    {
        // 繪製偵測距離範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        // 繪製被偵測距離範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, beDetectDistance);
    }
}
