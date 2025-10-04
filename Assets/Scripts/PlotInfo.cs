using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections;
using JetBrains.Annotations;

public class PlotInfo : MonoBehaviour
{
    private Tilemap tilemap;
    private int multiplier;
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
    public void BuildThisPlot(int typeNum)
    {
        int cost = DeterminePlotCost(PlotType.Dirty);
        //use determinecost before switch, if gold amount isn't enough, return debug
        if (cost > TilemapClicker.Instance.goldAmount)
        {
            Debug.Log("Not Enough Gold!");
            return;
        }

        switch (typeNum)
        {
            case 1:
                TilemapClicker.Instance.BuildPlot(PlotType.Dirty, region);
                break;
            case 2:
                TilemapClicker.Instance.BuildPlot(PlotType.Watery, region);
                break;
            case 3:
                TilemapClicker.Instance.BuildPlot(PlotType.Speedy, region);
                break;
            case 4:
                TilemapClicker.Instance.BuildPlot(PlotType.Golden, region);
                break;
        }

        TilemapClicker.Instance.goldAmount -= cost;
        TilemapClicker.Instance.goldText.text = TilemapClicker.Instance.goldAmount.ToString();

        purchasePlotButton.SetActive(false);
        plotUpgradePanel.SetActive(false);
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
    private int DeterminePlotCost(PlotType plotType)
    {
        ;
        switch (thisPlotSize)
        {
            case PlotSize.Small:
                multiplier = 1;
                break;
            case PlotSize.Medium:
                multiplier = 2;
                break;
            case PlotSize.Large:
                multiplier = 3;
                break;


        }
        switch (plotType)
        {
            case PlotType.Watery:
                return 150 * multiplier;
            case PlotType.Speedy:
                return 150 * multiplier;
            case PlotType.Golden:
                return 150 * multiplier;
            default:
                return 100 * multiplier;
        }
    }

}
