using Photon.Pun;
using UnityEngine;

public class DungeonActivator : MonoBehaviour
{
    public WaveSpawner waveSpawner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!collision.CompareTag("Player")) return;
        if (waveSpawner == null) return;
        waveSpawner.ActivateDungeon();
        Debug.Log("Dungeon Activated!");
    }
}
