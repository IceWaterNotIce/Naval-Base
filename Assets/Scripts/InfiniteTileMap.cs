using System.Collections.Generic;
using UnityEngine;

public class InfiniteTileMap : MonoBehaviour
{
    public GameObject tilePrefab; // Prefab for the tile
    public int tileSize = 10; // Size of each tile
    public int viewDistance = 3; // Number of tiles visible around the player
    public Transform player; // Reference to the player
    public bool showTileEdges = false; // Toggle for showing tile edges

    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentTilePosition;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        UpdateTileMap(); // Initialize the tile map
    }

    void Update()
    {
        Vector2Int playerTilePosition = GetPlayerTilePosition();
        if (playerTilePosition != currentTilePosition)
        {
            currentTilePosition = playerTilePosition;
            UpdateTileMap(); // Update the tile map when the player moves to a new tile
        }
    }

    private Vector2Int GetPlayerTilePosition()
    {
        return new Vector2Int(
            Mathf.FloorToInt(player.position.x / tileSize),
            Mathf.FloorToInt(player.position.y / tileSize)
        );
    }

    private void UpdateTileMap()
    {
        HashSet<Vector2Int> tilesToKeep = new HashSet<Vector2Int>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int tilePosition = currentTilePosition + new Vector2Int(x, y);
                tilesToKeep.Add(tilePosition);

                if (!tiles.ContainsKey(tilePosition))
                {
                    CreateTile(tilePosition);
                }
            }
        }

        RemoveUnusedTiles(tilesToKeep);
    }

    private void CreateTile(Vector2Int tilePosition)
    {
        Vector3 worldPosition = new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 10);
        GameObject tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);

        // Add edge line visibility logic
        LineRenderer lineRenderer = tile.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = showTileEdges; // Set visibility based on toggle
        }

        tiles[tilePosition] = tile;
    }

    private void RemoveUnusedTiles(HashSet<Vector2Int> tilesToKeep)
    {
        List<Vector2Int> tilesToRemove = new List<Vector2Int>();

        foreach (var tilePosition in tiles.Keys)
        {
            if (!tilesToKeep.Contains(tilePosition))
            {
                tilesToRemove.Add(tilePosition);
            }
        }

        foreach (var tilePosition in tilesToRemove)
        {
            Destroy(tiles[tilePosition]);
            tiles.Remove(tilePosition);
        }
    }

    public void ToggleTileEdges()
    {
        showTileEdges = !showTileEdges; // Toggle visibility

        foreach (var tile in tiles.Values)
        {
            LineRenderer lineRenderer = tile.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = showTileEdges; // Update visibility for all tiles
            }
        }
    }
}
