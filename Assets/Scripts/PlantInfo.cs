using System.Collections;
using UnityEngine;

public class PlantInfo : MonoBehaviour
{
    public string plantName;
    public int cost;
    public float growthTime; // in seconds
    public float growthCycleTime; // in seconds
    public int yieldAmount;
    public bool collectable;
    public Vector3Int myCellPos;
    public PlotType myPlotType;

    public GameObject UIPopUp;
    public Sprite plantSprite0;
    public Sprite plantSprite1;
    public Sprite plantSprite2;
    public Sprite plantSprite3;

    /* Ignore
    public float switchInterval = 45f; // seconds

    private SpriteRenderer spriteRenderer;
    private int switchState = 0;
    private Coroutine switchCoroutine;
    */

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


    /* Ignore
    public void StartGrothCyle()
    {
        StartCoroutine(GrowRoutine(growthCycleTime));
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = plantSprite0;
        switchCoroutine = StartCoroutine(SwitchSpriteRoutine());
    }

    private IEnumerator SwitchSpriteRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);
            NextGrowthState();
        }
    }

    public void NextGrowthState()
    {
        switchState++;
        if (switchState > 3)
            switchState = 3; // Stay at the last growth stage

        switch (switchState)
        {
            case 0:
                spriteRenderer.sprite = plantSprite0;
                break;
            case 1:
                spriteRenderer.sprite = plantSprite1;
                break;
            case 2:
                spriteRenderer.sprite = plantSprite2;
                break;
            case 3:
                spriteRenderer.sprite = plantSprite3;
                break;
        }
    }
    */
}