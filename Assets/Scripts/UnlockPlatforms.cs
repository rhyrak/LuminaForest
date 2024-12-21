using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockPlatforms : MonoBehaviour
{
    public static UnlockPlatforms instance;

    [Header("Dungeon Count")]
    [SerializeField] public static int dungeonCount = 0;

    [Header("Moving Platforms")]
    [SerializeField] private GameObject[] movingPlatforms;  // Reference to the moving platforms

    private bool[] dungeonIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Ensure all platforms are initially deactivated
        foreach (var platform in movingPlatforms)
        {
            platform.SetActive(false);
        }
        Debug.Log("Dungeon Count Unlock Platforms: " + dungeonCount);
        dungeonIndex = new bool[dungeonCount];
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic if needed
    }

    // Unlock the platforms once all dungeons are completed
    private void UnlockPlatform()
    {
        foreach (var platform in movingPlatforms)
        {
            platform.SetActive(true);  // Activates each platform
        }
    }

    // Mark a dungeon as completed
    public void MarkDungeon(int index)
    {
        dungeonIndex[index] = true;

        // Check if all dungeons are completed
        for (int i = 0; i < dungeonCount; i++)
        {
            if (!dungeonIndex[i])
            {
                return;  // If any dungeon isn't completed, stop the process
            }
        }

        // All dungeons are completed, unlock the platforms
        UnlockPlatform();
    }
}
