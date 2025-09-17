using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public GameObject tilePrefab;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);
                GameObject tile = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation, transform);

                GridTile gridTile = tile.GetComponent<GridTile>();
                gridTile.gridPosition = new Vector2Int(x, z);

                // Optional: alternate colors for readability
                if ((x + z) % 2 == 0)
                    tile.GetComponent<Renderer>().material.color = Color.green;
                else
                    tile.GetComponent<Renderer>().material.color = Color.gray;
            }
        }
    }
}
