using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using System.Linq;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private Vector3 SpawnPoint = new(0, 0, 0);  // Base spawn point
    [SerializeField] private CinemachineVirtualCamera Camera = null;
    [SerializeField] private float respawnDelay = 2f;  // Delay before respawn
    [SerializeField] private Transform[] spawnPoints; // Array to store spawn points
    private List<int> availableSpawnIndexes = new List<int>(); // List to track available spawn points
    private HashSet<Vector3> occupiedSpawnPoints = new HashSet<Vector3>();

    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Initially, all spawn points are available
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                availableSpawnIndexes.Add(i);
            }
            // Spawn the player when the scene starts
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 spawnPosition = GetSpawnPointForPlayer(actorNumber);
            SpawnPlayerAtPosition(spawnPosition);
        }
    }

    // Handles spawning players, both initially and after respawn
    private void SpawnPlayerAtPosition(Vector3 spawnPosition)
    {
        var player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPosition, Quaternion.identity);

        // Set the camera to follow the player
        if (Camera != null)
        {
            Camera.Follow = player.transform;
        }
    }

    // Automatically respawn player after they die
    public void RespawnPlayer()
    {
        // Get the player's ActorNumber for a unique spawn point
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Vector3 respawnPosition = GetSpawnPointForPlayer(actorNumber);

        // Wait a few seconds before respawning the player
        StartCoroutine(RespawnCoroutine(respawnPosition));
    }

    // Coroutine to delay the respawn
    private IEnumerator RespawnCoroutine(Vector3 respawnPosition)
    {
        yield return new WaitForSeconds(respawnDelay); // Wait before respawn

        // Respawn the player at the spawn position
        SpawnPlayerAtPosition(respawnPosition);

        // You can add a fade-out/fade-in effect for respawn here if desired
        ScreenFXManager.instance.DisablePlayerDiedEffects();
    }

    // Calculate a spawn point offset based on the player's unique ActorNumber
    private Vector3 GetSpawnPointForPlayer(int actorNumber)
    {
        // Define four unique X-axis offsets for spawning players
        float[] xOffsets = { -3f, -1f, 1f, 3f }; // Predefined unique offsets on the X-axis

        // Determine the index for this player's spawn point
        int spawnIndex = (actorNumber - 1) % 4; // Ensure the index is between 0 and 3

        // Calculate the potential spawn position
        Vector3 potentialSpawnPosition = SpawnPoint + new Vector3(xOffsets[spawnIndex], 0, 0);

        // Check if the position is already occupied
        if (occupiedSpawnPoints.Contains(potentialSpawnPosition))
        {
            Debug.LogWarning($"Spawn position {potentialSpawnPosition} is already occupied.");
        }
        else
        {
            // Mark the position as occupied
            occupiedSpawnPoints.Add(potentialSpawnPosition);
        }

        return potentialSpawnPosition;
    }

    // This method is used to spawn a new player at an available spawn point
    public Transform GetAvailableSpawnPoint()
    {
        if (availableSpawnIndexes.Count > 0)
        {
            // Get an available spawn point index
            int spawnIndex = availableSpawnIndexes[0];
            availableSpawnIndexes.RemoveAt(0); // Remove it from available list
            return spawnPoints[spawnIndex]; // Return the transform of the spawn point
        }
        else
        {
            // If no spawn points are available, return a random one (or handle it differently)
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
    }

    // This method is used to free a spawn point when a player leaves
    public void FreeUpSpawnPoint(int spawnPointIndex)
    {
        if (!availableSpawnIndexes.Contains(spawnPointIndex))
        {
            availableSpawnIndexes.Add(spawnPointIndex); // Mark spawn point as available
        }
    }
}
