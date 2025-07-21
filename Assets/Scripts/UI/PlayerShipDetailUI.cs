using UnityEngine;
using UnityEngine.UI;

public class PlayerShipDetailUI : MonoBehaviour
{
    public Text shipNameText;
    public Text maxSpeedText;
    public Text maxRotationSpeedText;
    public Text attackDamageText;
    public Text levelText;
    
    private PlayerShip ship;

    void Update()
    {
        if (ship != null)
        {
            UpdateShipInfo();
        }
    }

    public void SetShip(PlayerShip newShip)
    {
        ship = newShip;
        UpdateShipInfo();
    }

    private void UpdateShipInfo()
    {
        shipNameText.text = $"Name: {ship.ShipName}";
        maxSpeedText.text = $"Max Speed: {ship.maxSpeed}";
        maxRotationSpeedText.text = $"Max Rotation Speed: {ship.maxRotateSpeed}";
    }
}