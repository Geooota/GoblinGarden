using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;

public class TowerDefenseManager : MonoBehaviour
{
    public static TowerDefenseManager Instance { get; private set; }

    public float waveTimer;
    public float waveDuration;

    [Header("Enemies")]
    public Object kingPrefab;
    public Object knightPrefab;
    public Object roguePrefab;
    public Object peasantPrefab;
    public int spawningCredits = 10;
    public int waveNumber = 1;
    public int enemyTypes;
    public List<Transform> spawnPoints;

    [Header("Compactor")]
    public GameObject goal;
    public int goalMaxHealth;
    public float goalCurrentHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // optional: persists across scenes
    }

    public void Start()
    {
        TilemapClicker.Instance.EnterDefenseMode();
    }

    public void Update()
    {
        if (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            Debug.Log(waveTimer.ToString());
        }
    }



    public void Wave()
    {
        switch (waveNumber)
        {
            case >= 0 and <= 3:
                enemyTypes = 2;
                break;

            case >= 4 and <= 8:
                enemyTypes = 3;
                break;

            case >= 9:
                enemyTypes = 4;
                break;
        }
        waveDuration = waveNumber * 10;
        spawningCredits = (waveNumber ^ 2) + 10;
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        if (spawningCredits <= 0)
            yield return null;
        else
        {
            float randomWait = Random.Range(0, waveDuration);

            int random = Random.Range(0, enemyTypes);
            Object prefabToSpawn = null;
            int cost = 0;

            switch (random)
            {
                case 3:
                    prefabToSpawn = kingPrefab;
                    cost = 5;
                    break;
                case 2:
                    prefabToSpawn = knightPrefab;
                    cost = 3;
                    break;
                case 1:
                    prefabToSpawn = roguePrefab;
                    cost = 2;
                    break;
                case 0:
                    prefabToSpawn = peasantPrefab;
                    cost = 1;
                    break;
            }

            if (spawningCredits >= cost)
            {
                Instantiate(prefabToSpawn, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity);
                spawningCredits -= cost;
                yield return new WaitForSeconds(randomWait);
                waveDuration -= randomWait;
            }

            StartCoroutine(SpawnEnemy());
        }
    }

    public void DamageTrash(int damage)
    {
        goalCurrentHealth -= damage;
        Debug.Log(damage);
    }

    public void Win()
    {
        waveNumber++;
        Debug.Log("You win!");
        TilemapClicker.Instance.ExitDefenseMode();
    }

    public void Lose()
    {
        EnemyInfo[] enemies = FindObjectsOfType<EnemyInfo>();
        foreach (EnemyInfo enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        Debug.Log("You lose...");
        TilemapClicker.Instance.ExitDefenseMode();
    }

}