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

    public GameObject UIPopUp;

    public void BeginGrowing()
    {
        StartCoroutine(GrowRoutine(growthTime));
        UIPopUp.SetActive(false);
    }

    private IEnumerator GrowRoutine(float timeToGrow)
    {
        Debug.Log("GrowTimeStarted");

        float startTime = Time.time;
        yield return new WaitForSeconds(timeToGrow);
        collectable = true;
        UIPopUp.SetActive(true);
        Debug.Log($"Plant is now collectible after {Time.time - startTime} seconds!");
    }

    public int CollectCrop()
    {
        StartCoroutine(GrowRoutine(growthTime));
        UIPopUp.SetActive(false);
        return yieldAmount;
    }

    public void DoCollectCrop()
    {
        CollectCrop();
    }

}
