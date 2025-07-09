using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelShipList : PopupPanel
{
    [Header("UI Settings")]
    public GameObject shipUIPrefab;
    public Transform shipsListContent;
    public ScrollRect shipsScrollView;
    public GameObject shipsPanel;

    private PlayerShipManager playerShipManager;

    private void Start()
    {
        playerShipManager = FindFirstObjectByType<PlayerShipManager>();
        // 移除 shipsPanel.SetActive(false); 由 PopupPanel 控制顯示/隱藏
    }

    public void ToggleShipsPanel()
    {
        if (shipsPanel != null)
        {
            bool isActive = !shipsPanel.activeSelf;
            if (isActive)
            {
                Show();
                if (playerShipManager != null)
                {
                    UpdateShipsListUI(playerShipManager.GetPlayerShips());
                }
            }
            else
            {
                Hide();
            }
        }
    }

    public override void Show()
    {
        if (shipsPanel != null)
            shipsPanel.SetActive(true);
        base.Show();
    }

    public override void Hide()
    {
        if (shipsPanel != null)
            shipsPanel.SetActive(false);
        base.Hide();
    }

    private void UpdateShipsListUI(IReadOnlyList<GameObject> ships)
    {
        // 清除現有列表
        foreach (Transform child in shipsListContent)
        {
            Destroy(child.gameObject);
        }

        // 為每艘船創建 UI 項目
        for (int i = 0; i < ships.Count; i++)
        {
            if (ships[i] != null)
            {
                GameObject shipUI = Instantiate(shipUIPrefab, shipsListContent);
                Text shipText = shipUI.GetComponentInChildren<Text>();
                
                if (shipText != null)
                {
                    shipText.text = $"船隻 {i + 1}";
                }
            }
        }

        // 自動調整 Content 高度
        if (shipsListContent != null)
        {
            float itemHeight = 30f; // 假設每個項目高度為 30
            shipsListContent.GetComponent<RectTransform>().sizeDelta = 
                new Vector2(shipsListContent.GetComponent<RectTransform>().sizeDelta.x, 
                           ships.Count * itemHeight);
        }
    }
}