using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private Vector3 SpawnPoint = new(0, 0, 0);  // Base spawn point
    [SerializeField] private CinemachineVirtualCamera Camera = null;
    [SerializeField] private float respawnDelay = 2f;  // Delay before respawn
    private HashSet<Vector3> occupiedSpawnPoints = new HashSet<Vector3>();  // Keep track of occupied positions

    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
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
            // Optionally, you can shift the spawn point or add logic to find a free position
            // For now, just use the same one (it could lead to an issue if two players are spawned
            // on top of each other, but this is fine for demonstration).
        }
        else
        {
            // Mark the position as occupied
            occupiedSpawnPoints.Add(potentialSpawnPosition);
        }

        return potentialSpawnPosition;
    }

    // This method is used to free a spawn point when a player leaves
    public void FreeUpSpawnPoint(Vector3 spawnPosition)
    {
        if (occupiedSpawnPoints.Contains(spawnPosition))
        {
            occupiedSpawnPoints.Remove(spawnPosition); // Free the spawn point
        }
    }
}