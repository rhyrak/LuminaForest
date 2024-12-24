using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UnlockPlatforms : MonoBehaviourPunCallbacks
{
    [Header("Moving Platforms")]
    [SerializeField] private GameObject[] movingPlatforms;  // Reference to the moving platforms

    public static UnlockPlatforms Instance;
    
    private bool _topDungeonDefeated = false;
    private bool _bottomDungeonDefeated = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        // Ensure all platforms are initially deactivated
        foreach (var platform in movingPlatforms)
        {
            platform.SetActive(false);
        }
        _topDungeonDefeated = (bool)PhotonNetwork.CurrentRoom.
            CustomProperties[ConnectionManager.TOP_DUNGEON_DEFEATED];
        _bottomDungeonDefeated = (bool)PhotonNetwork.CurrentRoom.
            CustomProperties[ConnectionManager.BOTTOM_DUNGEON_DEFEATED];
        TryUnlockPlatforms();
    }

    // Unlock the platforms once all dungeons are completed
    private void TryUnlockPlatforms()
    {
        if (!_topDungeonDefeated || !_bottomDungeonDefeated)
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
                if (!PhotonNetwork.IsMasterClient) return;
                ExitGames.Client.Photon.Hashtable props = new()
                {
                    { ConnectionManager.BOSS_DEFEATED, true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            };
        }
    }

    public IEnumerator ResetBoss()
    {
        var dungeons = new List<WaveSpawner>(FindObjectsOfType<WaveSpawner>());
        foreach (var dungeon in dungeons)
        {
            dungeon.ResetDungeon();
        }
        foreach (var platform in movingPlatforms)
        {
            platform.SetActive(false);  // De-Activate each platform
        }

        yield return new WaitForSeconds(5f);

        if (!PhotonNetwork.IsMasterClient) yield break;
        ExitGames.Client.Photon.Hashtable props = new()
        {
            { ConnectionManager.BOSS_DEFEATED, false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable props)
    {
        var top = props[ConnectionManager.TOP_DUNGEON_DEFEATED];
        var bottom = props[ConnectionManager.BOTTOM_DUNGEON_DEFEATED];
        if (top != null)
        {
            _topDungeonDefeated = (bool)top;
        }
        if (bottom != null)
        {
            _bottomDungeonDefeated = (bool)bottom;
        }
        // prevent other room prop updates to trigger this logic
        if (top != null || bottom != null)
        {
            TryUnlockPlatforms();
        }
    }

}
