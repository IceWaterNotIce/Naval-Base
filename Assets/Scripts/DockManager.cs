using UnityEngine;
using UnityEngine.Tilemaps;

public class DockManager : MonoBehaviour
{
    public GameObject dockPrefab;
    public InfiniteTileMap infiniteTileMap;
    public Tilemap oceanTileMap;
    public TileBase oceanRuleTile;
    public Tilemap landTileMap;
    public TileBase landRuleTile;
    public int dockCost = 200;
    public NavalBaseController navalBaseController;

    // 新增：海軍基地瓦片地圖與瓦片
    public Tilemap navalBaseTileMap;
    public TileBase navalBaseTile;

    private bool isBuildMode = false;
    private Quaternion currentRotation = Quaternion.identity;

    void Update()
    {
        if (!isBuildMode) return;

        // 右鍵旋轉
        if (Input.GetMouseButtonDown(1))
        {
            currentRotation *= Quaternion.Euler(0, 0, -90);
        }

        // 左鍵放置
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = landTileMap.WorldToCell(mouseWorldPos);
            cellPos.z = 0;

            // 檢查左側是否為海洋瓦片，且當前為海軍基地瓦片
            Vector3Int leftCell = cellPos + Vector3Int.left;
            if (oceanTileMap.GetTile(leftCell) == oceanRuleTile &&
                navalBaseTileMap != null && navalBaseTile != null &&
                navalBaseTileMap.GetTile(cellPos) == navalBaseTile)
            {
                if (navalBaseController.gold >= dockCost)
                {
                    Vector3 placePos = landTileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                    Instantiate(dockPrefab, placePos, currentRotation);
                    // 刪除該位置的陸地瓦片與海軍基地瓦片
                    landTileMap.SetTile(cellPos, null);
                    navalBaseTileMap.SetTile(cellPos, null);
                    navalBaseController.AddGold(-dockCost);
                    ExitBuildMode();
                }
                else
                {
                    Debug.Log("金幣不足，無法建造碼頭！");
                }
            }
            else
            {
                Debug.Log("必須在海軍基地瓦片上且左側為海洋瓦片才能建造碼頭！");
            }
        }

        // Esc 退出建造模式
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitBuildMode();
        }
    }

    public void EnterBuildMode()
    {
        isBuildMode = true;
        currentRotation = Quaternion.identity;
        Debug.Log("進入碼頭建造模式。左鍵放置，右鍵旋轉，Esc 取消。");
    }

    public void ExitBuildMode()
    {
        isBuildMode = false;
        Debug.Log("離開碼頭建造模式。");
    }
}
