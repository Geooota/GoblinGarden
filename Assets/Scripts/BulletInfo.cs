using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class BulletInfo : MonoBehaviour
{
    public float speed = 10f;
    private Transform target;
    private float damage;
    private int pierce;
    private int type;
    public float bulletDuration;

    public void Start()
    {
        StartCoroutine(DestroyBulletAfterTime(bulletDuration));
    }

    public void SetTarget(Transform target, float damage, int pierce, int type)
    {
        this.target = target;
        this.damage = damage;
        this.pierce = pierce;
        this.type = type;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // rotate in the direction of movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90f, 0f, angle - 10f);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            EnemyInfo enemyInfo = target.GetComponent<EnemyInfo>();
            if (enemyInfo != null)
            {
                if (type == 0) // Basic bullet logic
                {
                    enemyInfo.TakeDamage(damage);
                    pierce--;
                    TowerDefenseManager.Instance.EnemyHitNormal();

                    if (pierce <= 0)
                    {
                        Destroy(gameObject, 10f);
                    }
                }
                else if (type == 1)
                {
                    enemyInfo.TakeDamage(damage);
                    pierce--;
                    TowerDefenseManager.Instance.EnemyHitNormal();

                    if (pierce <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
                else if (type == 2) // AOE bullet logic
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
                    foreach (var hitCollider in hitColliders)
                    {
                        EnemyInfo nearbyEnemy = hitCollider.GetComponent<EnemyInfo>();
                        if (nearbyEnemy != null)
                        {
                            nearbyEnemy.PoisonTimer = 5;
                            nearbyEnemy.Poison();
                        }
                    }
                    TowerDefenseManager.Instance.EnemyHitPoison();
                    Destroy(gameObject);
                }
            }
        }
    }

    private IEnumerator DestroyBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
