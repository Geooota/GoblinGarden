using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EnemyInfo : MonoBehaviour
{
    [Header("Enemy Stats")]
    private float health;
    private float moveSpeed;
    public float initialMoveSpeed;
    public float attackDelay;
    public float maxHealth;
    public int attackDamage;
    public Image fillImage;

    public int PoisonTimer = 5;
    public SpriteRenderer spriteRenderer;
    public List<Sprite> walkingSprite;
    public List<Sprite> attackSprites;
    public Sprite idleSprite;
    private int index = 0;
    private Coroutine walking;
    public float stoppingDistance = 1f;
    public Transform compactorTransform;

    // Tracks goop colliders currently slowing this enemy.
    // Each overlapping goop multiplies speed by 0.7.
    private readonly HashSet<Collider> overlappingGoops = new HashSet<Collider>();


    public void Start()
    {
        moveSpeed = initialMoveSpeed;
        health = maxHealth;
        fillImage.fillAmount = health / maxHealth;
        compactorTransform = TilemapClicker.Instance.trashCompactor.transform;
        StartCoroutine(Attack());
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

    private IEnumerator Attack()
    {
        // --- Move towards the target ---
        Vector3 dest = compactorTransform.position;
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
        if (attackSprites != null && attackSprites.Count > 0)
        {
            while (health > 0 && TowerDefenseManager.Instance.goalCurrentHealth > 0)
            {
                float frameDuration = 0.4f / attackSprites.Count;

                foreach (Sprite frame in attackSprites)
                {
                    spriteRenderer.sprite = frame;
                    yield return new WaitForSeconds(frameDuration);
                }

                TowerDefenseManager.Instance.DamageTrash(attackDamage);

                yield return new WaitForSeconds(attackDelay);
            }

        }
        spriteRenderer.sprite = idleSprite;

        // tiny buffer between jobs
        yield return new WaitForSeconds(0.05f);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            EnemyInfo[] enemies = FindObjectsOfType<EnemyInfo>();
            if (enemies.Length == 1 && TowerDefenseManager.Instance.spawningCredits <= 0)
                TowerDefenseManager.Instance.Win();
            Destroy(gameObject);
        }
        Debug.Log("took damage " + damage);
        Debug.Log("health left " + health / maxHealth);
        fillImage.fillAmount = health / maxHealth;
    }

    // Called when entering trigger colliders (requires this GameObject/Rigidbody2D + collider setup).
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        // detect goop by component (keeps coupling low)
        if (other.GetComponent<DestroyGoop>() != null)
        {
            if (overlappingGoops.Add(other))
            {
                RecalculateMoveSpeed();
            }
        }
    }

    // Called when leaving trigger colliders.
    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.GetComponent<DestroyGoop>() != null)
        {
            if (overlappingGoops.Remove(other))
            {
                RecalculateMoveSpeed();
            }
        }
    }

    private void RecalculateMoveSpeed()
    {
        // Each goop multiplies speed by 0.7
        int count = overlappingGoops.Count;
        moveSpeed = initialMoveSpeed * Mathf.Pow(0.7f, count);
    }
}
