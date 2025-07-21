using UnityEngine;

public class Turret : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Turret fired from {position} with damage {Damage}");
        // 實現炮塔邏輯
    }
}
