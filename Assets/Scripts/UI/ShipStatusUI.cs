using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipStatusUI : MonoBehaviour
{
    [Header("Status UI References")]
    public Slider healthSlider;
    public Text healthText;
    public Text nameText; // 新增名稱文字的支援

    private Ship m_ship;
    public Ship Ship
    {
        get => m_ship;
        set
        {
            // 取消舊船艦的事件訂閱
            if (m_ship != null)
            {
                m_ship.OnHealthChanged -= HandleHealthChanged;
                m_ship.OnNameChanged -= HandleNameChanged;
            }

            m_ship = value;

            // 訂閱新船艦的事件並更新 UI
            if (m_ship != null)
            {
                m_ship.OnHealthChanged += HandleHealthChanged;
                m_ship.OnNameChanged += HandleNameChanged;

                UpdateHealthUI();
                UpdateNameUI();
            }
        }
    }

    private void Awake()
    {
        // 嘗試自動獲取 Ship 參考
        if (m_ship == null)
        {
            m_ship = GetComponentInParent<Ship>();
            if (m_ship != null)
            {
                Debug.Log("Auto-found Ship in parent");
                Ship = m_ship; // 確保事件訂閱和 UI 更新
            }
        }
    }

    private void OnEnable()
    {
        if (Ship != null)
        {
            Ship.OnHealthChanged += HandleHealthChanged;
            Ship.OnNameChanged += HandleNameChanged;

            UpdateHealthUI();
            UpdateNameUI();
        }
    }

    private void OnDisable()
    {
        if (Ship != null)
        {
            Ship.OnHealthChanged -= HandleHealthChanged;
            Ship.OnNameChanged -= HandleNameChanged;
        }
    }

    private void Start()
    {
        if (Ship == null)
        {
            Debug.LogError("Ship is null in Start. Ensure it is assigned in the Inspector or dynamically at runtime.");
        }
        else
        {
            Debug.Log($"Ship is assigned in Start: {Ship.ShipName}");
        }
    }

    private void HandleHealthChanged(float newHealth)
    {
        UpdateHealthUI();
    }

    private void HandleNameChanged(string newName)
    {
        UpdateNameUI();
    }

    public void UpdateHealthUI()
    {
        if (Ship == null) return;

        if (healthSlider != null)
        {
            healthSlider.value = Ship.Health / (float)Ship.maxHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{Ship.Health}/{Ship.maxHealth}";
        }

        Debug.Log($"Updated Health UI: {Ship.Health}/{Ship.maxHealth}");
    }

    public void UpdateNameUI()
    {
        if (Ship == null || nameText == null) return;

        nameText.text = Ship.ShipName;
        Debug.Log($"Updated Name UI: {Ship.ShipName}");
    }
}