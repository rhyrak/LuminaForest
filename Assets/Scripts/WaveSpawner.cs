using System.Collections;
using Photon.Pun;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    private float spawnYLevel = 0f;
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private bool isTopDungeon;

    private bool isActivated = false;
    private int currentWave = 0;
    private int enemyCount = 0;
    private int killedSlimeCount = 0;
    private int waveCount = 3;
    private int enemyIncrease = 3;


    private IEnumerator NextWaveCooldown()
    {
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
    }

    private void HandleDeath(SlimeController controller)
    {
        killedSlimeCount++;
        if (killedSlimeCount == currentWave * enemyIncrease)
        {
            killedSlimeCount = 0;
            StartCoroutine(NextWaveCooldown());
        }
    }

    public void SpawnWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No enemies available to spawn. Please add enemy prefab to the in the Inspector.");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            // Randomize the x position to minimize enemies spawning on top of each other
            float randomX = transform.position.x + Random.Range(-3f, 3f);

            Vector2 spawnPosition = new Vector2(randomX, spawnYLevel);

            // Spawn the enemy at the randomized position
            var enemy = PhotonNetwork.InstantiateRoomObject(enemyPrefab.name, spawnPosition, Quaternion.identity);
            if (enemy != null) // enemy is null if caller is not master client
            {
                enemy.GetComponent<SlimeController>().OnDeath += HandleDeath;
            }
        }
    }

    public void ActivateDungeon()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isActivated)
        {
            Debug.LogWarning("Dungeon is already active. Cannot activate again.");
            return;
        }
        isActivated = true;
        currentWave = 0; // Reset wave count if reactivating the dungeon
        enemyCount = 0; // Reset enemy count
        StartNextWave(); // Start the first wave
    }

    public void ResetDungeon()
    {
        isActivated = false;
        currentWave = 0;
        enemyCount = 0;
        killedSlimeCount = 0;
        if (PhotonNetwork.IsMasterClient)
        {
            var key = isTopDungeon ? ConnectionManager.TOP_DUNGEON_DEFEATED : ConnectionManager.BOTTOM_DUNGEON_DEFEATED;
            ExitGames.Client.Photon.Hashtable props = new()
            {
                { key, false }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    private void DungeonDefeated()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var key = isTopDungeon ? ConnectionManager.TOP_DUNGEON_DEFEATED : ConnectionManager.BOTTOM_DUNGEON_DEFEATED;
            ExitGames.Client.Photon.Hashtable props = new()
            {
                { key, true }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }
}
