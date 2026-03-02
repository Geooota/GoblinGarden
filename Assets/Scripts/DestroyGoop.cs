using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/*
PSEUDOCODE / PLAN (detailed):

- Ensure the GameObject has a BoxCollider2D and a SpriteRenderer.
- Cache references to BoxCollider2D and SpriteRenderer in Start().
- Expose configurable fields:
  - damagePerSecond: how much damage the goop deals per second to overlapping enemies.
  - enemyLayer: optional LayerMask to restrict overlap checks to enemy layers.
  - enemyTag: optional tag string; if set, only hit objects with that tag (empty = ignore tag).
- Every Update():
  - Fade the sprite's alpha down at a fixed rate.
  - If alpha <= 0, destroy the GameObject.
  - Otherwise, query Physics2D.OverlapBoxAll using the box collider's bounds (center, size, rotation)
    and the configured enemyLayer.
  - For each overlapping collider:
    - If enemyTag is not empty and the collider's GameObject doesn't match the tag, skip it.
    - Deliver damage scaled by Time.deltaTime:
      - Use GameObject.SendMessage("TakeDamage", damageAmount, DontRequireReceiver)
        so this code remains decoupled from particular health implementations.
    - (This will repeatedly apply damage while the enemy stays in the goop.)
- Provide a gizmo to visualize the box in the editor for convenience.
*/

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class DestroyGoop : MonoBehaviour
{
    [Tooltip("SpriteRenderer used to fade out the goop.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("Damage applied per second to overlapping enemies.")]
    public float damagePerSecond = 10f;

    [Tooltip("Layer mask used to filter overlap checks (set to 'Everything' to ignore).")]
    public LayerMask enemyLayer = ~0;

    [Tooltip("If non-empty, only GameObjects with this tag will be damaged.")]
    public string enemyTag = "Enemy";

    [Tooltip("Alpha fade speed (units per second).")]
    public float fadeSpeed = 0.2f;

    private BoxCollider boxCollider;

    private List<GameObject> actorsInside = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        actorsInside.Add(other.gameObject);
    }



    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // Fade out
        Color c = spriteRenderer.color;
        c.a -= Time.deltaTime * fadeSpeed;
        spriteRenderer.color = c;

        // If invisible, destroy
        if (c.a <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float damageThisFrame = damagePerSecond * Time.deltaTime;

        foreach (GameObject actor in actorsInside)
        {
            if (actor == null) continue; // Skip destroyed objects
            actor.GetComponent<EnemyInfo>()?.TakeDamage(damageThisFrame);
            Debug.Log("Dealing " + damageThisFrame + " damage to " + actor.name);
        }
    }
}
