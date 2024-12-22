using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UnlockPlatforms : MonoBehaviourPunCallbacks
{
    public static UnlockPlatforms instance;

    [Header("Moving Platforms")]
    [SerializeField] private GameObject[] movingPlatforms;  // Reference to the moving platforms

    private bool topDungeonDefeated = false;
    private bool bottomDungeonDefeated = false;

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
        topDungeonDefeated = (bool)PhotonNetwork.CurrentRoom.
            CustomProperties[ConnectionManager.TOP_DUNGEON_DEFEATED];
        bottomDungeonDefeated = (bool)PhotonNetwork.CurrentRoom.
            CustomProperties[ConnectionManager.BOTTOM_DUNGEON_DEFEATED];
        TryUnlockPlatforms();
    }

    // Unlock the platforms once all dungeons are completed
    private void TryUnlockPlatforms()
    {
        if (!topDungeonDefeated || !bottomDungeonDefeated)
            return;
        foreach (var platform in movingPlatforms)
        {
            platform.SetActive(true);  // Activates each platform
        }
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        var boss = PhotonNetwork.InstantiateRoomObject("bossSlime", new Vector3(57, -2.9f, 0), Quaternion.identity);
        if (boss != null)
        {
            boss.GetComponent<SlimeController>().OnDeath += delegate (SlimeController obj)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    ExitGames.Client.Photon.Hashtable props = new()
                    {
                        { ConnectionManager.BOSS_DEFEATED, true }
                    };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                }
            };
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable props)
    {
        var top = props[ConnectionManager.TOP_DUNGEON_DEFEATED];
        var bottom = props[ConnectionManager.BOTTOM_DUNGEON_DEFEATED];
        if (top != null)
        {
            topDungeonDefeated = (bool)top;
        }
        if (bottom != null)
        {
            bottomDungeonDefeated = (bool)bottom;
        }
        // prevent other room prop updates to trigger this logic
        if (top != null || bottom != null)
        {
            TryUnlockPlatforms();
        }
    }

}
