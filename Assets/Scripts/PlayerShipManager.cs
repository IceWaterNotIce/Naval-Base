using UnityEngine;
using System.Collections.Generic; // 引入命名空間以使用 List

public class PlayerShipManager : MonoBehaviour
{
    public GameObject ShipPrefab;
    public int ShipCost = 50; // 每艘船的金幣成本
    public NavalBaseController navalBaseController; // 引用 NavalBaseController

    public List<GameObject> playerShips = new List<GameObject>(); // 管理所有玩家船隻

    public bool CreateShipFromPrefab(Vector3 position)
    {
        if (navalBaseController.gold >= ShipCost)
        {
            navalBaseController.gold -= ShipCost; // 扣除金幣
            navalBaseController.UpdateGoldUI(); // 更新金幣 UI
            GameObject newShip = Instantiate(ShipPrefab, position, Quaternion.identity);
            playerShips.Add(newShip); // 添加到玩家船隻列表
            return true; // 創建成功
        }
        else
        {
            Debug.Log("金幣不足，無法創建船隻！");
            return false; // 創建失敗
        }
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
}