using UnityEngine;

public class DungeonActivator : MonoBehaviour
{
    public WaveSpawner waveSpawner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player entered the trigger
        if (collision.CompareTag("Player"))
        {
            if (waveSpawner != null)
            {
                waveSpawner.ActivateDungeon();
                Debug.Log("Dungeon Activated!");
            }
        }
    }
}
