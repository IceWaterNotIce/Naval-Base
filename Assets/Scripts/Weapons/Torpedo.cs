using UnityEngine;

public class Torpedo : Weapon
{
    public override void Fire(Vector3 position, Vector3 direction)
    {
        Debug.Log($"Torpedo launched at {position} with damage {Damage}");
        // 實現魚雷邏輯
    }
}
