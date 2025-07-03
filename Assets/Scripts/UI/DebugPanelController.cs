using UnityEngine;
using UnityEngine.UI;

public class DebugPanelController : MonoBehaviour
{
    public GameObject debugPanel; // Reference to the debug panel UI
    public Text debugText; // Reference to the Text component for displaying debug information
    public NavalBaseController navalBaseController; // Reference to NavalBaseController
    public EnemyManager enemyManager; // Reference to EnemyManager
    public PlayerShipManager playerShipManager; // Reference to PlayerShipManager

    private bool isDebugPanelVisible = false;
    private bool showColliders = false; // 新增：是否顯示碰撞框

    void Start()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) // Toggle debug panel with F3
        {
            isDebugPanelVisible = !isDebugPanelVisible;
            debugPanel.SetActive(isDebugPanelVisible);
        }

        // F3+B 顯示碰撞框
        if (Input.GetKey(KeyCode.F3) && Input.GetKeyDown(KeyCode.B))
        {
            showColliders = !showColliders;
        }

        if (isDebugPanelVisible)
        {
            UpdateDebugInfo();
        }
    }

    private void UpdateDebugInfo()
    {
        if (debugText != null)
        {
            string version = "v1.0.0"; // 可根據實際版本修改
            float gameTime = Time.time;
            Vector3 camPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

            debugText.text =
                $"Version: {version}\n" +
                $"Game Time: {gameTime:F1}s\n" +
                $"Camera Pos: ({camPos.x:F1}, {camPos.y:F1}, {camPos.z:F1})\n\n" +
                $"Naval Base:\n" +
                $"- Gold: {navalBaseController.gold}\n" +
                $"- Health: {navalBaseController.health}/{navalBaseController.maxHealth}\n" +
                $"- Level: {navalBaseController.level}\n\n" +
                $"Enemies:\n" +
                $"- Active: {enemyManager.GetActiveEnemies().Count}\n\n" +
                $"Player Ships:\n" +
                $"- Active: {playerShipManager.playerShips.Count}\n";

            foreach (var ship in playerShipManager.playerShips)
            {
                if (ship != null)
                {
                    PlayerShip playerShip = ship.GetComponent<PlayerShip>();
                    if (playerShip != null)
                    {
                        debugText.text += $"- {playerShip.ShipName}: Level {playerShip.level}\n"; // 顯示每艘船的等級
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showColliders) return;

        // 畫出所有玩家船隻的碰撞框
        if (playerShipManager != null && playerShipManager.playerShips != null)
        {
            Gizmos.color = Color.green;
            foreach (var ship in playerShipManager.playerShips)
            {
                if (ship != null)
                {
                    Collider2D col = ship.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        DrawCollider2D(col);
                    }
                }
            }
        }

        // 畫出所有子彈的碰撞框
        var ammoManager = FindFirstObjectByType<AmmoManager>();
        if (ammoManager != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var ammo in ammoManager.GetActiveAmmo())
            {
                if (ammo != null)
                {
                    Collider2D col = ammo.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        DrawCollider2D(col);
                    }
                }
            }
        }
    }

    private void DrawCollider2D(Collider2D col)
    {
        if (col is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(circle.transform.position + (Vector3)circle.offset, circle.radius * circle.transform.lossyScale.x);
        }
        else if (col is BoxCollider2D box)
        {
            Gizmos.matrix = box.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        // 可擴充其他 Collider2D 類型
    }
#endif
}
