using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

public class JohnController : MonoBehaviour
{
    public static JohnController Instance { get; private set; }

    private float moveSpeed = 3f;
    public float stoppingDistance = 1f;

    public Coroutine walking;
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public List<Sprite> walkingSprite;
    private int index = 0;

    private Queue<Job> jobQueue = new Queue<Job>();
    private bool processing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EnqueueJob(Job job)
    {
        jobQueue.Enqueue(job);
        if (!processing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        processing = true;

        while (jobQueue.Count > 0)
        {
            Job job = jobQueue.Dequeue();
            yield return StartCoroutine(ExecuteJob(job));
        }

        processing = false;
    }

    private IEnumerator WalkingAnimation()
    {
        while (true)
        {
            spriteRenderer.sprite = walkingSprite[index];
            index = (index + 1) % walkingSprite.Count;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ExecuteJob(Job job)
    {
        // --- Move towards the target ---
        Vector3 dest = job.target != null ? job.target.transform.position : job.manualTargetPosition;
        dest.y = transform.position.y; // lock Y

        walking = StartCoroutine(WalkingAnimation());

        while (Vector3.Distance(transform.position, dest) > stoppingDistance)
        {
            Vector3 direction = (dest - transform.position).normalized;

            spriteRenderer.flipX = (direction.x + direction.y <= 0);

            transform.position += direction * moveSpeed * Time.deltaTime;

            yield return null;
        }

        StopCoroutine(walking);

        // --- Work animation phase ---
        if (job.workSprites != null && job.workSprites.Count > 0)
        {
            float frameDuration = 0.4f / job.workSprites.Count;

            foreach (Sprite frame in job.workSprites)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(frameDuration);
            }
        }
        spriteRenderer.sprite = idleSprite;
        // --- Perform the actual game action ---
        job.onComplete?.Invoke();

        // tiny buffer between jobs
        yield return new WaitForSeconds(0.05f);
    }
}

[Serializable]
public class Job
{
    public GameObject target;
    public Vector3 manualTargetPosition;
    public Action onComplete;
    public List<Sprite> workSprites;

    public Job(GameObject target, Action onComplete = null, List<Sprite> workSprites = null)
    {
        this.target = target;
        if (onComplete != null)
            this.onComplete = onComplete;
        if (workSprites != null)
            this.workSprites = workSprites;
    }

    public static Job ForPosition(Vector3 pos, Action onComplete, List<Sprite> workSprites = null)
    {
        var j = new Job(null, onComplete, workSprites);
        j.manualTargetPosition = pos;
        return j;
    }
}