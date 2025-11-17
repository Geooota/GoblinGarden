using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TowerDefenseManager : MonoBehaviour
{
    public static TowerDefenseManager Instance { get; private set; }

    public float waveTimer;
    public float waveDuration;

    public GameObject winScreenUI;
    public GameObject loseScreenUI;

    [Header("Enemies")]
    public Object kingPrefab;
    public Object knightPrefab;
    public Object roguePrefab;
    public Object peasantPrefab;
    public int spawningCredits = 10;
    public int waveNumber = 1;
    public int enemyTypes;
    public List<Transform> spawnPoints;
    public AudioClip hitSoundNormal;
    public AudioClip hitSoundPoison;
    public AudioClip hitCompactorSound1;
    public AudioClip hitCompactorSound2;
    private AudioSource audioSource;
    public Image trashHP;

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
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(TilemapClicker.Instance.TimerTillNextWave());
    }

    public void Update()
    {
        if (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            Debug.Log(waveTimer.ToString());
        }
        if (trashHP != null)
        {
            trashHP.fillAmount = goalCurrentHealth / goalMaxHealth;
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
        waveDuration = (waveNumber * 4) + 11;
        spawningCredits = Mathf.RoundToInt(Mathf.Pow(waveNumber, 2f)) + 10;
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        if (spawningCredits <= 0)
            yield return null;
        else
        {
            float randomWait = Random.Range(0, waveDuration/3);

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

        AudioClip soundToPlay = Random.value > 0.5f ? hitCompactorSound1 : hitCompactorSound2;
        audioSource.PlayOneShot(soundToPlay);

        if (goalCurrentHealth <= 0)
        {
            Lose();
        }

        Debug.Log(damage);
    }

    public void Win()
    {
        TilemapClicker.Instance.GetGoldOrTrash(true, waveNumber * 10);
        waveNumber++;
        Debug.Log("You win!");
        goalCurrentHealth = goalMaxHealth;
        winScreenUI.SetActive(true);
    }

    public void Lose()
    {
        EnemyInfo[] enemies = FindObjectsOfType<EnemyInfo>();
        foreach (EnemyInfo enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        Debug.Log("You lose...");
        goalCurrentHealth = goalMaxHealth;
        loseScreenUI.SetActive(true);
    }

    public void EnemyHitNormal()
    {
        audioSource.PlayOneShot(hitSoundNormal);
    }

    public void EnemyHitPoison()
    {
        audioSource.PlayOneShot(hitSoundPoison);
    }
}