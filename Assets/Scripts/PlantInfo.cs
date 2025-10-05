using System.Collections;
using UnityEngine;

public class PlantInfo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string plantName;
    public int cost;
    public float growthTime; // in seconds
    public int yieldAmount;
    public bool collectable;
    public Vector3Int myCellPos;
    public PlotType myPlotType;

    public GameObject UIPopUp;

    public void BeginGrowing()
    {
        StartCoroutine(GrowRoutine(growthTime));
        UIPopUp.SetActive(false);
    }

    private IEnumerator GrowRoutine(float timeToGrow)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("GrowTimeStarted");
        float startTime = Time.time;
        myPlotType = TilemapClicker.Instance.tileInfos[myCellPos].plotType;

        if (myPlotType == PlotType.Speedy)
            yield return new WaitForSeconds(timeToGrow * 0.7f);
        else
            yield return new WaitForSeconds(timeToGrow);

        collectable = true;
        UIPopUp.SetActive(true);
        Debug.Log($"Plant is now collectible after {Time.time - startTime} seconds!");
    }

    public int CollectCrop()
    {
        UIPopUp.SetActive(false);
        collectable = false;
        StartCoroutine(GrowRoutine(growthTime));
        if (myPlotType == PlotType.Golden)
            return Mathf.RoundToInt(yieldAmount * 1.3f);
        else
            return yieldAmount;
    }

    public void DoCollectCrop()
    {
        CollectCrop();
    }

}
