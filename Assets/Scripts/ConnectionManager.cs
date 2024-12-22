using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField RoomNameInputField;
    [SerializeField]
    private RectTransform RoomList;
    [SerializeField]
    private GameObject RoomTilePrefab;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public static ConnectionManager instance;

    public static readonly string TOP_DUNGEON_DEFEATED = "t";
    public static readonly string BOTTOM_DUNGEON_DEFEATED = "b";
    public static readonly string BOSS_DEFEATED = "g";

    public void Start()
    {
        if (instance == null) instance = this;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        cachedRoomList.Clear();
    }

    public void CreateRoom()
    {
        Photon.Realtime.RoomOptions roomOptions = new();
        roomOptions.MaxPlayers = 4;
        ExitGames.Client.Photon.Hashtable props = new()
            {
                { TOP_DUNGEON_DEFEATED, false },
                { BOTTOM_DUNGEON_DEFEATED, false },
                { BOSS_DEFEATED, false },
            };
        roomOptions.CustomRoomProperties = props;
        PhotonNetwork.CreateRoom(RoomNameInputField.text, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        PhotonNetwork.LoadLevel("GameScene MP");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform child in RoomList)
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
        foreach (var room in cachedRoomList)
        {
            var tile = Instantiate(RoomTilePrefab);
            tile.GetComponent<RoomTile>().SetValues(room.Value);
            tile.transform.parent = RoomList;
            tile.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
    }
}
