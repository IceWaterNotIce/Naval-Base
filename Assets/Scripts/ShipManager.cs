using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public GameObject ShipPrefab;
    public int ShipCost = 50; // 每艘船的金幣成本
    public NavalBaseController navalBaseController; // 引用 NavalBaseController

    public bool CreateShipFromPrefab(Vector3 position)
    {
        if (navalBaseController.gold >= ShipCost)
        {
            navalBaseController.gold -= ShipCost; // 扣除金幣
            navalBaseController.UpdateGoldUI(); // 更新金幣 UI
            Instantiate(ShipPrefab, position, Quaternion.identity);
            return true; // 創建成功
        }
        else
        {
            Debug.Log("金幣不足，無法創建船隻！");
            return false; // 創建失敗
        }
    }
}