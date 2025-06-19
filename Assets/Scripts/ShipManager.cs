using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public GameObject ShipPrefab;
    public int PlayerGold = 100; // 玩家初始金幣
    public int ShipCost = 50;    // 每艘船的金幣成本

    public bool CreateShipFromPrefab(Vector3 position)
    {
        if (PlayerGold >= ShipCost)
        {
            PlayerGold -= ShipCost;
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