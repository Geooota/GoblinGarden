using System.Collections;
using UnityEngine;

public class PlantAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackDamage;
    public float attackDelay;
    public float attackRange;
    public int bulletpierce;
    public int bulletType;
    public GameObject bulletPrefab;
    private EnemyInfo enemy;

    private EnemyInfo FindClosestEnemy()
    {
        EnemyInfo[] enemies = FindObjectsOfType<EnemyInfo>();
        EnemyInfo closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyInfo enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= attackRange)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public IEnumerator AttackRoutine()
    {
        enemy = FindClosestEnemy();

        if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
        {
            // Instantiate bullet and set its target
            GameObject bullet = Instantiate(bulletPrefab, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity);

            BulletInfo bulletInfo = bullet.GetComponent<BulletInfo>();
            bulletInfo.SetTarget(enemy.transform, attackDamage, bulletpierce, bulletType);

            yield return new WaitForSeconds(attackDelay);
        }

        yield return null;
        StartCoroutine(AttackRoutine());
    }
}
