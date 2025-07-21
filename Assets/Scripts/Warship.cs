// =============================================
// 檔案名稱: Warship.cs
// 創建日期: [日期]
// 最後修改: [日期]
// 功能描述: 戰艦基礎類別，包含攻擊、偵測、升級系統
// =============================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 戰艦基礎類別，繼承自Ship類
/// </summary>
public class Warship : Ship
{
    #region 偵測系統變數
    [Header("偵測系統")]
    public float detectDistance = 10f;     // 偵測距離
    public float beDetectDistance = 8f;    // 被偵測距離
    private float stealthFactor = 5f;      // 隱蔽值
    private float environmentModifier = 0f; // 環境修正值
    private float detectionThreshold = 10f; // 偵測閾值
    public List<int> enemyLayerMasks = new List<int>(); // 敵人圖層遮罩列表
    #endregion

    #region 攻擊系統變數
    [Header("攻擊系統")]
    public Transform firePoint;            // 子彈發射點
    public GameObject ammoPrefab;          // 子彈預製體
    public float attackInterval = 1f;      // 攻擊間隔（秒）
    private float m_attackTimer;           // 攻擊計時器
    private int m_attackDamage = 1;        // 攻擊傷害
    #endregion

    #region 升級系統變數
    [Header("升級系統")]
    private int m_level = 1;               // 初始等級
    private int m_experience = 0;          // 當前經驗值
    private int m_experienceToNextLevel = 100; // 升級所需經驗值
    #endregion

    #region UI參考
    [Header("UI 參考")]
    public Text levelText;                 // 等級顯示文字
    public Slider experienceSlider;        // 經驗條
    public Text experienceText;            // 經驗值文字
    #endregion

    #region Unity生命週期
    void Start()
    {
        UpdateLevelUI();
        UpdateExperienceUI();
    }

    protected override void Update()
    {
        base.Update();          // 繼承基類的移動與旋轉邏輯
        HandleAttack();         // 處理攻擊邏輯
        DetectNearbyEnemies();  // 偵測附近敵人
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
    #endregion

    #region 攻擊系統
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
    #endregion

    #region 升級系統
    public virtual void GainExperience(int amount)
    {
        m_experience += amount;
        if (m_experience >= m_experienceToNextLevel)
        {
            LevelUp();
        }
        UpdateExperienceUI();
    }

    protected virtual void LevelUp()
    {
        m_level++;
        m_experience -= m_experienceToNextLevel;
        m_experienceToNextLevel += 50; // 每次升級增加所需經驗值
        m_attackDamage += 1;           // 提升攻擊傷害
        maxHealth += 10;               // 提升最大血量
        Health = maxHealth;            // 恢復血量
        
        Debug.Log($"Warship leveled up to {m_level}! Attack Damage: {m_attackDamage}, Max Health: {maxHealth}");
        UpdateLevelUI();
        UpdateExperienceUI();
    }
    #endregion

    #region UI更新方法
    public void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{m_level}";
        }
    }

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
    #endregion

    #region 偵測系統
    private bool IsDetected(Warship detector)
    {
        float distance = Vector2.Distance(transform.position, detector.transform.position);
        float distanceFactor = Mathf.Clamp01(1 - distance / detector.detectDistance);
        float weatherModifier = WeatherManager.Instance.GetCurrentVisibility();
        float detectionValue = (detector.detectDistance * 0.5f) * distanceFactor * weatherModifier - stealthFactor;
        return detectionValue > detectionThreshold;
    }

    private void DetectNearbyEnemies()
    {
        foreach (int layerMask in enemyLayerMasks)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectDistance, 1 << layerMask);
            foreach (var hit in hits)
            {
                Warship enemy = hit.GetComponent<Warship>();
                if (enemy == null || enemy == this) continue;

                float detectionCertainty = CalculateDetectionCertainty(enemy);
                if (detectionCertainty > 0.7f)
                {
                    Debug.Log($"[確信] 偵測到 {enemy.ShipName}！");
                    enemy.OnDetected(this);
                }
                else if (detectionCertainty > 0.3f)
                {
                    Debug.Log($"[懷疑] 可能發現 {enemy.ShipName}...");
                }
            }
        }
    }

    private float CalculateDetectionCertainty(Warship enemy)
    {
        float distance = Vector2.Distance(transform.position, enemy.transform.position);
        return Mathf.Clamp01(1 - distance / detectDistance);
    }

    public virtual void OnDetected(Warship detector)
    {
        Debug.Log($"{ShipName} 被 {detector.ShipName} 偵測到！");
    }
    #endregion
}