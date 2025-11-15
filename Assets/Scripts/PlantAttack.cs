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

    void Update()
    {
        enemy = FindClosestEnemy();
    }

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

    public IEnumerator AttackRoutine(EnemyInfo enemy)
    {
        while (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
        {
            // Instantiate bullet and set its target
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            switch (bulletType)
            {
                case 0:
                    // Basic bullet logic
                    BulletInfo bulletInfo = bullet.GetComponent<BulletInfo>();
                    break;
                case 1:
                    // Piercing arking bullet logic

                    break;
                case 2:
                    // Aoe bullet logic

                    break;
            }

            yield return new WaitForSeconds(attackDelay);
        }
    }

}
