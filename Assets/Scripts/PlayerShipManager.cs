using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // 新增 UI 命名空間

public class PlayerShipManager : MonoBehaviour
{
    [Header("Ship Settings")]
    public GameObject ShipPrefab;
    public int ShipCost = 50;
    public NavalBaseController navalBaseController;

    [Header("UI Settings")]
    public GameObject shipUIPrefab; // 單個船隻的 UI 預製體
    public Transform shipsListContent; // ScrollView 的 Content 物件
    public ScrollRect shipsScrollView; // ScrollView 元件

    [Header("Panel Settings")]
    public GameObject shipsPanel; // 船隻列表面板

    public List<GameObject> playerShips = new List<GameObject>(); // 保持 privat

    private void Start()
    {
        if (shipsPanel != null)
        {
            shipsPanel.SetActive(false); // 遊戲開始時隱藏面板
        }
    }

    public bool CreateShipFromPrefab(Vector3 position)
    {
        if (navalBaseController.gold >= ShipCost)
        {
            navalBaseController.gold -= ShipCost;
            navalBaseController.UpdateGoldUI();
            
            GameObject newShip = Instantiate(ShipPrefab, position, Quaternion.identity);
            playerShips.Add(newShip);
            
            UpdateShipsListUI(); // 新增船隻後更新 UI
            return true;
        }
        
        Debug.Log("金幣不足，無法創建船隻！");
        return false;
    }

    public void FormBattleLine(Vector3 startPosition, Vector3 direction, float spacing)
    {
        for (int i = 0; i < playerShips.Count; i++)
        {
            if (playerShips[i] != null)
            {
                Vector3 position = startPosition + direction.normalized * spacing * i;
                playerShips[i].transform.position = position;
            }
        }
    }

    // 新增：更新船隻列表 UI
    private void UpdateShipsListUI()
    {

        Debug.Log("Updating ships list UI...");
        // 清除現有列表
        foreach (Transform child in shipsListContent)
        {
            Destroy(child.gameObject);
        }

        // 為每艘船創建 UI 項目
        for (int i = 0; i < playerShips.Count; i++)
        {
            if (playerShips[i] != null)
            {
                GameObject shipUI = Instantiate(shipUIPrefab, shipsListContent);
                Text shipText = shipUI.GetComponentInChildren<Text>();
                
                if (shipText != null)
                {
                    shipText.text = $"船隻 {i + 1}";
                }
            
            }
        }

        // 自動調整 Content 大小
        LayoutRebuilder.ForceRebuildLayoutImmediate(shipsListContent.GetComponent<RectTransform>());
        
        // 滾動到最底部
        shipsScrollView.normalizedPosition = new Vector2(0, 0);
    }

    // 新增：開啟和關閉船隻列表面板的方法
    public void ToggleShipsPanel()
    {
        if (shipsPanel != null)
        {
            shipsPanel.SetActive(!shipsPanel.activeSelf);
        }
    }
}