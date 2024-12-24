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

    private bool _isActivated = false;
    private int _currentWave = 0;
    private int _enemyCount = 0;
    private int _killedSlimeCount = 0;
    private const int WaveCount = 3;
    private const int EnemyIncrease = 3;


    private IEnumerator NextWaveCooldown()
    {
        yield return new WaitForSeconds(5f); // Cooldown between waves

        if (_currentWave < WaveCount)
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
        _currentWave++;
        _enemyCount += EnemyIncrease;
        SpawnWave();
    }

    private void HandleDeath(SlimeController controller)
    {
        _killedSlimeCount++;
        if (_killedSlimeCount == _currentWave * EnemyIncrease)
        {
            _killedSlimeCount = 0;
            StartCoroutine(NextWaveCooldown());
        }
    }

    private void SpawnWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("No enemies available to spawn. Please add enemy prefab to the in the Inspector.");
            return;
        }

        for (var i = 0; i < _enemyCount; i++)
        {
            // Randomize the x position to minimize enemies spawning on top of each other
            var randomX = transform.position.x + Random.Range(-3f, 3f);

            var spawnPosition = new Vector2(randomX, spawnYLevel);

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
        if (_isActivated)
        {
            Debug.LogWarning("Dungeon is already active. Cannot activate again.");
            return;
        }
        _isActivated = true;
        _currentWave = 0; // Reset wave count if reactivating the dungeon
        _enemyCount = 0; // Reset enemy count
        StartNextWave(); // Start the first wave
    }

    public void ResetDungeon()
    {
        _isActivated = false;
        _currentWave = 0;
        _enemyCount = 0;
        _killedSlimeCount = 0;
        
        if (!PhotonNetwork.IsMasterClient) return;
        var key = isTopDungeon ? ConnectionManager.TOP_DUNGEON_DEFEATED : ConnectionManager.BOTTOM_DUNGEON_DEFEATED;
        ExitGames.Client.Photon.Hashtable props = new()
        {
            { key, false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void DungeonDefeated()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var key = isTopDungeon ? ConnectionManager.TOP_DUNGEON_DEFEATED : ConnectionManager.BOTTOM_DUNGEON_DEFEATED;
        ExitGames.Client.Photon.Hashtable props = new()
        {
            { key, true }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
