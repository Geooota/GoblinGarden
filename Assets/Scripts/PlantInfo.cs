using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlantInfo : MonoBehaviour
{
    public string plantName;
    public int cost;
    public float growthTime; // in seconds
    public float growthCycleTime; // in seconds
    public int yieldAmount;
    public bool collectable;
    public bool dry;
    public Vector3Int myCellPos;
    public PlotInfo myPlot; 
    public PlotType myPlotType;
    private float startTime;
    private float timeLeft;
    public bool wasCollectable;

    private Coroutine growRoutine;
    private Coroutine spriteRoutine;

    public GameObject UIPopUp;
    public GameObject spriteHolder;
    public Sprite plantSprite0;
    public Sprite plantSprite1;
    public Sprite plantSprite2;
    public Sprite plantSprite3;

    public AudioClip[] trashSounds;
    private AudioSource audioSource;

    [Header("Grow Cycle")]
    public float switchInterval = 45f; // seconds

    public SpriteRenderer spriteRenderer;
    private int switchState = 0;

    private void Start()
    {
        GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer.sprite = plantSprite0;
    }

    public void BeginGrowing()
    {
        growRoutine = StartCoroutine(GrowRoutine(growthTime));
        UIPopUp.SetActive(false);
    }

    private IEnumerator GrowRoutine(float timeToGrow)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("GrowTimeStarted");
        startTime = Time.time;

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
        PlayRandomHarvestSound();
        StartCoroutine(GrowRoutine(growthTime));
        if (myPlotType == PlotType.Golden)
            return Mathf.RoundToInt(yieldAmount * 1.3f);
        else
            return yieldAmount;
    }



    public void StartGrowthCycle()
    {
        spriteRoutine = StartCoroutine(SwitchSpriteRoutine());
    }

    private IEnumerator SwitchSpriteRoutine()
    {
        while (switchState < 3)
        {
            yield return new WaitForSeconds(switchInterval);
            NextGrowthState();
        }
    }

    private void NextGrowthState()
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
                BeginGrowing();
                AnimSprite();
                break;
        }
    }
    public void DryOut()
    {
        Debug.Log($"Plant " + this + " dried");
        if (collectable)
        {
            wasCollectable = true;
            collectable = false;
            UIPopUp.SetActive(false);
        }
        else if (switchState == 3)
        {
            timeLeft = Time.time - startTime;
            StopAllCoroutines();
        }
        else
        {
            Debug.Log("Sprite Routine Stopped");
            StopAllCoroutines();
        }
    }

    public void Wet()
    {
        if (wasCollectable)
        {
            wasCollectable = false;
            collectable = true;
            UIPopUp.SetActive(true);
        }
        else if (timeLeft != 0)
        {
            growRoutine = StartCoroutine(GrowRoutine(timeLeft));
        }
        else
        {
            spriteRoutine = StartCoroutine(SwitchSpriteRoutine());
        }
    }

    public void PlayRandomHarvestSound()
    {
        if (trashSounds.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned to the array!");
            return;
        }

        int randomIndex = Random.Range(0, trashSounds.Length);
        audioSource.PlayOneShot(trashSounds[randomIndex]);
    }

    private float animRotation;
    private float animHeight;
    public IEnumerator AnimSprite()
    {
        yield return null;
    }
}