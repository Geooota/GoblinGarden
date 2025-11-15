using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float health;
    public float moveSpeed;
    public float attackSpeed;
    public float maxHealth;
    public float attackDamage;


    public SpriteRenderer spriteRenderer;
    public List<Sprite> walkingSprite;
    public Sprite idleSprite;
    private int index = 0;
    private Coroutine walking;
    public float stoppingDistance = 1f;
    public Transform compactorTransform;

    private IEnumerator WalkingAnimation()
    {
        while (true)
        {
            spriteRenderer.sprite = walkingSprite[index];
            index = (index + 1) % walkingSprite.Count;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Attack()
    {
        // --- Move towards the target ---
        Vector3 dest = job.target != null ? job.target.transform.position : job.manualTargetPosition;
        dest.y = transform.position.y;

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
