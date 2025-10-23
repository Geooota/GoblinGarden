using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
    public BoundsInt region;
    public MeshRenderer dirtObject;
    public GameObject purchasePlotButton;
    public GameObject plotUpgradePanel;
    public GameObject rocks;
    public Material dirtNormal;
    public Material dirtWatery;
    public Material dirtSpeedy;
    public Material dirtGolden;
    public Material woodMat;
    private Material waterMat;
    public AudioClip destroyRocksSound;
    public AudioClip WaterSound;
    public bool dry = false;

    private Material[] mats;

    public PlotType thisPlotType;

    private void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        Vector3Int cellPos = tilemap.WorldToCell(transform.position);          // Convert world position to tilemap cell
        mats = dirtObject.materials;
        mats[1] = woodMat;
        mats[2] = dirtNormal;
        waterMat = mats[0];
        dirtObject.materials = mats;

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
    }

    public void BuildThisPlot(int typeNum)
    {
        int cost = DeterminePlotCost(PlotType.Dirty);
        //use determinecost before switch, if gold amount isn't enough, return debug
        if (cost > TilemapClicker.Instance.goldAmount)
        {
            Debug.Log("Not Enough Gold!");
            plotUpgradePanel.SetActive(false);
            return;
        }

        StartCoroutine(DryOut());

        mats = dirtObject.materials;

        TilemapClicker.Instance.BuildPlot(this, region);

        switch (typeNum)
        {
            case 1:
                thisPlotType = PlotType.Dirty;
                mats[2] = dirtNormal;
                rocks.SetActive(false);
                AudioSource.PlayClipAtPoint(destroyRocksSound, Camera.main.transform.position);
                break;
            case 2:
                thisPlotType = PlotType.Watery;
                mats[2] = dirtWatery;
                break;
            case 3:
                thisPlotType = PlotType.Speedy;
                mats[2] = dirtSpeedy;
                break;
            case 4:
                thisPlotType = PlotType.Golden;
                mats[2] = dirtGolden;
                break;
        }

        TilemapClicker.Instance.goldAmount -= cost;
        TilemapClicker.Instance.goldText.text = TilemapClicker.Instance.goldAmount.ToString();

        purchasePlotButton.SetActive(false);
        plotUpgradePanel.SetActive(false);
        purchasedPlot = true;
        dirtObject.materials = mats;
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

    public void MakeOptionsDisappear()
    {
        if (!purchasedPlot)
        {
            purchasePlotButton.SetActive(false);
        }
        else
        {
            plotUpgradePanel.SetActive(false);
        }
    }

    private int DeterminePlotCost(PlotType plotType)
    {
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

    private IEnumerator DryOut()
    {
        Debug.LogError("Starting Dryout");
        yield return new WaitForSeconds(30f);

        if (thisPlotType != PlotType.Watery)
        {
            dry = true;

            mats[0] = dirtNormal;
            dirtObject.materials = mats;

            Debug.LogError("All Dried Out!");
            foreach (var loc in region.allPositionsWithin)
            {
                if (TilemapClicker.Instance.tileInfos.ContainsKey(loc))
                {
                    if (TilemapClicker.Instance.tileInfos[loc].plantInfo != null)
                        TilemapClicker.Instance.tileInfos[loc].plantInfo.DryOut();
                }
            }
        }
        else StartCoroutine(DryOut());
    }
    public void Wet()
    {
        Debug.Log("You watered it");

        mats[0] = waterMat;
        dirtObject.materials = mats;
        AudioSource.PlayClipAtPoint(WaterSound, Camera.main.transform.position);

        dry = false;
        foreach (var loc in region.allPositionsWithin)
        {
            if (TilemapClicker.Instance.tileInfos.ContainsKey(loc))
            {
                if (TilemapClicker.Instance.tileInfos[loc].plantInfo != null)
                    TilemapClicker.Instance.tileInfos[loc].plantInfo.Wet();
            }
        }
        StartCoroutine(DryOut());
    }

}
