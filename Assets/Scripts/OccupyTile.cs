using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilemapClicker;

public class OccupyTile : MonoBehaviour
{
    public GameObject otherNono;
    
    void Start()
    {
        if (otherNono != null)
        {
            Vector3Int cellPos1 = TilemapClicker.Instance.tilemap.WorldToCell(this.transform.position);
            Vector3Int cellPos2 = TilemapClicker.Instance.tilemap.WorldToCell(otherNono.transform.position);
            cellPos2.z = 1; // Set depth to 1 for 3D bounds
            Vector3Int min = Vector3Int.Min(cellPos1, cellPos2);
            Vector3Int max = Vector3Int.Max(cellPos1, cellPos2);

            // BoundsInt expects position (min) and size (max - min)
            BoundsInt region = new BoundsInt(min, max - min);

            foreach (Vector3Int pos in region.allPositionsWithin)
            {
                TileInfo tileInfo = new TileInfo
                {
                    isOccupied = true,
                    plotInfo = null,
                    plantInfo = null
                };
                TilemapClicker.Instance.tileInfos.Add(pos,tileInfo);
            }
        }
        Destroy(this.gameObject);
    }
}
