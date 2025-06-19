using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class DangerTileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public float updateInterval = 1f; // 每秒更新一次
    public float dangerIncreaseRate = 10f; // 危險等級提升百分比（例如 10%）

    private void Start()
    {
        StartCoroutine(UpdateDangerLevels());
    }

    IEnumerator UpdateDangerLevels()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            // 遍歷所有 Tile
            BoundsInt bounds = tilemap.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                DangerTile tile = tilemap.GetTile<DangerTile>(pos);
                if (tile != null)
                {
                    // 檢查上下左右的 Danger Level
                    float avgSurroundingDanger = GetAverageSurroundingDanger(pos);
                    
                    // 如果周圍有相同 Danger Level，則提升自身 Danger Level
                    if (avgSurroundingDanger >= tile.dangerLevel)
                    {
                        tile.dangerLevel += dangerIncreaseRate;
                        tile.dangerLevel = Mathf.Clamp(tile.dangerLevel, 0f, 100f); // 限制在 0~100
                        tilemap.RefreshTile(pos); // 更新 Tile 顯示
                    }
                }
            }
        }
    }

    // 計算周圍 4 個方向的平均 Danger Level
    private float GetAverageSurroundingDanger(Vector3Int pos)
    {
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        float totalDanger = 0f;
        int count = 0;

        foreach (Vector3Int dir in directions)
        {
            DangerTile neighborTile = tilemap.GetTile<DangerTile>(pos + dir);
            if (neighborTile != null)
            {
                totalDanger += neighborTile.dangerLevel;
                count++;
            }
        }

        return (count > 0) ? (totalDanger / count) : 0f;
    }
}