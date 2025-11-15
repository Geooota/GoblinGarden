using UnityEngine;

public class TowerDefenseManager : MonoBehaviour
{
    public static TowerDefenseManager Instance { get; private set; }

    [Header("Enemies")]
    public Object kingPrefab;
    public Object knightPrefab;
    public Object roguePrefab;
    public Object peasantPrefab;
    public int spawningCredits = 10;
    public int waveNumber = 1;
    public int enemyTypes;

    [Header("Compactor")]
    public Object goalPrefab;
    public int goalMaxHealth;
    public float goalCurrentHealth;

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

            case >= 9 and <= 12:
                enemyTypes = 4;
                break;
        }

        spawningCredits = (waveNumber ^ 2) + 10;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (spawningCredits <= 0)
            return;

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
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            spawningCredits -= cost;
        }

        SpawnEnemy();
    }

    public void Win()
    {
        waveNumber++;
        Debug.Log("You win!");
        TilemapClicker.Instance.ExitDefenseMode();
    }

    public void Lose()
    {
        Debug.Log("You lose...");
        TilemapClicker.Instance.ExitDefenseMode();
    }
}
