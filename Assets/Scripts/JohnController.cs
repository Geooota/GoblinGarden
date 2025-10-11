using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class JohnController : MonoBehaviour
{
    public static JohnController Instance { get; private set; }

    private float moveSpeed = 3f;
    private float stoppingDistance = 2f;


    public SpriteRenderer spriteRenderer;
    private Sprite idleSprite;
    private Sprite workingSprite;
    private float workSpriteDuration = 0.4f;

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

    private IEnumerator ExecuteJob(Job job)
    {
        // --- Move towards the target ---
        Vector3 dest = job.target != null ? job.target.transform.position : job.manualTargetPosition;
        dest.y = transform.position.y;
        while (Vector3.Distance(transform.position, dest) > stoppingDistance)
        {
            Vector3 direction = (dest - transform.position).normalized;

            // Determine sprite flip robustly for isometric camera (45�)
            // If movement is generally toward right-down, face right, else left
            if (direction.x + direction.y > 0)
                spriteRenderer.flipX = false; // facing bottom-right
            else
                spriteRenderer.flipX = true;  // facing top-left

            transform.position += direction * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // --- "Work" phase: swap sprite briefly ---
        if (spriteRenderer != null && workingSprite != null)
        {
            spriteRenderer.sprite = workingSprite;
            yield return new WaitForSeconds(workSpriteDuration);
            spriteRenderer.sprite = idleSprite;
        }

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

    public Job(GameObject target, Action onComplete)
    {
        this.target = target;
        this.onComplete = onComplete;
    }

    public static Job ForPosition(Vector3 pos, Action onComplete)
    {
        var j = new Job(null, onComplete);
        j.manualTargetPosition = pos;
        return j;
    }
}

