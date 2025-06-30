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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) // Toggle debug panel with F3
        {
            isDebugPanelVisible = !isDebugPanelVisible;
            debugPanel.SetActive(isDebugPanelVisible);
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
            debugText.text = $"Naval Base:\n" +
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
}
