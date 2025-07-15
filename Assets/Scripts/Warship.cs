using UnityEngine;
using UnityEngine.UI;

public class Warship : Ship
{
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
    }
}
