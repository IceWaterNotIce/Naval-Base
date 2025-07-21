using UnityEngine;

public class MilitaryAircraft : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Military Aircraft launched from {position} with damage {Damage}");
        // 實現軍用飛機邏輯
    }
}
