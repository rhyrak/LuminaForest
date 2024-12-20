using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    private bool isSpawning = false;
    public float spawnYLevel = 0f;
    private int currentWave = 0;
    private int enemyCount = 0;
    private int waveCount = 3;
    private int enemyIncrease = 3;

    public List<SlimeController> enemies = new List<SlimeController>();
    private List<SlimeController> activeEnemies = new List<SlimeController>();

    // Global variable to track dungeon completions
    public static int dungeonCount = 0;
    private int dungeonIndex = 0;

    private void Awake()
    {
        dungeonIndex = dungeonCount++;
    }

    void Start()
    {
        UnlockPlatforms.dungeonCount = dungeonCount;
        Debug.Log("Dungeon Count Wave Spawner: " + dungeonCount);
    }

    void Update()
    {
        if (isSpawning)
        {
            if (activeEnemies.Count == 0) // Check if all enemies are defeated
            {
                StartCoroutine(NextWaveCooldown());
            }
        }
    }

    private IEnumerator DelayedSlimeDespawn(SlimeController enemy)
    {
        yield return new WaitForSeconds(2f);
        Destroy(enemy.gameObject);
    }

    private IEnumerator NextWaveCooldown()
    {
        isSpawning = false; // Pause spawning during cooldown
        yield return new WaitForSeconds(5f); // Cooldown between waves

        if (currentWave < waveCount)
        {
            StartNextWave();
        }
        else
        {
            // Dungeon defeated logic
            DungeonDefeated();
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        enemyCount += enemyIncrease;
        SpawnWave();
        isSpawning = true; // Resume spawning
    }

    public void SpawnWave()
    {
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogError("No enemies available to spawn. Please add enemy prefabs to the 'enemies' list in the Inspector.");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            // Randomize the x position to minimize enemies spawning on top of each other
            float randomX = transform.position.x + Random.Range(-3f, 3f);

            Vector2 spawnPosition = new Vector2(randomX, spawnYLevel);

            // Spawn the enemy at the randomized position
            SlimeController enemy = Instantiate(enemies[0], spawnPosition, Quaternion.identity);
            enemy.gameObject.SetActive(true);

            // Add to activeEnemies list
            activeEnemies.Add(enemy);

            // Subscribe to OnDeath event
            enemy.OnDeath += HandleEnemyDeath;
        }
    }


    private void HandleEnemyDeath(SlimeController enemy)
    {
        // Remove the enemy from the activeEnemies list when it dies
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            StartCoroutine(DelayedSlimeDespawn(enemy));
        }
    }

    public void ActivateDungeon()
    {
        if (isSpawning)
        {
            Debug.LogWarning("Dungeon is already active. Cannot activate again.");
            return;
        }
        isSpawning = true;
        currentWave = 0; // Reset wave count if reactivating the dungeon
        enemyCount = 0; // Reset enemy count
        StartNextWave(); // Start the first wave
    }

    private void DungeonDefeated()
    {
        isSpawning = false;
        UnlockPlatforms.instance.MarkDungeon(dungeonIndex);
    }
}
