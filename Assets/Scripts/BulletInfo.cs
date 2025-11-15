using UnityEngine;
using UnityEngine.Rendering;

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
        // Optionally, you can add a self-destruct timer to prevent bullets from existing indefinitely
        Destroy(gameObject, bulletDuration);
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
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            EnemyInfo enemyInfo = target.GetComponent<EnemyInfo>();
            if (enemyInfo != null)
            {
                if (type == 0) // Basic bullet logic
                {
                    enemyInfo.health -= damage;
                    pierce--;

                    if (pierce <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
                else if (type == 1) // Piercing arking bullet logic
                {
                    enemyInfo.health -= damage;
                    pierce--;

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
                    Destroy(gameObject);
                }
            }
        }
    }
}
