using UnityEngine;

[CreateAssetMenu(fileName = "DangerTile", menuName = "Tiles/DangerTile")]
public class DangerTile : UnityEngine.Tilemaps.Tile
{
    public float dangerLevel = 0f; // 0~100
}