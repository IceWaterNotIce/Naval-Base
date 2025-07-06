using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class CoastalTurretManager : MonoBehaviour
{
    [Header("References")]
    public InfiniteTileMap tileMap; // 參考 InfiniteTileMap
    public NavalBaseController navalBaseController; // 參考 NavalBaseController

    [Header("Coastal Turret Settings")]
    public GameObject coastalTurretPrefab; // 沿海砲塔預製體
    public int coastalTurretCost = 100; // 建造砲塔所需金幣

    [Header("UI")]
    public Button buildTurretButton; // 建造砲塔按鈕
    public GameObject selectionIndicator; // 選擇位置時的視覺指示器

    private bool isWaitingForPositionSelection = false;

    void Start()
    {
        // 初始化按鈕監聽
        if (buildTurretButton != null)
        {
            buildTurretButton.onClick.AddListener(OnBuildButtonClicked);
        }

        // 隱藏選擇指示器
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // 在等待選擇位置時處理點擊
        if (isWaitingForPositionSelection && Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Test] HandlePositionSelection called");
            HandlePositionSelection();
        }
    }

    private void OnBuildButtonClicked()
    {
        StartPositionSelection();
    }

    // 判斷指定陸地瓦片是否靠近海洋瓦片
    public bool CanBuildCoastalTurret(Vector3Int landTilePos)
    {
        // 強制將Z軸設為0，因為Tilemap通常是2D的
        landTilePos.z = 0;

        // 檢查是否為陸地瓦片
        TileBase tileAtPos = tileMap.landSavedTileMap.GetTile(landTilePos); // 修改
        bool isLand = tileAtPos == tileMap.landRuleTile;

        Debug.Log($"[Debug] Checking tile at {landTilePos}");
        Debug.Log($"[Debug] Tile found: {tileAtPos != null}");
        Debug.Log($"[Debug] Tile type: {tileAtPos?.GetType()}");
        Debug.Log($"[Debug] Expected rule tile: {tileMap.landRuleTile?.name}");
        Debug.Log($"[Debug] Actual tile name: {tileAtPos?.name}");
        Debug.Log($"[Debug] Is land tile: {isLand}");

        if (!isLand)
        {
            Debug.LogWarning($"[Debug] Not a land tile at {landTilePos}");
            return false;
        }

        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        bool nearOcean = false;
        foreach (var dir in directions)
        {
            Vector3Int neighbor = landTilePos + dir;
            neighbor.z = 0; // 確保Z軸為0
            TileBase oceanTile = tileMap.oceanSavedTileMap.GetTile(neighbor); // 修改
            bool isOcean = oceanTile == tileMap.oceanRuleTile;

            Debug.Log($"[Debug] Checking neighbor at {neighbor}");
            Debug.Log($"[Debug] Neighbor tile: {oceanTile?.name}");
            Debug.Log($"[Debug] Is ocean: {isOcean}");

            if (isOcean)
            {
                nearOcean = true;
                break; // 找到一個相鄰海洋即可
            }
        }

        Debug.Log($"[Debug] Final result - near ocean: {nearOcean}");
        return nearOcean;
    }

    // 在指定陸地瓦片上建造沿海砲塔
    public bool BuildCoastalTurret(Vector3Int landTilePos)
    {
        if (!isWaitingForPositionSelection) return false;
        if (coastalTurretPrefab == null || navalBaseController == null) return false;
        if (!CanBuildCoastalTurret(landTilePos)) return false;
        if (navalBaseController.gold < coastalTurretCost) return false;

        navalBaseController.DeductGold(coastalTurretCost);
        Vector3 worldPos = tileMap.landSavedTileMap.CellToWorld(landTilePos) + new Vector3(0.5f, 0.5f, 0); // 修改
        Instantiate(coastalTurretPrefab, worldPos, Quaternion.identity, this.transform);

        Debug.Log("[Test] 砲塔建造成功");
        return true;
    }

    private void StartPositionSelection()
    {
        isWaitingForPositionSelection = true;
        if (buildTurretButton != null)
            buildTurretButton.GetComponent<Image>().color = Color.yellow; // 視覺反饋
        Debug.Log("[Test] 請點擊地圖選擇砲塔位置");
    }

    private void CancelPositionSelection()
    {
        isWaitingForPositionSelection = false;
        if (buildTurretButton != null)
            buildTurretButton.GetComponent<Image>().color = Color.white;
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
        Debug.Log("[Test] 取消選擇砲塔位置");
    }

    private void HandlePositionSelection()
    {
        Debug.Log("[Test] HandlePositionSelection called, 檢測鼠標位置");

        // 獲取鼠標位置並轉換為世界座標
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 確保Z軸為0

        // 轉換為Tilemap座標
        Vector3Int tilePos = tileMap.landSavedTileMap.WorldToCell(mouseWorldPos); // 修改
        tilePos.z = 0; // 確保Z軸為0

        Debug.Log($"[Debug] Mouse world position: {mouseWorldPos}");
        Debug.Log($"[Debug] Converted tile position: {tilePos}");

        // 顯示選擇指示器
        if (selectionIndicator != null)
        {
            Vector3 indicatorPos = tileMap.landSavedTileMap.CellToWorld(tilePos); // 修改
            indicatorPos.x += 0.5f;
            indicatorPos.y += 0.5f;
            indicatorPos.z = 0;
            selectionIndicator.transform.position = indicatorPos;
            selectionIndicator.SetActive(true);
        }

        // 檢查是否有效位置
        bool canBuild = CanBuildCoastalTurret(tilePos);
        Debug.Log($"[Test] CanBuild at {tilePos}: {canBuild}");

        if (canBuild)
        {
            if (BuildCoastalTurret(tilePos))
            {
                Debug.Log($"[Test] 成功在 {tilePos} 建造砲塔");
                CancelPositionSelection();
            }
            else
            {
                Debug.LogWarning("[Test] 建造失敗，可能是金幣不足");
            }
        }
        else
        {
            Debug.LogWarning($"[Test] 無效的砲塔位置！{tilePos} 必須是靠近海洋的陸地");
        }
    }
}