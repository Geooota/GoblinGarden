using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections;

public class PlotInfo : MonoBehaviour
{
    private Tilemap tilemap;
    public enum PlotSize
    {
        Small, Medium, Large
    }

    private bool purchasedPlot = false;
    public PlotSize thisPlotSize;
    private Vector3Int min;
    private Vector3Int size;
    private BoundsInt region;
    public GameObject purchasePlotButton;
    public GameObject plotUpgradePanel;

    private void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);          // Convert world position to tilemap cell


        switch (thisPlotSize)
        {
            case PlotSize.Small:
                min = cellPos + new Vector3Int(-1, -1, 0);
                size = new Vector3Int(3, 3, 1);
                region = new BoundsInt(min, size);
                break;

            case PlotSize.Medium:
                min = cellPos + new Vector3Int(-2, -2, 0);
                size = new Vector3Int(5, 5, 1);
                region = new BoundsInt(min, size);
                break;

            case PlotSize.Large:
                min = cellPos + new Vector3Int(-3, -3, 0);
                size = new Vector3Int(7, 7, 1);
                region = new BoundsInt(min, size);
                break;
        }

        foreach (var pos in region.allPositionsWithin)
        {
            Debug.Log($"Cell at {pos}");
        }
    }
    public void BuildThisDirtyPlot()
    {
        TilemapClicker.Instance.BuildPlot(PlotType.Dirty, region);
        purchasePlotButton.SetActive(false);
        purchasedPlot = true;
    }
    public void BuildThisWateryPlot()
    {
        TilemapClicker.Instance.BuildPlot(PlotType.Watery, region);
        purchasePlotButton.SetActive(false);
        purchasedPlot = true;
    }
    public void BuildThisSpeedyPlot()
    {
        TilemapClicker.Instance.BuildPlot(PlotType.Speedy, region);
        purchasePlotButton.SetActive(false);
        purchasedPlot = true;
    }
    public void BuildThisGoldenPlot()
    {
        TilemapClicker.Instance.BuildPlot(PlotType.Golden, region);
        purchasePlotButton.SetActive(false);
        purchasedPlot = true;
    }
    public void MakeOptionsAppear()
    {
        if (!purchasedPlot)
        {
            purchasePlotButton.SetActive(true);
        }
        else
        {
            plotUpgradePanel.SetActive(true);
        }
    }


}
