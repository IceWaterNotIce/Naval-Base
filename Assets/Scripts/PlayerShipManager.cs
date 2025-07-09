using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public class PlayerShipManager : MonoBehaviour
{
    [Header("Ship Settings")]
    public GameObject ShipPrefab;
    public int ShipCost = 50;
    public NavalBaseController navalBaseController;

    public List<GameObject> playerShips = new List<GameObject>();

    public bool CreateShipFromPrefab()
    {
        if (navalBaseController.gold >= ShipCost)
        {
            navalBaseController.gold -= ShipCost;
            navalBaseController.UpdateGoldUI();



            var dock = GameObject.FindGameObjectWithTag("Dock");
            if (dock == null)
            {
                Debug.LogError("找不到碼頭，無法創建船隻！");
                return false;
            }
            Vector3 spawnPos = dock.transform.position;

            GameObject newShip = Instantiate(ShipPrefab, spawnPos, Quaternion.identity, transform);
            playerShips.Add(newShip);
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

    // 提供船隻列表的唯讀訪問
    public IReadOnlyList<GameObject> GetPlayerShips()
    {
        return playerShips.AsReadOnly();
    }
}